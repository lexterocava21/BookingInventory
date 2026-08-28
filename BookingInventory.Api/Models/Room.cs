using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookingInventory.Api.Models;

public class Room
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string Number { get; set; } = null!;
    public int Capacity { get; set; }

    public Hotel? Hotel { get; set; }

    [JsonIgnore]
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [JsonIgnore]
    public ICollection<RateHistory> RateHistories { get; set; } = new List<RateHistory>();
}
