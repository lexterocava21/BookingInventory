using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingInventory.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    private readonly BookingDbContext _context;

    public HotelsController(BookingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetHotels()
    {
        var hotels = await _context.Hotels
            .AsNoTracking()
            .OrderBy(h => h.Name)
            .Select(h => new
            {
                Id = h.Id,
                Name = h.Name,
                AllowOverbooking = h.AllowOverbooking,
                Rooms = h.Rooms
                    .OrderBy(r => r.Number)
                    .Select(r => new
                    {
                        Id = r.Id,
                        HotelId = r.HotelId,
                        Number = r.Number,
                        Capacity = r.Capacity
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(hotels);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetHotel(int id)
    {
        var hotel = await _context.Hotels
            .AsNoTracking()
            .Where(h => h.Id == id)
            .Select(h => new
            {
                Id = h.Id,
                Name = h.Name,
                AllowOverbooking = h.AllowOverbooking,
                Rooms = h.Rooms
                    .OrderBy(r => r.Number)
                    .Select(r => new
                    {
                        Id = r.Id,
                        HotelId = r.HotelId,
                        Number = r.Number,
                        Capacity = r.Capacity
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (hotel == null)
            return NotFound();

        return Ok(hotel);
    }
}
