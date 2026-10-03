using System.Text.Json.Nodes;
using System.Xml.Linq;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Units;

public class DUnit : IAsset
{
    private string unitClass;

    private int lives;

    private string unitID;

    private string name;
    private string description;

    private string[] inventory;

    private Stats baseStats;

    public string ID => unitID;

    public string Name => name;
    public string Description => description;

    public string[] Inventory
    {
        get { return inventory; }
    }

    public Stats Stats => baseStats;

    public DUnit()
    {
        this.unitID = "";
        this.name = "";
        this.description = "";
        this.unitClass = "";
        this.baseStats = new Stats();
        this.lives = 0;
        this.inventory = new string[5];
        for (int i = 0; i < inventory.Length; ++i)
            this.inventory[i] = "NONE";
    }

    public DUnit(string unitID, Stats baseStats, string unitClass)
    {
        this.unitID = unitID;
        this.name = "";
        this.description = "";
        this.unitClass = unitClass;
        this.baseStats = baseStats;
        this.lives = 0;
        this.inventory = new string[5];
        for (int i = 0; i < inventory.Length; ++i)
            this.inventory[i] = "NONE";
    }

    public void FromJson(JsonObject json)
    {
        unitID = json["ID"]!.GetValue<string>();
        name = json["Name"]!.GetValue<string>();
        description = json["Description"]!.GetValue<string>();
        unitClass = json["ClassID"]!.GetValue<string>();
        baseStats.FromJson(json["Stats"]!.AsObject());
        lives = json["Lives"]!.GetValue<int>();

        JsonObject inv = json["Inventory"]!.AsObject();

        for (int i = 0; i < inventory.Length; i++)
            inventory[i] = inv["Item" + (i + 1)]!.GetValue<string>();
    }

    public JsonObject ToJson()
    {
        JsonObject json = new JsonObject();
        json.Add("ID", ID);
        json.Add("Name", Name);
        json.Add("Description", Description);
        json.Add("ClassID", unitClass);
        json.Add("Stats", baseStats.ToJson());
        json.Add("Lives", lives);
        JsonObject inv = new JsonObject();
        for (int i = 0; i < inventory.Length; i++)
            inv.Add("Item" + (i + 1), inventory[i]);
        json.Add("Inventory", inv);

        return json;
    }
}