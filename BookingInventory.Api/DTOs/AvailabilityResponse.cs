using System;

namespace BookingInventory.Api.DTOs;

public class AvailabilityResponse
{
    public int RoomId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; }
}
