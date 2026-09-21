using Tactical.Core.Domain.Units;
using Tactical.Core.Domain;
using System.Text.Json;

class DConsumable : DItem, IPersistent
{
    Stats stats;
    int uses;

    int damage;
    int healing;

    public DConsumable(String itemID, int uses, Stats stats) : base(itemID)
    {
        this.stats = stats;
        this.uses = uses;
        this.damage = 0;
        this.healing = 0;
    }

    public DConsumable(String itemID, int uses, int damage, int healing) : base(itemID)
    {
        this.stats = new Stats();
        this.uses = uses;
        this.damage = damage;
        this.healing = 0;
    }

    public DConsumable(String itemID, int uses, Stats stats, int damage, int healing) : base(itemID)
    {
        this.stats = stats;
        this.uses = uses;
        this.damage = damage;
        this.healing = healing;
    }

    public Stats Stats => stats;

    public new void FromJson(JsonElement json)
    {
        throw new NotImplementedException();
    }

    public new JsonElement ToJson()
    {
        throw new NotImplementedException();
    }
}