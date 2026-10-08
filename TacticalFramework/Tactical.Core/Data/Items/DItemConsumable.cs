using Tactical.Core.Domain.Units;
using System.Text.Json.Nodes;
using Tactical.Core.Persistence;

public class DItemConsumable : DItem
{
    string effectID;
    int uses;

    public int Uses => uses;

    public string Effect => effectID;
    public DItemConsumable() : base("")
    {
        this.effectID = "";
    }

    public DItemConsumable(string itemID, string name, string description, int uses, string effectID) : base(itemID, name, description)
    {
        this.effectID = effectID;
        this.uses = uses;
    }

    public override void FromJson(JsonObject json)
    {
        itemID = json["ID"]!.GetValue<string>();
        name = json["Name"]!.GetValue<string>();
        description = json["Description"]!.GetValue<string>();
        uses = json["Uses"]!.GetValue<int>();
        effectID = json["Effect"]!.GetValue<string>();
    }

    public override JsonObject ToJson()
    {
        JsonObject json = new JsonObject();
        json.Add("ID", ID);
        json.Add("Name", Name);
        json.Add("Description", Description);
        json.Add("Type", "DItemConsumable");
        json.Add("Uses", uses);
        json.Add("Effect", effectID);
        return json;
    }
}