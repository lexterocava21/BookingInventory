using System;
using System.Linq;
using System.Threading.Tasks;
using BookingInventory.Api.Controllers;
using BookingInventory.Api.Data;
using BookingInventory.Api.Models;
using BookingInventory.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingInventory.Tests;

public class BookingDeleteTests
{
    [Fact]
    public async Task DeleteBooking_DeletesBookingRecord_WhenBookingExists()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new BookingDbContext(options);
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { Hotel = hotel, Number = "101", Capacity = 2 };
        var booking = new Booking
        {
            Room = room,
            CheckIn = DateTime.UtcNow.AddDays(10),
            CheckOut = DateTime.UtcNow.AddDays(12),
            GuestCount = 2,
            TotalPrice = 200,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);
        var controller = new BookingsController(context, service);

        var result = await controller.DeleteBooking(booking.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Bookings.Where(b => b.Id == booking.Id));
    }
}
