using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarCalc.Api.Data;
using WarCalc.Api.Models;

namespace WarCalc.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class ArmiesController(WarContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<Army>> GetAll() =>
        await db.Armies.AsNoTracking().OrderBy(a => a.Name).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Army>> Get(int id)
    {
        var army = await db.Armies.AsNoTracking()
            .Include(a => a.ArmyUnits).ThenInclude(au => au.Unit)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (army is null) return NotFound();
        return army;
    }

    [HttpPost]
    public async Task<ActionResult<Army>> Create(Army army)
    {
        if (!await db.Factions.AnyAsync(f => f.Id == army.FactionId)) return BadRequest("Unknown faction.");
        army.Id = 0;
        army.ArmyUnits.Clear();         
        db.Armies.Add(army);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = army.Id }, army);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Army army)
    {
        var existing = await db.Armies.Include(a => a.ArmyUnits).FirstOrDefaultAsync(a => a.Id == id);
        if (existing is null) return NotFound();
        if (!await db.Factions.AnyAsync(f => f.Id == army.FactionId)) return BadRequest("Unknown faction.");
        if (army.FactionId != existing.FactionId && existing.ArmyUnits.Count > 0)
            return BadRequest("Remove the army's units before changing its faction.");
        existing.Name = army.Name;
        existing.FactionId = army.FactionId;
        existing.PointsLimit = army.PointsLimit;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await db.Armies.FindAsync(id);
        if (existing is null) return NotFound();
        db.Armies.Remove(existing);
        await db.SaveChangesAsync();
        return NoContent();
    }

    
    [HttpGet("{id:int}/summary")]
    public async Task<ActionResult<ArmySummary>> Summary(int id)
    {
        var summary = await BuildSummary(id);
        if (summary is null) return NotFound();
        return summary;
    }

    [HttpPost("{id:int}/units")]
    public async Task<ActionResult<ArmySummary>> AddUnit(int id, AddUnitRequest request)
    {
        if (request.Quantity < 1) return BadRequest("Quantity must be at least 1.");
        var army = await db.Armies.FindAsync(id);
        if (army is null) return NotFound();
        var unit = await db.Units.FindAsync(request.UnitId);
        if (unit is null) return BadRequest("Unknown unit.");
        if (unit.FactionId != army.FactionId) return BadRequest("This unit belongs to a different faction than the army.");

        var line = await db.ArmyUnits.FirstOrDefaultAsync(a => a.ArmyId == id && a.UnitId == request.UnitId);
        if (line is null) db.ArmyUnits.Add(new ArmyUnit { ArmyId = id, UnitId = request.UnitId, Quantity = request.Quantity });
        else line.Quantity += request.Quantity;

        await db.SaveChangesAsync();
        return (await BuildSummary(id))!;
    }

    [HttpDelete("{id:int}/units/{unitId:int}")]
    public async Task<ActionResult<ArmySummary>> RemoveUnit(int id, int unitId)
    {
        var line = await db.ArmyUnits.FirstOrDefaultAsync(a => a.ArmyId == id && a.UnitId == unitId);
        if (line is null) return NotFound();
        db.ArmyUnits.Remove(line);
        await db.SaveChangesAsync();
        return (await BuildSummary(id))!;
    }

    private async Task<ArmySummary?> BuildSummary(int id)
    {
        var army = await db.Armies.AsNoTracking()
            .Include(a => a.Faction)
            .Include(a => a.ArmyUnits).ThenInclude(au => au.Unit)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (army is null) return null;

        var lines = army.ArmyUnits
            .Select(au => new ArmyLine(au.UnitId, au.Unit!.Name, au.Quantity, au.Unit.Points, au.Quantity * au.Unit.Points))
            .OrderBy(l => l.UnitName).ToList();
        var total = lines.Sum(l => l.LineTotal);
        return new ArmySummary(army.Id, army.Name, army.Faction!.Name, army.PointsLimit,
                               total, army.PointsLimit - total, total <= army.PointsLimit, lines);
    }
}
