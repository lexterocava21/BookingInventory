using BookingInventory.Api.Data;
using BookingInventory.Api.Models;
using BookingInventory.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingInventory.Tests;

public class BookingServiceAvailabilityTests
{
    private BookingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BookingDbContext(options);
    }

    [Fact]
    public async Task CheckAvailability_NoBookings_IsAvailable()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act
        var (isAvailable, reason) = await service.CheckAvailabilityAsync(1,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 5));

        // Assert
        Assert.True(isAvailable);
        Assert.Null(reason);
    }

    [Fact]
    public async Task CheckAvailability_OverlappingBooking_NotAvailable()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };
        var booking = new Booking
        {
            RoomId = 1,
            CheckIn = new DateTime(2024, 1, 3),
            CheckOut = new DateTime(2024, 1, 6),
            GuestCount = 1,
            TotalPrice = 300,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: Try to book Jan 1-5, which overlaps with Jan 3-6
        var (isAvailable, reason) = await service.CheckAvailabilityAsync(1,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 5));

        // Assert
        Assert.False(isAvailable);
        Assert.NotNull(reason);
    }

    [Fact]
    public async Task CheckAvailability_BackToBackBookings_IsAvailable()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };
        var booking = new Booking
        {
            RoomId = 1,
            CheckIn = new DateTime(2024, 1, 1),
            CheckOut = new DateTime(2024, 1, 3),
            GuestCount = 1,
            TotalPrice = 200,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: Try to book Jan 3-5 (starts exactly when previous ends)
        var (isAvailable, reason) = await service.CheckAvailabilityAsync(1,
            new DateTime(2024, 1, 3),
            new DateTime(2024, 1, 5));

        // Assert: Back-to-back should be allowed
        Assert.True(isAvailable);
        Assert.Null(reason);
    }

    [Fact]
    public async Task CheckAvailability_CancelledBooking_IsAvailable()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };
        var booking = new Booking
        {
            RoomId = 1,
            CheckIn = new DateTime(2024, 1, 3),
            CheckOut = new DateTime(2024, 1, 6),
            GuestCount = 1,
            TotalPrice = 300,
            IsCancelled = true,  // Cancelled
            CreatedAt = DateTime.UtcNow
        };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: Try to book same dates
        var (isAvailable, reason) = await service.CheckAvailabilityAsync(1,
            new DateTime(2024, 1, 3),
            new DateTime(2024, 1, 6));

        // Assert: Cancelled bookings should be ignored
        Assert.True(isAvailable);
        Assert.Null(reason);
    }

    [Fact]
    public async Task CheckAvailability_PartialOverlap_NotAvailable()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };
        var booking = new Booking
        {
            RoomId = 1,
            CheckIn = new DateTime(2024, 1, 3),
            CheckOut = new DateTime(2024, 1, 6),
            GuestCount = 1,
            TotalPrice = 300,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: Try to book Jan 4-7 (overlaps with Jan 3-6 on Jan 4-5)
        var (isAvailable, reason) = await service.CheckAvailabilityAsync(1,
            new DateTime(2024, 1, 4),
            new DateTime(2024, 1, 7));

        // Assert
        Assert.False(isAvailable);
        Assert.NotNull(reason);
    }
}
