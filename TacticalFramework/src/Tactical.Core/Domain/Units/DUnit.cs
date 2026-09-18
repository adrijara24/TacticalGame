using System.Text.Json;

namespace Tactical.Core.Domain.Units;

public class Unit : IPersistent
{
    public Stats baseStats;
    public Weapon weapon;

    public void FromJson(JsonElement json)
    {
        throw new NotImplementedException();
    }

    public Stats GetAccumulatedStats() { return baseStats + weapon.stats; }

    public JsonElement ToJson()
    {
        throw new NotImplementedException();
    }
}