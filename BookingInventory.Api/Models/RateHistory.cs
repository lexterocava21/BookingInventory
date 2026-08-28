using System;
using System.Text.Json.Serialization;

namespace BookingInventory.Api.Models;

/// <summary>
/// Tracks historical rate changes for a room.
/// A new RateHistory entry is created whenever the BaseRate changes.
/// To find the rate active on a specific date, query the most recent RateHistory
/// where EffectiveDate <= targetDate and RoomId matches.
/// </summary>
public class RateHistory
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public decimal BaseRate { get; set; }
    public DateTime EffectiveDate { get; set; }

    [JsonIgnore]
    public Room? Room { get; set; }
}
