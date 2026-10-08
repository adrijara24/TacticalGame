using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Nodes;
using Tactical.Core.Domain.Terrain;
using Tactical.Core.Persistence;

namespace Tactical.Editor.ViewModels
{
    public partial class MapEditorViewModel : AssetEditorViewModel
    {
        private const string AirTileId = "";

        private string[,] tileIds;

        [ObservableProperty]
        private string id = "";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private int width = 10;

        [ObservableProperty]
        private int height = 10;

        [ObservableProperty]
        private string searchText = "";

        [ObservableProperty]
        private string selectedTileId = AirTileId;

        public ObservableCollection<MapCellViewModel> Cells { get; } = new();

        public ObservableCollection<TilePaletteItemViewModel> Tiles { get; } = new();

        public ObservableCollection<TilePaletteItemViewModel> VisibleTiles { get; } = new();

        public string Title => IsNew ? "New Map" : "Edit Map";

        public MapEditorViewModel(MainViewModel owner) : this(owner, null) { }

        public MapEditorViewModel(MainViewModel owner, DMap? map) : base(owner, map)
        {
            if (map == null)
            {
                tileIds = CreateAirMap(width, height);
                RebuildCells();
                RefreshTiles();
                return;
            }

            id = map.ID;
            name = map.Name;
            description = map.Description;
            width = map.Width;
            height = map.Height;

            tileIds = new string[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    tileIds[x, y] = map[x, y] ?? AirTileId;
            }

            RebuildCells();
            RefreshTiles();
        }

        partial void OnWidthChanged(int value)
        {
            if (value > 0)
                ResizeMap(value, Height);
        }

        partial void OnHeightChanged(int value)
        {
            if (value > 0)
                ResizeMap(Width, value);
        }

        partial void OnSearchTextChanged(string value)
        {
            RefreshVisibleTiles();
        }

        partial void OnSelectedTileIdChanged(string value)
        {
            foreach (TilePaletteItemViewModel tile in Tiles)
            {
                tile.IsSelected = tile.TileId == value;
            }
        }

        public void SetCellTile(MapCellViewModel cell, string tileId)
        {
            if (cell.X < 0 || cell.X >= Width || cell.Y < 0 || cell.Y >= Height)
                return;

            tileIds[cell.X, cell.Y] = tileId;
            cell.SetTile(tileId, GetTileName(tileId));
        }

        public string GetTileName(string tileId)
        {
            if (string.IsNullOrEmpty(tileId))
                return "Air";

            DTile? tile = CampaignTileAssets.FirstOrDefault(tile => tile.ID == tileId);

            return tile == null ? $"Missing: {tileId}" : tile.Name;
        }

        private IEnumerable<DTile> CampaignTileAssets => Owner.CampaignAssets.GetAll<DTile>();

        public void SelectTile(string tileId)
        {
            SelectedTileId = tileId;
        }

        private void ResizeMap(int newWidth, int newHeight)
        {
            if (tileIds.GetLength(0) == newWidth && tileIds.GetLength(1) == newHeight)
                return;

            string[,] resized = CreateAirMap(newWidth, newHeight);

            int copyWidth = Math.Min(tileIds.GetLength(0), newWidth);
            int copyHeight = Math.Min(tileIds.GetLength(1), newHeight);

            for (int x = 0; x < copyWidth; x++)
            {
                for (int y = 0; y < copyHeight; y++)
                    resized[x, y] = tileIds[x, y];
            }

            tileIds = resized;
            RebuildCells();
        }

        private static string[,] CreateAirMap(int mapWidth, int mapHeight)
        {
            string[,] result = new string[mapWidth, mapHeight];

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                    result[x, y] = AirTileId;
            }

            return result;
        }

        private void RebuildCells()
        {
            Cells.Clear();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                    Cells.Add(new MapCellViewModel(x, y, tileIds[x, y], GetTileName(tileIds[x, y])));
            }
        }

        private void RefreshTiles()
        {
            Tiles.Clear();

            Tiles.Add(new TilePaletteItemViewModel(AirTileId, "Air", true));

            foreach (DTile tile in CampaignTileAssets.OrderBy(tile => tile.Name).ThenBy(tile => tile.ID))
                Tiles.Add(new TilePaletteItemViewModel(tile.ID, tile.Name, false));

            RefreshVisibleTiles();
            SelectedTileId = AirTileId;
        }

        private void RefreshVisibleTiles()
        {
            VisibleTiles.Clear();

            string search = SearchText.Trim();

            foreach (TilePaletteItemViewModel tile in Tiles)
            {
                if (string.IsNullOrEmpty(search)
                    || tile.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || tile.TileId.Contains(search, StringComparison.OrdinalIgnoreCase))
                {
                    VisibleTiles.Add(tile);
                }
            }
        }

        [RelayCommand]
        private void AddAirToMap()
        {
            SelectedTileId = AirTileId;
        }

        protected override bool TryBuildAsset(out IAsset? asset)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                ErrorMessage = "The ID cannot be empty.";
                asset = null;
                return false;
            }

            DMap result = new DMap(Id.Trim(), tileIds);

            JsonObject json = result.ToJson();
            json["Name"] = Name.Trim();
            json["Description"] = Description.Trim();

            result.FromJson(json);

            asset = result;
            ErrorMessage = null;
            return true;
        }
    }

    public partial class MapCellViewModel : ObservableObject
    {
        public int X { get; }

        public int Y { get; }

        [ObservableProperty]
        private string tileId;

        [ObservableProperty]
        private string tileName;

        public MapCellViewModel(int x, int y, string tileId, string tileName)
        {
            X = x;
            Y = y;
            this.tileId = tileId;
            this.tileName = tileName;
        }

        public void SetTile(string id, string name)
        {
            TileId = id;
            TileName = name;
        }
    }

    public partial class TilePaletteItemViewModel : ObservableObject
    {
        public string TileId { get; }

        public string Name { get; }

        public bool IsAir { get; }

        [ObservableProperty]
        private bool isSelected;

        public TilePaletteItemViewModel(string tileId, string name, bool isAir)
        {
            TileId = tileId;
            Name = name;
            IsAir = isAir;
        }
    }
}