using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingInventory.Api.DTOs;
using BookingInventory.Api.Models;
using BookingInventory.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingInventory.Api.Data;

namespace BookingInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly BookingDbContext _context;
    private readonly IBookingService _bookingService;

    public BookingsController(BookingDbContext context, IBookingService bookingService)
    {
        _context = context;
        _bookingService = bookingService;
    }

    /// <summary>
    /// Creates a new booking with comprehensive validation:
    /// - Checks for overlapping bookings
    /// - Validates guest count against room capacity (with overbooking exception)
    /// - Calculates total price based on nightly rates
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> CreateBooking(CreateBookingRequest request)
    {
        // Validate input
        if (request.CheckIn >= request.CheckOut)
            return BadRequest(new ErrorResponse
            {
                ErrorCode = "INVALID_DATES",
                Message = "CheckIn must be before CheckOut"
            });

        if (request.GuestCount <= 0)
            return BadRequest(new ErrorResponse
            {
                ErrorCode = "INVALID_GUEST_COUNT",
                Message = "Guest count must be greater than 0"
            });

        // Get the room and hotel info
        var room = await _context.Rooms
            .Include(r => r.Hotel)
            .FirstOrDefaultAsync(r => r.Id == request.RoomId);

        if (room == null)
            return NotFound(new ErrorResponse
            {
                ErrorCode = "ROOM_NOT_FOUND",
                Message = "Room not found"
            });

        // Check availability
        var (isAvailable, availabilityReason) = await _bookingService.CheckAvailabilityAsync(
            request.RoomId, request.CheckIn, request.CheckOut);

        if (!isAvailable)
            return StatusCode(409, new ErrorResponse
            {
                ErrorCode = "BOOKING_OVERLAP",
                Message = availabilityReason ?? "Room is not available for the requested dates"
            });

        // Check guest count
        bool isOverCapacity = false;
        if (request.GuestCount > room.Capacity)
        {
            if (!room.Hotel!.AllowOverbooking || request.GuestCount > room.Capacity + 1)
            {
                return StatusCode(422, new ErrorResponse
                {
                    ErrorCode = "OVER_CAPACITY",
                    Message = $"Room capacity is {room.Capacity}. Overbooking allowed: {room.Hotel.AllowOverbooking}"
                });
            }
            isOverCapacity = true;
        }

        // Calculate total price
        var (totalPrice, priceError) = await _bookingService.CalculateTotalPriceAsync(
            request.RoomId, request.CheckIn, request.CheckOut);

        if (!string.IsNullOrEmpty(priceError))
            return BadRequest(new ErrorResponse
            {
                ErrorCode = "RATE_CALCULATION_ERROR",
                Message = priceError
            });

        // Create the booking
        var booking = new Booking
        {
            RoomId = request.RoomId,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            GuestCount = request.GuestCount,
            TotalPrice = totalPrice,
            IsOverCapacity = isOverCapacity,
            IsCompleted = false,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateBooking), new BookingResponse
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            GuestCount = booking.GuestCount,
            TotalPrice = booking.TotalPrice,
            IsOverCapacity = booking.IsOverCapacity
        });
    }

    /// <summary>
    /// Lists all bookings.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingResponse>>> GetBookings()
    {
        var bookings = await _context.Bookings
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(bookings.Select(b => new BookingResponse
        {
            Id = b.Id,
            RoomId = b.RoomId,
            CheckIn = b.CheckIn,
            CheckOut = b.CheckOut,
            GuestCount = b.GuestCount,
            TotalPrice = b.TotalPrice,
            IsOverCapacity = b.IsOverCapacity,
            IsCompleted = b.IsCompleted,
            IsCancelled = b.IsCancelled
        }));
    }

    /// <summary>
    /// Gets availability for a room over a date range
    /// </summary>
    [HttpGet("rooms/{roomId}/availability")]
    [HttpGet("availability")]
    public async Task<ActionResult<AvailabilityResponse>> GetAvailability(
        [FromRoute] int? roomId, [FromQuery] int? roomIdQuery, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var resolvedRoomId = roomId ?? roomIdQuery ?? 0;
        if (resolvedRoomId <= 0)
            return BadRequest(new ErrorResponse
            {
                ErrorCode = "INVALID_ROOM_ID",
                Message = "Room ID is required"
            });

        var room = await _context.Rooms.FindAsync(resolvedRoomId);
        if (room == null)
            return NotFound(new ErrorResponse
            {
                ErrorCode = "ROOM_NOT_FOUND",
                Message = "Room not found"
            });

        var (isAvailable, reason) = await _bookingService.CheckAvailabilityAsync(resolvedRoomId, from, to);

        return Ok(new AvailabilityResponse
        {
            RoomId = resolvedRoomId,
            CheckIn = from,
            CheckOut = to,
            IsAvailable = isAvailable,
            Reason = reason
        });
    }

    /// <summary>
    /// Soft-cancels a booking instead of hard deleting it.
    /// Completed bookings are marked as cancelled and remain visible for audit/history.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var booking = await _bookingService.GetBookingAsync(id);
        if (booking == null)
            return NotFound(new ErrorResponse
            {
                ErrorCode = "BOOKING_NOT_FOUND",
                Message = "Booking not found"
            });

        if (booking.IsCancelled)
            return NoContent();

        booking.IsCancelled = true;
        if (booking.CheckOut <= DateTime.UtcNow)
        {
            booking.IsCompleted = true;
        }

        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Cancels a booking if CheckIn is more than 48 hours in the future
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var booking = await _bookingService.GetBookingAsync(id);
        if (booking == null)
            return NotFound(new ErrorResponse
            {
                ErrorCode = "BOOKING_NOT_FOUND",
                Message = "Booking not found"
            });

        if (booking.IsCancelled)
            return BadRequest(new ErrorResponse
            {
                ErrorCode = "ALREADY_CANCELLED",
                Message = "Booking is already cancelled"
            });

        var canCancel = await _bookingService.CanCancelBookingAsync(id);
        if (!canCancel)
        {
            var hoursUntilCheckIn = (booking.CheckIn - DateTime.UtcNow).TotalHours;
            return StatusCode(422, new ErrorResponse
            {
                ErrorCode = "CANCELLATION_NOT_ALLOWED",
                Message = $"Booking can only be cancelled if CheckIn is more than 48 hours in the future. Hours until CheckIn: {hoursUntilCheckIn:F1}"
            });
        }

        booking.IsCancelled = true;
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
