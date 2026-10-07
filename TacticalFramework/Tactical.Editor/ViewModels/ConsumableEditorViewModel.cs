using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Tactical.Core;
using Tactical.Core.Domain.Units;

namespace Tactical.Editor.ViewModels
{
    public partial class ConsumableEditorViewModel : AssetEditorViewModel
    {
        [ObservableProperty]
        private string id = "";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private int uses = 1;

        public AssetIdPickerViewModel EffectPicker { get; }

        public string Title => IsNew ? "New Consumable" : "Edit Consumable";

        public ConsumableEditorViewModel(MainViewModel owner) : this(owner, null) { }

        public ConsumableEditorViewModel(MainViewModel owner, DItemConsumable? consumable) : base(owner, consumable)
        {
            string effectId = consumable?.Effect ?? "";
            EffectPicker = new AssetIdPickerViewModel(owner, typeof(DEffect), effectId);

            if (consumable == null)
                return;

            Id = consumable.ID;
            Name = consumable.Name;
            Description = consumable.Description;
            Uses = consumable.Uses;
        }

        protected override bool TryBuildAsset(out IAsset? asset)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                ErrorMessage = "The ID cannot be empty.";
                asset = null;
                return false;
            }

            if (Uses < 1)
            {
                ErrorMessage = "Uses must be at least 1.";
                asset = null;
                return false;
            }

            asset = new DItemConsumable(Id, Name, Description, Uses, EffectPicker.SelectedId);
            return true;
        }
    }
}
