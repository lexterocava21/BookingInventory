using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookingInventory.Api.Models;

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool AllowOverbooking { get; set; }

    [JsonIgnore]
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
