using DeliveryAggregator.DTOs;
using DeliveryAggregator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAggregator.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _service;

    public ApplicationsController(IApplicationService service) => _service = service;

    // POST /api/applications
    // Любой желающий оставляет заявку (ФИО/название + email + роль)
    // Без авторизации — форма на сайте
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApplicationRequest request)
    {
        var result = await _service.CreateAsync(request);
        return Ok(result);
    }

    // GET /api/applications/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
