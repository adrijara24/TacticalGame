using System.Text.Json.Nodes;
using Tactical.Core;

public class DItem : IAsset
{
    private String itemID;

    public String ID => itemID;

    public DItem(String itemID)
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