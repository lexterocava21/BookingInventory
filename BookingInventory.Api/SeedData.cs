using System;
using System.Linq;
using BookingInventory.Api.Data;
using BookingInventory.Api.Models;

namespace BookingInventory.Api;

public static class SeedData
{
    public static void Initialize(BookingDbContext context)
    {
        // Check if data already exists
        if (context.Hotels.Any())
        {
            return;
        }

        // Create Hotels
        var luxuryHotel = new Hotel { Name = "Luxury Palace", AllowOverbooking = false };
        var budgetHotel = new Hotel { Name = "Budget Inn", AllowOverbooking = true };

        context.Hotels.AddRange(luxuryHotel, budgetHotel);
        context.SaveChanges();

        // Create Rooms
        var luxuryRooms = new[]
        {
            new Room { HotelId = luxuryHotel.Id, Number = "101", Capacity = 2 },
            new Room { HotelId = luxuryHotel.Id, Number = "102", Capacity = 4 },
            new Room { HotelId = luxuryHotel.Id, Number = "201", Capacity = 2 }
        };

        var budgetRooms = new[]
        {
            new Room { HotelId = budgetHotel.Id, Number = "A1", Capacity = 2 },
            new Room { HotelId = budgetHotel.Id, Number = "A2", Capacity = 1 }
        };

        context.Rooms.AddRange(luxuryRooms);
        context.Rooms.AddRange(budgetRooms);
        context.SaveChanges();

        // Create RateHistories (with a rate change for room 101)
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var nextWeek = today.AddDays(7);

        // Room 101 (Luxury): $100/night initially, increases to $150/night after tomorrow
        var room101 = luxuryRooms[0];
        context.RateHistories.AddRange(
            new RateHistory { RoomId = room101.Id, BaseRate = 100, EffectiveDate = today.AddDays(-30) },
            new RateHistory { RoomId = room101.Id, BaseRate = 150, EffectiveDate = nextWeek }
        );

        // Room 102 (Luxury): $200/night
        var room102 = luxuryRooms[1];
        context.RateHistories.Add(
            new RateHistory { RoomId = room102.Id, BaseRate = 200, EffectiveDate = today.AddDays(-30) }
        );

        // Room 201 (Luxury): $120/night
        var room201 = luxuryRooms[2];
        context.RateHistories.Add(
            new RateHistory { RoomId = room201.Id, BaseRate = 120, EffectiveDate = today.AddDays(-30) }
        );

        // Room A1 (Budget): $50/night
        var roomA1 = budgetRooms[0];
        context.RateHistories.Add(
            new RateHistory { RoomId = roomA1.Id, BaseRate = 50, EffectiveDate = today.AddDays(-30) }
        );

        // Room A2 (Budget): $30/night
        var roomA2 = budgetRooms[1];
        context.RateHistories.Add(
            new RateHistory { RoomId = roomA2.Id, BaseRate = 30, EffectiveDate = today.AddDays(-30) }
        );

        context.SaveChanges();

        // Create sample bookings
        var pastDate = today.AddDays(-10);
        var pastCheckOut = today.AddDays(-5);
        var futureCheckIn = today.AddDays(2);
        var futureCheckOut = today.AddDays(5);

        var booking1 = new Booking
        {
            RoomId = room101.Id,
            CheckIn = pastDate,
            CheckOut = pastCheckOut,
            GuestCount = 2,
            TotalPrice = 500, // 5 nights at $100
            IsOverCapacity = false,
            IsCompleted = true,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var booking2 = new Booking
        {
            RoomId = room102.Id,
            CheckIn = futureCheckIn,
            CheckOut = futureCheckOut,
            GuestCount = 3,
            TotalPrice = 600, // 3 nights at $200
            IsOverCapacity = false,
            IsCompleted = false,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Bookings.AddRange(booking1, booking2);
        context.SaveChanges();
    }
}
