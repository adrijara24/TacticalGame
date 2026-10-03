using System.Text.Json.Nodes;
using Tactical.Core;

public abstract class DItem : IAsset
{
    protected string itemID;
    protected string name;
    protected string description;

    public string ID => itemID;

    public string Name => name;

    public string Description => description;

    public DItem(string itemID)
    {
        this.itemID = itemID;
        this.name = "";
        this.description = "";
    }

    public virtual void FromJson(JsonObject json)
    {
        throw new NotImplementedException();
    }

    public virtual JsonObject ToJson()
    {
        throw new NotImplementedException();
    }
}