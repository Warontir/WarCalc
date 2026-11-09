namespace WarCalc.Api.Models;

public record AddUnitRequest(int UnitId, int Quantity = 1);

public record ArmyLine(int UnitId, string UnitName, int Quantity, int PointsEach, int LineTotal);

public record ArmySummary(int ArmyId, string Name, string Faction, int PointsLimit, int TotalPoints, int RemainingPoints, bool IsValid, List<ArmyLine> Lines);

public record DamageResult(string Weapon, string Target, int Attackers, double HitChance, double WoundChance, double FailedSaveChance, double ExpectedUnsavedWounds, double ExpectedDamage, double ExpectedModelsSlain);