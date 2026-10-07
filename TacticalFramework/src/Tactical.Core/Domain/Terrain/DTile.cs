using System.Text.Json.Nodes;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Terrain;

public class DTile : IAsset
{
    string tileID;
    private string name;
    private string description;

    Stats stats;

    int movementCost;

    int passableBy;

    string effectID;

    public DTile() : this("", "", "", "") { }

    public DTile(string tileID, string name, string description, string effectID, Stats stats = default, int movementCost = 1, int passableBy = (int)(DClass.EMovementType.GROUND | DClass.EMovementType.WATER | DClass.EMovementType.FLYING))
    {
        this.tileID = tileID;
        this.name = name;
        this.description = description;
        this.effectID = effectID;
        this.stats = stats;
        this.movementCost = movementCost;
        this.passableBy = passableBy;
    }

    public Stats Stats => stats;

    public int MovementCost => movementCost;

    public int PassableBy => passableBy;

    public string Effect => effectID;

    public string ID => tileID;

    public string Name => name;

    public string Description => description;

    public bool IsPassableBy(DClass unitClass)
    {
        return (passableBy & (int)unitClass.MovementType) != 0;
    }

    public JsonObject ToJson()
    {
        JsonObject json = new JsonObject
        {
            ["ID"] = ID,
            ["Name"] = Name,
            ["Description"] = Description,
            ["EffectID"] = Effect,
            ["Stats"] = stats.ToJson(),
            ["MovementCost"] = MovementCost,
            ["PassableBy"] = PassableBy
        };

        return json;
    }

    public void FromJson(JsonObject json)
    {
        tileID = json["ID"]!.GetValue<string>();
        name = json["Name"]!.GetValue<string>();
        description = json["Description"]!.GetValue<string>();
        effectID = json["EffectID"]!.GetValue<string>() ?? "";

        if (json["Stats"] is JsonObject statsJson)
        {
            Stats loadedStats = new Stats();
            loadedStats.FromJson(statsJson);
            stats = loadedStats;
        }
        else
        {
            stats = default;
        }

        movementCost = json["MovementCost"]!.GetValue<int>();
        passableBy = json["PassableBy"]!.GetValue<int>();
    }
}
