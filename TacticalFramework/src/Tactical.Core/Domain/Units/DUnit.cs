using System.Text.Json;

namespace Tactical.Core.Domain.Units;

public class Unit : IPersistent
{
    private DClass unitClass;
    private Weapon weapon;

    private Stats aggregatedStats;

    private String unitID;

    public String ID => unitID;

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
        aggregatedStats = unitClass.Stats + weapon.Stats;
    }

    public Unit(String unitID, DClass unitClass, Weapon weapon)
    {
        this.unitID = unitID;
        this.unitClass = unitClass;
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