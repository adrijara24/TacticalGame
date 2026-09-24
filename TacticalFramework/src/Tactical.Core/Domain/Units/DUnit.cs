using System.Text.Json;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Units;

public class DUnit : IPersistent
{
    private String unitClass;

    private int lives;

    private String unitID;

    private String[] inventory;

    private Stats baseStats;

    public String ID => unitID;

    public String[] Inventory
    {
        get { return inventory; }
    }

    public Stats Stats => baseStats;

    public DUnit()
    {
        this.unitID = "";
        this.unitClass = "";
        this.baseStats = new Stats();
        this.lives = 0;
        this.inventory = new String[5];
    }

    public DUnit(String unitID, Stats baseStats, String unitClass)
    {
        this.unitID = unitID;
        this.unitClass = unitClass;
        this.baseStats = baseStats;
        this.lives = 0;
        this.inventory = new String[5];
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