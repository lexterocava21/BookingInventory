using System;

namespace BookingInventory.Api.DTOs;

public class CreateBookingRequest
{
    public int RoomId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int GuestCount { get; set; }
}
