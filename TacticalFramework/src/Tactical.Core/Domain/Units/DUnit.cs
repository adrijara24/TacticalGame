using System.Text.Json;

namespace Tactical.Core.Domain.Units;

public class Unit : IPersistent
{
    private DClass unitClass;
    private Weapon weapon;

    private int lives;

    private Stats aggregatedStats;

    private String unitID;

    private DInventory inventory;

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

    public DInventory Inventory
    {
        get { return inventory; }
    }

    public void UpdateStats()
    {
        aggregatedStats = unitClass.Stats + weapon.Stats;
    }

    public Unit(String unitID, DClass unitClass, Weapon weapon)
    {
        this.unitID = unitID;
        this.unitClass = unitClass;
        this.weapon = weapon;
        this.lives = 0;
        this.inventory = new DInventory(5);
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