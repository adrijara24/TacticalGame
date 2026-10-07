using System.Text.Json.Nodes;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Terrain;

public class DMap : IAsset
{
    private string[,] tiles;

    private string mapID;
    private string mapName;
    private string description;

    public DMap() : this("", 1, 1, "") { }

    public DMap(string mapID, int width, int height)
    {
        this.mapID = mapID;
        tiles = new string[width, height];
        mapName = "Default Map";
        description = "";
    }

    public DMap(string mapID, int width, int height, string defaultTile)
    {
        this.mapID = mapID;
        tiles = new string[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tiles[x, y] = defaultTile;
        mapName = "Default Map";
        description = "";
    }

    public DMap(string mapID, string[,] tiles)
    {
        this.mapID = mapID;
        this.tiles = tiles;
        mapName = "Default Map";
        description = "";
    }

    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);

    public string ID => mapID;

    public string Name => mapName;

    public string Description => description;

    public string this[int x, int y]
    {
        get => GetTile(x, y);
        set
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("Coordinates are out of bounds.");
            tiles[x, y] = value;
        }
    }

    public string GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException("Coordinates are out of bounds.");
        return tiles[x, y];
    }

    public JsonObject ToJson()
    {
        JsonArray tilesJson = new JsonArray();

        for (int y = 0; y < Height; y++)
        {
            JsonArray row = new JsonArray();

            for (int x = 0; x < Width; x++)
                row.Add(tiles[x, y]);

            tilesJson.Add(row);
        }

        JsonObject json = new JsonObject
        {
            ["ID"] = ID,
            ["Name"] = Name,
            ["Description"] = Description,
            ["Width"] = Width,
            ["Height"] = Height,
            ["Tiles"] = tilesJson
        };

        return json;
    }

    public void FromJson(JsonObject json)
    {
        mapID = json["ID"]!.GetValue<string>();
        mapName = json["Name"]!.GetValue<string>();
        description = json["Description"]!.GetValue<string>();

        int width = json["Width"]!.GetValue<int>();
        int height = json["Height"]!.GetValue<int>();

        if (width <= 0 || height <= 0)
            throw new InvalidDataException("Map dimensions must be greater than zero.");

        JsonArray tilesJson = json["Tiles"]!.AsArray();

        if (tilesJson.Count != height)
            throw new InvalidDataException("Map tile data does not match the map height.");

        string[,] loadedTiles = new string[width, height];

        for (int y = 0; y < height; y++)
        {
            JsonArray row = tilesJson[y]!.AsArray();

            if (row.Count != width)
                throw new InvalidDataException($"Map row {y} does not match the map width.");

            for (int x = 0; x < width; x++)
                loadedTiles[x, y] = row[x]!.GetValue<string>();
        }

        tiles = loadedTiles;
    }
}
