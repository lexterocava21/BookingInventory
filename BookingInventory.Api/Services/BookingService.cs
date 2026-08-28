using BookingInventory.Api.Data;
using BookingInventory.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingInventory.Api.Services;

public interface IBookingService
{
    Task<(decimal TotalPrice, string? Error)> CalculateTotalPriceAsync(int roomId, DateTime checkIn, DateTime checkOut);
    Task<(bool IsAvailable, string? Reason)> CheckAvailabilityAsync(int roomId, DateTime checkIn, DateTime checkOut);
    Task<Booking?> GetBookingAsync(int id);
    Task<bool> CanCancelBookingAsync(int id);
}

public class BookingService : IBookingService
{
    private readonly BookingDbContext _context;

    public BookingService(BookingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Calculates the total price for a booking by determining the rate active on each night
    /// and summing them up. CheckOut is exclusive (not included in the stay).
    /// </summary>
    public async Task<(decimal TotalPrice, string? Error)> CalculateTotalPriceAsync(
        int roomId, DateTime checkIn, DateTime checkOut)
    {
        if (checkIn >= checkOut)
            return (0, "CheckIn must be before CheckOut");

        var rates = await _context.RateHistories
            .Where(r => r.RoomId == roomId)
            .OrderBy(r => r.EffectiveDate)
            .ToListAsync();

        if (!rates.Any())
            return (0, "No rate history found for the room");

        decimal totalPrice = 0;
        var currentDate = checkIn.Date;
        var checkOutDate = checkOut.Date;

        while (currentDate < checkOutDate)
        {
            // Find the rate active on currentDate (most recent rate <= currentDate)
            var activeRate = rates
                .Where(r => r.EffectiveDate.Date <= currentDate)
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefault();

            if (activeRate == null)
                return (0, $"No rate found for date {currentDate:yyyy-MM-dd}");

            totalPrice += activeRate.BaseRate;
            currentDate = currentDate.AddDays(1);
        }

        return (totalPrice, null);
    }

    /// <summary>
    /// Checks if a room is available for the given date range.
    /// Overlapping bookings are not allowed, except:
    /// - A booking ending on date X and a booking starting on date X are NOT overlapping
    /// - Only non-cancelled bookings are considered
    /// </summary>
    public async Task<(bool IsAvailable, string? Reason)> CheckAvailabilityAsync(
        int roomId, DateTime checkIn, DateTime checkOut)
    {
        if (checkIn >= checkOut)
            return (false, "CheckIn must be before CheckOut");

        var overlappingBooking = await _context.Bookings
            .Where(b => b.RoomId == roomId && !b.IsCancelled)
            .Where(b => b.CheckIn < checkOut && b.CheckOut > checkIn)
            .FirstOrDefaultAsync();

        if (overlappingBooking != null)
            return (false, $"Room is already booked from {overlappingBooking.CheckIn:yyyy-MM-dd} to {overlappingBooking.CheckOut:yyyy-MM-dd}");

        return (true, null);
    }

    public async Task<Booking?> GetBookingAsync(int id)
    {
        return await _context.Bookings.FindAsync(id);
    }

    public bool IsBookingCompleted(Booking booking)
    {
        return booking.IsCompleted || booking.CheckOut <= DateTime.UtcNow;
    }

    /// <summary>
    /// A booking can only be cancelled if CheckIn is more than 48 hours in the future.
    /// </summary>
    public async Task<bool> CanCancelBookingAsync(int id)
    {
        var booking = await GetBookingAsync(id);
        if (booking == null || booking.IsCancelled)
            return false;

        var hoursUntilCheckIn = (booking.CheckIn - DateTime.UtcNow).TotalHours;
        return hoursUntilCheckIn > 48;
    }
}
