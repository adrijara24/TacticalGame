using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using Tactical.Core.Domain.Units;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Terrain;

[Obsolete("Deprecated for simplicity. Keep for reference", true)]
public class DTerrainType : IAsset
{

    string terrainID;
    string name;
    string description;

    int movementCost;
    int passableBy;
    Stats stats;


    public string ID => terrainID;

    public string Name => name;

    public string Description => description;

    public Stats Stats => stats;
    public int MovementCost => movementCost;
    public int PassableBy => passableBy;

    public DTerrainType()
    {
        terrainID = "";
        name = "";
        description = "";
        movementCost = 1;
        passableBy = (int)(DClass.EMovementType.GROUND | DClass.EMovementType.WATER | DClass.EMovementType.FLYING);
        stats = default;
    }

    public DTerrainType(string terrainID, string name, string description, Stats stats = default, int movementCost = 1, int passableBy = (int)(DClass.EMovementType.GROUND | DClass.EMovementType.WATER | DClass.EMovementType.FLYING))
    {
        this.terrainID = terrainID;
        this.name = name;
        this.description = description;
        this.stats = stats;
        this.movementCost = movementCost;
        this.passableBy = passableBy;
    }

    public JsonObject ToJson()
    {
        JsonObject json = new JsonObject
        {
            ["ID"] = ID,
            ["Name"] = Name,
            ["Description"] = Description,
            ["MovementCost"] = MovementCost,
            ["PassableBy"] = PassableBy,
            ["Stats"] = Stats.ToJson()
        };

        return json;
    }

    public void FromJson(JsonObject json)
    {
        terrainID = json["ID"]!.GetValue<string>();
        name = json["Name"]!.GetValue<string>();
        description = json["Description"]!.GetValue<string>();
        movementCost = json["MovementCost"]!.GetValue<int>();
        passableBy = json["PassableBy"]!.GetValue<int>();
        stats.FromJson(json["Stats"]!.AsObject());
    }
}
