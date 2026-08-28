using BookingInventory.Api.Data;
using BookingInventory.Api.Models;
using BookingInventory.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingInventory.Tests;

public class BookingServiceRateCalculationTests
{
    private BookingDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BookingDbContext(options);
    }

    [Fact]
    public async Task CalculateTotalPrice_SingleNightSingleRate()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };
        var rateHistory = new RateHistory { RoomId = 1, BaseRate = 100, EffectiveDate = new DateTime(2024, 1, 1) };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.RateHistories.Add(rateHistory);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: One night from Jan 1 to Jan 2
        var (totalPrice, error) = await service.CalculateTotalPriceAsync(1, 
            new DateTime(2024, 1, 1), 
            new DateTime(2024, 1, 2));

        // Assert: Should be $100 for 1 night
        Assert.Null(error);
        Assert.Equal(100, totalPrice);
    }

    [Fact]
    public async Task CalculateTotalPrice_MultipleNightsSingleRate()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };
        var rateHistory = new RateHistory { RoomId = 1, BaseRate = 100, EffectiveDate = new DateTime(2024, 1, 1) };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.RateHistories.Add(rateHistory);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: 3 nights from Jan 1 to Jan 4
        var (totalPrice, error) = await service.CalculateTotalPriceAsync(1,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 4));

        // Assert: Should be $300 for 3 nights
        Assert.Null(error);
        Assert.Equal(300, totalPrice);
    }

    [Fact]
    public async Task CalculateTotalPrice_SpansRateChange()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };

        // Rate is $100 from Jan 1 to Jan 3, then $150 from Jan 3 onward
        var rate1 = new RateHistory { RoomId = 1, BaseRate = 100, EffectiveDate = new DateTime(2024, 1, 1) };
        var rate2 = new RateHistory { RoomId = 1, BaseRate = 150, EffectiveDate = new DateTime(2024, 1, 3) };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.RateHistories.Add(rate1);
        context.RateHistories.Add(rate2);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: Book from Jan 1 to Jan 5 (2 nights at $100, 2 nights at $150)
        var (totalPrice, error) = await service.CalculateTotalPriceAsync(1,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 5));

        // Assert: Should be $400 (Jan 1=$100, Jan 2=$100, Jan 3=$150, Jan 4=$150)
        Assert.Null(error);
        Assert.Equal(400, totalPrice);
    }

    [Fact]
    public async Task CalculateTotalPrice_MultipleRateChanges()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };

        var rate1 = new RateHistory { RoomId = 1, BaseRate = 100, EffectiveDate = new DateTime(2024, 1, 1) };
        var rate2 = new RateHistory { RoomId = 1, BaseRate = 150, EffectiveDate = new DateTime(2024, 1, 3) };
        var rate3 = new RateHistory { RoomId = 1, BaseRate = 120, EffectiveDate = new DateTime(2024, 1, 5) };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.RateHistories.Add(rate1);
        context.RateHistories.Add(rate2);
        context.RateHistories.Add(rate3);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: Book from Jan 1 to Jan 7
        var (totalPrice, error) = await service.CalculateTotalPriceAsync(1,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 7));

        // Assert: Jan 1-2: $100 each, Jan 3-4: $150 each, Jan 5-6: $120 each = $840
        Assert.Null(error);
        Assert.Equal(840, totalPrice);
    }

    [Fact]
    public async Task CalculateTotalPrice_RateChangeAfterCheckout()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };

        var rate1 = new RateHistory { RoomId = 1, BaseRate = 100, EffectiveDate = new DateTime(2024, 1, 1) };
        var rate2 = new RateHistory { RoomId = 1, BaseRate = 150, EffectiveDate = new DateTime(2024, 1, 5) };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.RateHistories.Add(rate1);
        context.RateHistories.Add(rate2);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: Book from Jan 1 to Jan 4 (before rate change)
        var (totalPrice, error) = await service.CalculateTotalPriceAsync(1,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 4));

        // Assert: 3 nights at $100 = $300
        Assert.Null(error);
        Assert.Equal(300, totalPrice);
    }

    [Fact]
    public async Task CalculateTotalPrice_InvalidDateRange()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var hotel = new Hotel { Name = "Test Hotel", AllowOverbooking = false };
        var room = new Room { HotelId = 1, Number = "101", Capacity = 2 };
        var rateHistory = new RateHistory { RoomId = 1, BaseRate = 100, EffectiveDate = new DateTime(2024, 1, 1) };

        context.Hotels.Add(hotel);
        context.Rooms.Add(room);
        context.RateHistories.Add(rateHistory);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: CheckIn >= CheckOut
        var (totalPrice, error) = await service.CalculateTotalPriceAsync(1,
            new DateTime(2024, 1, 5),
            new DateTime(2024, 1, 5));

        // Assert: Should return an error
        Assert.NotNull(error);
        Assert.Equal(0, totalPrice);
    }
}
