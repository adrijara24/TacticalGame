using System.Text.Json;

namespace Tactical.Core.Domain.Units;

public class Unit : IPersistent
{
    private Stats baseStats;
    private Weapon weapon;

    private Stats aggregatedStats;

    public Stats Stats
    {
        get => aggregatedStats;
    }

    public Weapon Weapon
    {
        get => weapon;
        set
        {
            weapon = value;
            UpdateStats();
        }
    }

    private void UpdateStats()
    {
        aggregatedStats = baseStats + weapon.Stats;
    }

    public Unit(Stats baseStats, Weapon weapon)
    {
        this.baseStats = baseStats;
        this.weapon = weapon;
        UpdateStats();
    }

    public void FromJson(JsonElement json)
    {
        throw new NotImplementedException();
    }

    public JsonElement ToJson()
    {
        throw new NotImplementedException();
    }
}