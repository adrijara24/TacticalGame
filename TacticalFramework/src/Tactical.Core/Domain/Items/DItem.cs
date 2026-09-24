using System.Text.Json;
using Tactical.Core.Persistence;
public class DItem : IPersistent
{
    private String itemID;

    public String ID => itemID;

    public DItem(String itemID)
    {
        this.itemID = itemID;
    }

    public void FromJson(JsonElement json)
    {
        throw new NotImplementedException();
    }

    public JsonElement ToJson()
    {
        throw new NotImplementedException();
    }
}