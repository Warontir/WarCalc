using WarCalc.Api.Models;

namespace WarCalc.Api.Services;

public static class DamageCalculator
{
    private static double Chance(int x) => Math.Clamp((7 - x) / 6.0, 1 / 6.0, 5 / 6.0);

    public static int WoundRoll(int strength, int toughness) =>
        strength >= toughness * 2 ? 2 :
        strength > toughness ? 3 :
        strength == toughness ? 4 :
        strength * 2 <= toughness ? 6 : 5;

    public static DamageResult Calculate(Weapon w, Unit target, int attackers)
    {
        var hit = Chance(w.Skill);
        var wound = Chance(WoundRoll(w.Strength, target.Toughness));

        var armour = target.Save + w.Ap;
        var bestSave = target.InvulnerableSave is int inv ? Math.Min(armour, inv) : armour;
        var saved = Math.Clamp((7 - bestSave) / 6.0, 0, 5 / 6.0);

        var unsaved = attackers * w.Attacks * hit * wound * (1 - saved);
        var damage = unsaved * Math.Min(w.Damage, target.Wounds);   
        var slain = Math.Min(damage / target.Wounds, target.ModelCount);

        return new DamageResult(w.Name, target.Name, attackers,
            Math.Round(hit, 3), Math.Round(wound, 3), Math.Round(1 - saved, 3),
            Math.Round(unsaved, 2), Math.Round(damage, 2), Math.Round(slain, 2));
    }
}
