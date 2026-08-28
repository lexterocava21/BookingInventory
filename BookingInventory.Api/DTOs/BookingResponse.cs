using System;

namespace BookingInventory.Api.DTOs;

public class BookingResponse
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int GuestCount { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsOverCapacity { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCancelled { get; set; }
}
