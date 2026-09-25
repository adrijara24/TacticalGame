using Tactical.Core.Domain.Units;
using System.Text.Json.Nodes;
using Tactical.Core.Persistence;

public class DItemConsumable : DItem
{
    Stats stats;
    int uses;
    int damage;
    int healing;

    public DItemConsumable(String itemID, int uses, Stats stats) : base(itemID)
    {
        this.stats = stats;
        this.uses = uses;
        this.damage = 0;
        this.healing = 0;
    }

    public DItemConsumable(String itemID, int uses, int damage, int healing) : base(itemID)
    {
        this.stats = new Stats();
        this.uses = uses;
        this.damage = damage;
        this.healing = 0;
    }

    public DItemConsumable(String itemID, int uses, Stats stats, int damage = 0, int healing = 0) : base(itemID)
    {
        this.stats = stats;
        this.uses = uses;
        this.damage = damage;
        this.healing = healing;
    }

    public Stats Stats => stats;

    public new void FromJson(JsonObject json)
    {
        throw new NotImplementedException();
    }

    public new JsonObject ToJson()
    {
        throw new NotImplementedException();
    }
}