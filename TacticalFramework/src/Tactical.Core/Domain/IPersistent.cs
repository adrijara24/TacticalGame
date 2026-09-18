namespace Tactical.Core.Domain;

using System.Text.Json;

public interface IPersistent
{
    public JsonElement ToJson();

    public void FromJson(JsonElement json);
}