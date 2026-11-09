using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarCalc.Api.Data;
using WarCalc.Api.Models;

namespace WarCalc.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class FactionsController(WarContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<Faction>> GetAll() =>
        await db.Factions.AsNoTracking().OrderBy(f => f.Name).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Faction>> Get(int id)
    {
        var faction = await db.Factions.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        if (faction is null) return NotFound();
        return faction;
    }

    [HttpPost]
    public async Task<ActionResult<Faction>> Create(Faction faction)
    {
        if (await db.Factions.AnyAsync(f => f.Name == faction.Name)) return Conflict("Faction name already exists.");
        faction.Id = 0;
        faction.Units.Clear();
        db.Factions.Add(faction);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = faction.Id }, faction);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Faction faction)
    {
        var existing = await db.Factions.FindAsync(id);
        if (existing is null) return NotFound();
        if (await db.Factions.AnyAsync(f => f.Name == faction.Name && f.Id != id)) return Conflict("Faction name already exists.");
        existing.Name = faction.Name;
        existing.Description = faction.Description;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await db.Factions.FindAsync(id);
        if (existing is null) return NotFound();
        db.Factions.Remove(existing);   // cascades to units, weapons and armies
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ---- custom ----
    /// <summary>All units that belong to the faction.</summary>
    [HttpGet("{id:int}/units")]
    public async Task<ActionResult<List<Unit>>> GetUnits(int id)
    {
        if (!await db.Factions.AnyAsync(f => f.Id == id)) return NotFound();
        return await db.Units.AsNoTracking().Include(u => u.Weapons)
            .Where(u => u.FactionId == id).OrderBy(u => u.Name).ToListAsync();
    }
}
