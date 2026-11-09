using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarCalc.Api.Data;
using WarCalc.Api.Models;

namespace WarCalc.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class UnitsController(WarContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<Unit>> GetAll() =>
        await db.Units.AsNoTracking().Include(u => u.Weapons).OrderBy(u => u.Name).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Unit>> Get(int id)
    {
        var unit = await db.Units.AsNoTracking().Include(u => u.Weapons).FirstOrDefaultAsync(u => u.Id == id);
        if (unit is null) return NotFound();
        return unit;
    }

    [HttpPost]
    public async Task<ActionResult<Unit>> Create(Unit unit)
    {
        if (!await db.Factions.AnyAsync(f => f.Id == unit.FactionId)) return BadRequest("Unknown faction.");
        unit.Id = 0;
        unit.Weapons.Clear();           
        db.Units.Add(unit);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = unit.Id }, unit);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Unit unit)
    {
        var existing = await db.Units.FindAsync(id);
        if (existing is null) return NotFound();
        if (!await db.Factions.AnyAsync(f => f.Id == unit.FactionId)) return BadRequest("Unknown faction.");
        unit.Id = id;
        db.Entry(existing).CurrentValues.SetValues(unit);  
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await db.Units.FindAsync(id);
        if (existing is null) return NotFound();
        db.Units.Remove(existing);
        await db.SaveChangesAsync();
        return NoContent();
    }

    
    [HttpGet("search")]
    public async Task<List<Unit>> Search(string? name, int? factionId, int? maxPoints)
    {
        var q = db.Units.AsNoTracking().Include(u => u.Weapons).AsQueryable();
        if (!string.IsNullOrWhiteSpace(name)) q = q.Where(u => EF.Functions.Like(u.Name, $"%{name}%"));
        if (factionId is > 0) q = q.Where(u => u.FactionId == factionId);
        if (maxPoints is > 0) q = q.Where(u => u.Points <= maxPoints);
        return await q.OrderBy(u => u.Name).ToListAsync();
    }
}
