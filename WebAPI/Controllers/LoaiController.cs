using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs.Loai;
using WebAPI.Services.Interfaces;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoaiController : ControllerBase
{
    private readonly ILoaiService _service;

    public LoaiController(ILoaiService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1,[FromQuery] int pageSize = 10,[FromQuery] string? sortBy = "id",[FromQuery] string? sortOrder = "asc")
    {
        var result = await _service.GetAllAsync(page, pageSize, sortBy, sortOrder);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLoaiDto dto)
    {
        var result = await _service.CreateAsync(dto);

        return CreatedAtAction(nameof(GetById),new { id = result.Id },result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update( int id,[FromBody] UpdateLoaiDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}