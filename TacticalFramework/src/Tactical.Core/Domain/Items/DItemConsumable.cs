using Tactical.Core.Domain.Units;
using System.Text.Json.Nodes;
using Tactical.Core.Persistence;

public class DItemConsumable : DItem
{
    string effectID;
    int uses;

    public DItemConsumable(string itemID, int uses, string effect) : base(itemID)
    {
        this.effectID = effect;
        this.uses = uses;
    }

    public override void FromJson(JsonObject json)
    {
        throw new NotImplementedException();
    }

    public override JsonObject ToJson()
    {
        JsonObject json = new JsonObject();
        json.Add("ID", ID);
        json.Add("Uses", uses);
        json.Add("Effect", effectID);
        return json;
    }
}