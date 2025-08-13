using backend.Dto;
using backend.Interface.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/vendors")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _service;

    public VendorsController(IVendorService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<VendorReadDto>>> GetAll()
        => Ok(await _service.GetAllVendorsAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<VendorReadDto>> GetById(int id)
    {
        var result = await _service.GetVendorByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<VendorReadDto>> Create(VendorCreateDto dto)
    {
        var created = await _service.CreateVendorAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, VendorUpdateDto dto)
    {
        var success = await _service.UpdateVendorAsync(id, dto);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _service.DeleteVendorAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
