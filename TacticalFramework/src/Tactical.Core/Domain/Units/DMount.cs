using Tactical.Core.Domain.Units;
using Tactical.Core.Domain;
using System.Text.Json;

public class Mount : IPersistent
{
    String name;
    Stats stats;

    public enum MountType
    {
        GROUND = 1, FLYING = 2, WATER = 4
    }

    MountType type;

    public Mount()
    {
        this.name = "No mount";
        this.stats = new Stats();
        this.type = MountType.GROUND;
    }

    public Mount(String name)
    {
        this.name = name;
        this.stats = new Stats();
        this.type = MountType.GROUND;
    }

    public Mount(String name, Stats stats)
    {
        this.name = name;
        this.stats = stats;
        this.type = MountType.GROUND;
    }

    public Mount(String name, Stats stats, MountType type)
    {
        this.name = name;
        this.stats = stats;
        this.type = type;
    }

    public String Name => name;

    public Stats Stats => stats;

    public MountType Type => type;

    public void FromJson(JsonElement json)
    {
        throw new NotImplementedException();
    }

    public JsonElement ToJson()
    {
        throw new NotImplementedException();
    }
}