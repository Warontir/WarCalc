namespace WarCalc.Wpf;


public class Faction
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class Unit
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int FactionId { get; set; }
    public int ModelCount { get; set; } = 1;
    public int Points { get; set; }
    public int Move { get; set; } = 6;
    public int Toughness { get; set; } = 4;
    public int Save { get; set; } = 3;
    public int? InvulnerableSave { get; set; }
    public int Wounds { get; set; } = 1;
    public int Leadership { get; set; } = 6;
    public int ObjectiveControl { get; set; } = 1;
}

public class Weapon
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int UnitId { get; set; }
    public int RangeInches { get; set; }
    public double Attacks { get; set; } = 1;
    public int Skill { get; set; } = 3;
    public int Strength { get; set; } = 4;
    public int Ap { get; set; }
    public double Damage { get; set; } = 1;
}

public class Army
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int FactionId { get; set; }
    public int PointsLimit { get; set; } = 2000;
}

public record ArmyLine(int UnitId, string UnitName, int Quantity, int PointsEach, int LineTotal);

public record ArmySummary(int ArmyId, string Name, string Faction, int PointsLimit, int TotalPoints,
                          int RemainingPoints, bool IsValid, List<ArmyLine> Lines);

public record DamageResult(string Weapon, string Target, int Attackers, double HitChance, double WoundChance,
                           double FailedSaveChance, double ExpectedUnsavedWounds, double ExpectedDamage,
                           double ExpectedModelsSlain);
