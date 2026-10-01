using System.Text.Json.Nodes;
using Tactical.Core;

public abstract class DItem : IAsset
{
    protected string itemID;

    public string ID => itemID;

    public DItem(string itemID)
    {
        this.itemID = itemID;
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