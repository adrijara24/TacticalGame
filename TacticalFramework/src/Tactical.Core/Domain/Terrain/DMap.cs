using System.Text.Json.Nodes;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Terrain;

public class Map : IAsset
{
    private Tile[,] tiles;
    private string mapName;
    private string description;

    public Map(int width, int height)
    {
        tiles = new Tile[width, height];
        mapName = "Default Map";
    }

    public Map(int width, int height, Tile defaultTile)
    {
        tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tiles[x, y] = defaultTile;
        mapName = "Default Map";
    }

    public Map(Tile[,] tiles)
    {
        this.tiles = tiles;
        mapName = "Default Map";
    }

    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);

    public string MapName
    {
        get => mapName;
        set => mapName = value;
    }

    public string ID => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public string Description => throw new NotImplementedException();

    public Tile this[int x, int y]
    {
        get => GetTile(x, y);
        set
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("Coordinates are out of bounds.");
            tiles[x, y] = value;
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException("Coordinates are out of bounds.");
        return tiles[x, y];
    }

    public JsonObject ToJson()
    {
        throw new NotImplementedException();
    }

    public void FromJson(JsonObject json)
    {
        throw new NotImplementedException();
    }
}