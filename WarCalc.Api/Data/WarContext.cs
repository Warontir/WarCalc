using Microsoft.EntityFrameworkCore;
using WarCalc.Api.Models;

namespace WarCalc.Api.Data;

public class WarContext(DbContextOptions<WarContext> options) : DbContext(options)
{
    public DbSet<Faction> Factions => Set<Faction>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Weapon> Weapons => Set<Weapon>();
    public DbSet<Army> Armies => Set<Army>();
    public DbSet<ArmyUnit> ArmyUnits => Set<ArmyUnit>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Faction>().HasIndex(f => f.Name).IsUnique();
        b.Entity<ArmyUnit>().HasIndex(a => new { a.ArmyId, a.UnitId }).IsUnique();

        static Unit U(int id, string n, int f, int models, int pts, int m, int t, int sv, int? inv, int w, int ld, int oc) =>
            new() { Id = id, Name = n, FactionId = f, ModelCount = models, Points = pts, Move = m, Toughness = t,
                    Save = sv, InvulnerableSave = inv, Wounds = w, Leadership = ld, ObjectiveControl = oc };
        static Weapon W(int id, int unit, string n, int range, double a, int skill, int s, int ap, double d) =>
            new() { Id = id, UnitId = unit, Name = n, RangeInches = range, Attacks = a, Skill = skill,
                    Strength = s, Ap = ap, Damage = d };

        b.Entity<Faction>().HasData(
            new Faction { Id = 1, Name = "Space Marines", Description = "Genetically enhanced super-soldiers." },
            new Faction { Id = 2, Name = "Orks", Description = "Brutal green xenos who love a good fight." },
            new Faction { Id = 3, Name = "Necrons", Description = "Ancient undying metal warriors." });

        b.Entity<Unit>().HasData(
            U(1, "Intercessor Squad", 1, 5, 80, 6, 4, 3, null, 2, 6, 2),
            U(2, "Terminator Squad", 1, 5, 180, 5, 5, 2, 4, 3, 6, 1),
            U(3, "Redemptor Dreadnought", 1, 1, 210, 8, 10, 2, null, 12, 6, 4),
            U(4, "Boyz", 2, 10, 80, 6, 5, 5, null, 1, 7, 2),
            U(5, "Nobz", 2, 3, 105, 5, 5, 4, null, 3, 7, 1),
            U(6, "Necron Warriors", 3, 10, 100, 5, 4, 4, null, 1, 7, 2),
            U(7, "Immortals", 3, 5, 70, 5, 5, 3, null, 1, 7, 2));

        b.Entity<Weapon>().HasData(
            W(1, 1, "Bolt rifle", 24, 2, 3, 4, 1, 1),
            W(2, 1, "Close combat weapon", 0, 3, 3, 4, 0, 1),
            W(3, 2, "Storm bolter", 24, 2, 3, 4, 0, 1),
            W(4, 2, "Power fist", 0, 3, 3, 8, 2, 2),
            W(5, 3, "Macro plasma incinerator", 36, 3.5, 3, 9, 4, 2),
            W(6, 3, "Redemptor fist", 0, 5, 3, 12, 2, 3),
            W(7, 4, "Slugga", 12, 1, 5, 4, 0, 1),
            W(8, 4, "Choppa", 0, 3, 3, 4, 1, 1),
            W(9, 5, "Power klaw", 0, 3, 4, 9, 2, 2),
            W(10, 6, "Gauss flayer", 24, 1, 4, 4, 1, 1),
            W(11, 7, "Gauss blaster", 24, 2, 3, 5, 2, 1));
    }
}
