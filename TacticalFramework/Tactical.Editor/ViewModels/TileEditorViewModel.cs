using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Linq;
using Tactical.Core.Domain.Terrain;
using Tactical.Core.Domain.Units;
using System.Linq;
using Tactical.Core.Persistence;

namespace Tactical.Editor.ViewModels
{
    public partial class TileEditorViewModel : AssetEditorViewModel
    {
        [ObservableProperty]
        private string id = "";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private int movementCost = 1;

        public StatsEditorViewModel Stats { get; }

        public AssetIdPickerViewModel EffectPicker { get; }

        public ObservableCollection<MovementTypeOptionViewModel> MovementTypes { get; } = new();

        public string Title => IsNew ? "New Tile" : "Edit Tile";

        public TileEditorViewModel(MainViewModel owner) : this(owner, null) { }

        public TileEditorViewModel(MainViewModel owner, DTile? tile) : base(owner, tile)
        {
            Stats = new StatsEditorViewModel(tile?.Stats ?? new Stats());
            EffectPicker = new AssetIdPickerViewModel(owner, typeof(DEffect), tile?.Effect ?? "");

            foreach (DClass.EMovementType type in Enum.GetValues<DClass.EMovementType>())
            {
                bool selected = tile != null && (tile.PassableBy & (int)type) != 0;
                MovementTypes.Add(new MovementTypeOptionViewModel(type, selected));
            }

            if (tile == null)
                return;

            Id = tile.ID;
            Name = tile.Name;
            Description = tile.Description;
            MovementCost = tile.MovementCost;
        }

        protected override bool TryBuildAsset(out IAsset? asset)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                ErrorMessage = "The ID cannot be empty.";
                asset = null;
                return false;
            }

            if (MovementCost < 1)
            {
                ErrorMessage = "Movement cost must be at least 1.";
                asset = null;
                return false;
            }

            int passableBy = MovementTypes.Where(type => type.IsSelected).Sum(type => (int)type.Type);

            if (passableBy == 0)
            {
                ErrorMessage = "At least one movement type must be selected.";
                asset = null;
                return false;
            }

            DTile result = new DTile(Id.Trim(), Name.Trim(), Description.Trim(), EffectPicker.SelectedId, Stats.GetStats(), MovementCost, passableBy);

            asset = result;
            ErrorMessage = null;
            return true;
        }
    }

    public partial class MovementTypeOptionViewModel : ObservableObject
    {
        public DClass.EMovementType Type { get; }

        [ObservableProperty]
        private bool isSelected;

        public string DisplayName => Type.ToString();

        public MovementTypeOptionViewModel(DClass.EMovementType type, bool isSelected)
        {
            Type = type;
            this.isSelected = isSelected;
        }
    }
}
