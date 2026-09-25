using System.Text.Json.Nodes;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Terrain;

public class Tile : IAsset
{
    public enum TerrainType
    {
        GRASS = 0, WATER, MOUNTAIN, FOREST, ROAD, NONE
    }

    TerrainType type;

    Stats bonusStats;

    int movementCost;

    int damage;

    int passableBy;

    public Tile(TerrainType type, Stats bonusStats, int movementCost, int damage)
    {
        this.type = type;
        this.bonusStats = bonusStats;
        this.movementCost = movementCost;
        this.damage = damage;
        this.passableBy = (int)(DClass.EMovementType.GROUND | DClass.EMovementType.FLYING | DClass.EMovementType.WATER);
    }

    public Tile(TerrainType type, Stats bonusStats, int movementCost, int damage, int passableBy)
    {
        this.type = type;
        this.bonusStats = bonusStats;
        this.movementCost = movementCost;
        this.damage = damage;
        this.passableBy = passableBy;
    }

    public TerrainType Type => type;

    public Stats BonusStats => bonusStats;

    public int MovementCost => movementCost;

    public int Damage => damage;

    public string ID => throw new NotImplementedException();

    public bool IsPassableBy(DClass unitClass)
    {
        return (passableBy & (int)unitClass.MovementType) != 0;
    }

    public JsonObject ToJson()
    {
        throw new NotImplementedException();
    }

    public void FromJson(JsonObject json)
    {
        throw new NotImplementedException();
    }
}
