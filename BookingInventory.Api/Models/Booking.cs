using System;
using System.Text.Json.Serialization;

namespace BookingInventory.Api.Models;

public class Booking
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
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public Room? Room { get; set; }
}
