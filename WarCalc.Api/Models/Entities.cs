using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WarCalc.Api.Models;

public class Faction
{
    public int Id { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = "";
    [MaxLength(300)] public string? Description { get; set; }
    [JsonIgnore] public List<Unit> Units { get; set; } = new();
}

public class Unit
{
    public int Id { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = "";
    public int FactionId { get; set; }
    [Range(1, 100)] public int ModelCount { get; set; } = 1;
    [Range(0, 3000)] public int Points { get; set; }
    [Range(0, 50)] public int Move { get; set; }
    [Range(1, 20)] public int Toughness { get; set; }
    [Range(2, 7)] public int Save { get; set; }                 
    [Range(2, 6)] public int? InvulnerableSave { get; set; }    
    [Range(1, 50)] public int Wounds { get; set; }
    [Range(2, 10)] public int Leadership { get; set; }
    [Range(0, 10)] public int ObjectiveControl { get; set; }
    [JsonIgnore] public Faction? Faction { get; set; }
    public List<Weapon> Weapons { get; set; } = new();
}

public class Weapon
{
    public int Id { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = "";
    public int UnitId { get; set; }
    [Range(0, 72)] public int RangeInches { get; set; }         
    [Range(0.5, 50)] public double Attacks { get; set; }        
    [Range(2, 6)] public int Skill { get; set; }                
    [Range(1, 30)] public int Strength { get; set; }
    [Range(0, 6)] public int Ap { get; set; }                   
    [Range(0.5, 50)] public double Damage { get; set; }         
    [JsonIgnore] public Unit? Unit { get; set; }
}

public class Army
{
    public int Id { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = "";
    public int FactionId { get; set; }
    [Range(1, 10000)] public int PointsLimit { get; set; } = 2000;
    [JsonIgnore] public Faction? Faction { get; set; }
    public List<ArmyUnit> ArmyUnits { get; set; } = new();
}

public class ArmyUnit
{
    public int Id { get; set; }
    public int ArmyId { get; set; }
    public int UnitId { get; set; }
    [Range(1, 50)] public int Quantity { get; set; } = 1;
    [JsonIgnore] public Army? Army { get; set; }
    public Unit? Unit { get; set; }
}
