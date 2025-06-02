using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrmSystem.Api.Data;
using CrmSystem.Api.Models;
using CrmSystem.Api.Dtos;
using CrmSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace CrmSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // авторизация по умолчанию
public class ClientsController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly LogService _log;
    private readonly PdfService _pdf;

    public ClientsController(CrmDbContext context, LogService log, PdfService pdf)
    {
        _context = context;
        _log = log;
        _pdf = pdf;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
    {
        var clients = await _context.Clients.OrderByDescending(c => c.CreatedAt).ToListAsync();

        return clients.Select(c => new ClientDto
        {
            Id = c.Id,
            FullName = c.FullName,
            Email = c.Email,
            Phone = c.Phone,
            Company = c.Company,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetById(int id)
    {
        var c = await _context.Clients.FindAsync(id);
        if (c == null) return NotFound();

        return new ClientDto
        {
            Id = c.Id,
            FullName = c.FullName,
            Email = c.Email,
            Phone = c.Phone,
            Company = c.Company,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        };
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create(ClientDto dto)
    {
        var client = new Client
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Company = dto.Company,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        await _log.LogAsync("Create", "Client", $"Added {client.FullName}");

        dto.Id = client.Id;
        dto.CreatedAt = client.CreatedAt;

        return CreatedAtAction(nameof(GetById), new { id = client.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ClientDto dto)
    {
        if (id != dto.Id) return BadRequest();

        var client = await _context.Clients.FindAsync(id);
        if (client == null) return NotFound();

        client.FullName = dto.FullName;
        client.Email = dto.Email;
        client.Phone = dto.Phone;
        client.Company = dto.Company;
        client.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        await _log.LogAsync("Update", "Client", $"Updated {client.FullName}");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return NotFound();

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        await _log.LogAsync("Delete", "Client", $"Deleted {client.FullName}");

        return NoContent();
    }

    [AllowAnonymous] // ← Делаем метод публичным
    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf()
    {
        var clients = await _context.Clients.OrderByDescending(c => c.CreatedAt).ToListAsync();
        var pdfBytes = _pdf.GenerateClientPdf(clients);
        return File(pdfBytes, "application/pdf", "clients.pdf");
    }
}