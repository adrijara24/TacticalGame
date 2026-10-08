using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;

namespace Tactical.Editor.ViewModels
{
    public partial class UnitEditorViewModel : AssetEditorViewModel
    {
        [ObservableProperty]
        private string id = "";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private int lives;

        public StatsEditorViewModel Stats { get; }

        public AssetIdPickerViewModel ClassPicker { get; }

        public ObservableCollection<UnitInventorySlotViewModel> Inventory { get; } = new();

        public string Title => IsNew ? "New Unit" : "Edit Unit";

        public UnitEditorViewModel(MainViewModel owner) : this(owner, null) { }

        public UnitEditorViewModel(MainViewModel owner, DUnit? unit) : base(owner, unit)
        {
            JsonObject? source = unit?.ToJson();

            string classId = source?["ClassID"]?.GetValue<string>() ?? "";
            ClassPicker = new AssetIdPickerViewModel(owner, typeof(DClass), classId);

            Stats = new StatsEditorViewModel(unit?.Stats ?? new Stats());

            for (int i = 0; i < 5; i++)
            {
                string itemId = source?["Inventory"]?.AsObject()?["Item" + (i + 1)]?.GetValue<string>() ?? "NONE";
                Inventory.Add(new UnitInventorySlotViewModel(i + 1, new AssetIdPickerViewModel(owner, typeof(DItem), itemId)));
            }

            if (unit == null)
                return;

            Id = unit.ID;
            Name = unit.Name;
            Description = unit.Description;
            Lives = source?["Lives"]?.GetValue<int>() ?? 0;
        }

        protected override bool TryBuildAsset(out IAsset? asset)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                ErrorMessage = "The ID cannot be empty.";
                asset = null;
                return false;
            }

            JsonObject inventory = new JsonObject();

            for (int i = 0; i < Inventory.Count; i++)
                inventory["Item" + (i + 1)] = Inventory[i].Picker.SelectedId;

            JsonObject json = new JsonObject
            {
                ["ID"] = Id.Trim(),
                ["Name"] = Name.Trim(),
                ["Description"] = Description.Trim(),
                ["ClassID"] = ClassPicker.SelectedId,
                ["Stats"] = Stats.GetStats().ToJson(),
                ["Lives"] = Lives,
                ["Inventory"] = inventory
            };

            DUnit result = new DUnit();
            result.FromJson(json);

            asset = result;
            ErrorMessage = null;
            return true;
        }
    }

    public class UnitInventorySlotViewModel
    {
        public string Label { get; }

        public AssetIdPickerViewModel Picker { get; }

        public UnitInventorySlotViewModel(int slot, AssetIdPickerViewModel picker)
        {
            Label = $"Item {slot}";
            Picker = picker;
        }
    }
}
