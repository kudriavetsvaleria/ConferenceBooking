using ConferenceBooking.Application.DTOs.Rooms;
using ConferenceBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoomResponse>>> Search(
        [FromQuery] int? minCapacity, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] decimal? maxPrice)
    {
        var rooms = await _roomService.SearchAsync(minCapacity, from, to, maxPrice);
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomResponse>> GetById(int id)
    {
        var room = await _roomService.GetByIdAsync(id);
        return room is null ? NotFound() : Ok(room);
    }

    [Authorize(Roles = "Organizer")]
    [HttpPost]
    public async Task<ActionResult<RoomResponse>> Create(RoomRequest request)
    {
        var room = await _roomService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [Authorize(Roles = "Organizer")]
    [HttpPut("{id}")]
    public async Task<ActionResult<RoomResponse>> Update(int id, RoomRequest request)
    {
        try
        {
            var room = await _roomService.UpdateAsync(id, request);
            return Ok(room);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Organizer")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _roomService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}