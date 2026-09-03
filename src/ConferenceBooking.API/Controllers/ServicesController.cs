using ConferenceBooking.Application.DTOs.Services;
using ConferenceBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _serviceCatalogService;

    public ServicesController(IServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceResponse>>> GetAll()
    {
        var services = await _serviceCatalogService.GetAllAsync();
        return Ok(services);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResponse>> GetById(int id)
    {
        var service = await _serviceCatalogService.GetByIdAsync(id);
        return service is null ? NotFound() : Ok(service);
    }

    [Authorize(Roles = "Organizer")]
    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(ServiceRequest request)
    {
        var service = await _serviceCatalogService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    [Authorize(Roles = "Organizer")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ServiceResponse>> Update(int id, ServiceRequest request)
    {
        try
        {
            var service = await _serviceCatalogService.UpdateAsync(id, request);
            return Ok(service);
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
            await _serviceCatalogService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}