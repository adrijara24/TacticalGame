using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using Tactical.Core;
using Tactical.Core.Domain;
using Tactical.Core.Domain.Units;

namespace Tactical.Editor.ViewModels
{
    public partial class BattleEditorViewModel : AssetEditorViewModel
    {
        [ObservableProperty]
        private string id = "";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private string mapId = "";

        public ObservableCollection<BattleUnitPositionViewModel> Team1 { get; } = new();

        public ObservableCollection<BattleUnitPositionViewModel> Team2 { get; } = new();

        public string Title => IsNew ? "New Battle" : "Edit Battle";

        public BattleEditorViewModel(MainViewModel owner) : this(owner, null) { }

        public BattleEditorViewModel(MainViewModel owner, DBattle? battle) : base(owner, battle)
        {
            if (battle == null)
                return;

            Id = battle.ID;
            Name = battle.Name;
            Description = battle.Description;
            MapId = battle.mapID;

            foreach (var unit in battle.team1)
                Team1.Add(new BattleUnitPositionViewModel(owner, unit.Key, unit.Value, RemoveTeam1Unit));

            foreach (var unit in battle.team2)
                Team2.Add(new BattleUnitPositionViewModel(owner, unit.Key, unit.Value, RemoveTeam2Unit));
        }

        [RelayCommand]
        private void AddTeam1Unit()
        {
            Team1.Add(new BattleUnitPositionViewModel(Owner, "", Vector3.Zero, RemoveTeam1Unit));
        }

        private void RemoveTeam1Unit(BattleUnitPositionViewModel unit)
        {
            Team1.Remove(unit);
        }

        [RelayCommand]
        private void AddTeam2Unit()
        {
            Team2.Add(new BattleUnitPositionViewModel(Owner, "", Vector3.Zero, RemoveTeam2Unit));
        }

        private void RemoveTeam2Unit(BattleUnitPositionViewModel unit)
        {
            Team2.Remove(unit);
        }

        protected override bool TryBuildAsset(out IAsset? asset)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                ErrorMessage = "The ID cannot be empty.";
                asset = null;
                return false;
            }

            DBattle result = new DBattle(Id.Trim(), Name.Trim(), Description.Trim());
            result.mapID = MapId.Trim();

            result.team1.Clear();

            foreach (BattleUnitPositionViewModel unit in Team1)
            {
                if (string.IsNullOrWhiteSpace(unit.UnitPicker.SelectedId))
                {
                    ErrorMessage = "Every Team 1 entry must have a unit selected.";
                    asset = null;
                    return false;
                }

                result.team1.Add(new System.Collections.Generic.KeyValuePair<string, Vector3>(unit.UnitPicker.SelectedId, unit.GetPosition()));
            }

            result.team2.Clear();

            foreach (BattleUnitPositionViewModel unit in Team2)
            {
                if (string.IsNullOrWhiteSpace(unit.UnitPicker.SelectedId))
                {
                    ErrorMessage = "Every Team 2 entry must have a unit selected.";
                    asset = null;
                    return false;
                }

                result.team2.Add(new System.Collections.Generic.KeyValuePair<string, Vector3>(unit.UnitPicker.SelectedId, unit.GetPosition()));
            }

            asset = result;
            ErrorMessage = null;
            return true;
        }
    }

    public partial class BattleUnitPositionViewModel : ViewModelBase
    {
        public AssetIdPickerViewModel UnitPicker { get; }

        [ObservableProperty]
        private decimal? x;

        [ObservableProperty]
        private decimal? y;

        [ObservableProperty]
        private decimal? z;

        private readonly Action<BattleUnitPositionViewModel> removeCallback;

        public BattleUnitPositionViewModel(MainViewModel owner, string unitId, Vector3 position, Action<BattleUnitPositionViewModel> removeCallback)
        {
            UnitPicker = new AssetIdPickerViewModel(owner, typeof(DUnit), unitId);
            x = (decimal)position.X;
            y = (decimal)position.Y;
            z = (decimal)position.Z;
            this.removeCallback = removeCallback;
        }

        [RelayCommand]
        private void Remove()
        {
            removeCallback(this);
        }

        public Vector3 GetPosition()
        {
            return new Vector3((float)(X ?? 0), (float)(Y ?? 0), (float)(Z ?? 0));
        }
    }
}
