using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingInventory.Api.Models;
using BookingInventory.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingInventory.Api.DTOs;

namespace BookingInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly BookingDbContext _context;

    public RoomsController(BookingDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> GetRoom(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.Hotel)
            .Include(r => r.RateHistories.OrderByDescending(rh => rh.EffectiveDate))
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
            return NotFound();

        return Ok(room);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
    {
        return Ok(await _context.Rooms
            .Include(r => r.Hotel)
            .Include(r => r.RateHistories.OrderByDescending(rh => rh.EffectiveDate))
            .ToListAsync());
    }
}
