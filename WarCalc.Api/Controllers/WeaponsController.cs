using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarCalc.Api.Data;
using WarCalc.Api.Models;
using WarCalc.Api.Services;

namespace WarCalc.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class WeaponsController(WarContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<Weapon>> GetAll(int? unitId)
    {
        var q = db.Weapons.AsNoTracking().AsQueryable();
        if (unitId is > 0) q = q.Where(w => w.UnitId == unitId);
        return await q.OrderBy(w => w.Name).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Weapon>> Get(int id)
    {
        var weapon = await db.Weapons.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);
        if (weapon is null) return NotFound();
        return weapon;
    }

    [HttpPost]
    public async Task<ActionResult<Weapon>> Create(Weapon weapon)
    {
        if (!await db.Units.AnyAsync(u => u.Id == weapon.UnitId)) return BadRequest("Unknown unit.");
        weapon.Id = 0;
        db.Weapons.Add(weapon);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = weapon.Id }, weapon);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Weapon weapon)
    {
        var existing = await db.Weapons.FindAsync(id);
        if (existing is null) return NotFound();
        if (!await db.Units.AnyAsync(u => u.Id == weapon.UnitId)) return BadRequest("Unknown unit.");
        weapon.Id = id;
        db.Entry(existing).CurrentValues.SetValues(weapon);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await db.Weapons.FindAsync(id);
        if (existing is null) return NotFound();
        db.Weapons.Remove(existing);
        await db.SaveChangesAsync();
        return NoContent();
    }

   
    [HttpGet("{id:int}/damage/{targetUnitId:int}")]
    public async Task<ActionResult<DamageResult>> Damage(int id, int targetUnitId, int attackers = 1)
    {
        if (attackers < 1) return BadRequest("attackers must be at least 1.");
        var weapon = await db.Weapons.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);
        var target = await db.Units.AsNoTracking().FirstOrDefaultAsync(u => u.Id == targetUnitId);
        if (weapon is null || target is null) return NotFound();
        return DamageCalculator.Calculate(weapon, target, attackers);
    }
}
