namespace Tactical.Core.Persistence;

using System.Text.Json;

public interface IPersistent
{
    public JsonElement ToJson();

    public void FromJson(JsonElement json);
}