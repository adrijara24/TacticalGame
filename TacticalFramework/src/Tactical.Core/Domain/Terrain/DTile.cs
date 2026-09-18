using System.Text.Json;
using Tactical.Core.Domain.Units;

namespace Tactical.Core.Domain.Terrain;

public class Tile : IPersistent
{
    public enum TerrainType
    {
        GRASS = 0, WATER, MOUNTAIN, FOREST, ROAD, NONE
    }

    public TerrainType type;

    public Stats bonusStats;

    public int movementCost;

    public int damage;

    public Tile(TerrainType type, Stats bonusStats, int movementCost, int damage)
    {
        this.type = type;
        this.bonusStats = bonusStats;
        this.movementCost = movementCost;
        this.damage = damage;
    }

    public JsonElement ToJson()
    {
        throw new NotImplementedException();
    }

    public void FromJson(JsonElement json)
    {
        throw new NotImplementedException();
    }
}
