using System.Text.Json;

namespace Tactical.Core.Domain.Units;

public class Unit : IPersistent
{
    private Stats baseStats;
    private Weapon weapon;

    private Mount mount;

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

    public Mount Mount
    {
        get => mount;
        set
        {
            mount = value;
            UpdateStats();
        }
    }

    private void UpdateStats()
    {
        aggregatedStats = baseStats + weapon.Stats + mount.Stats;
    }

    public Unit(Stats baseStats, Weapon weapon)
    {
        this.baseStats = baseStats;
        this.weapon = weapon;
        this.mount = new Mount();
        UpdateStats();
    }

    public Unit(Stats baseStats, Weapon weapon, Mount mount)
    {
        this.baseStats = baseStats;
        this.weapon = weapon;
        this.mount = mount;
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