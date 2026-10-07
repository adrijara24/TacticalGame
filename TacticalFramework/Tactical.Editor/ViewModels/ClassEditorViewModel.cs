using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Tactical.Core;
using Tactical.Core.Domain.Items;
using Tactical.Core.Domain.Units;
using static DClass;

namespace Tactical.Editor.ViewModels
{
    public partial class ClassEditorViewModel : AssetEditorViewModel
    {
        [ObservableProperty]
        private string id = "";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private DClass.EMovementType movementType = DClass.EMovementType.GROUND;

        public StatsEditorViewModel Stats { get; }

        public ObservableCollection<WeaponProficiencyOptionViewModel> WeaponProficiencies { get; } = new();

        public DClass.EMovementType[] MovementTypes { get; } = Enum.GetValues<DClass.EMovementType>();

        public string Title => IsNew ? "New Class" : "Edit Class";

        public ClassEditorViewModel(MainViewModel owner) : this(owner, null) { }

        public ClassEditorViewModel(MainViewModel owner, DClass? @class) : base(owner, @class)
        {
            Stats = new StatsEditorViewModel(@class?.Stats ?? new Stats());

            foreach (DWeapon.WeaponType type in Enum.GetValues<DWeapon.WeaponType>().Where(type => type != DWeapon.WeaponType.NONE))
                WeaponProficiencies.Add(new WeaponProficiencyOptionViewModel(type, @class?.WeaponProficiency.Contains(type) == true));

            if (@class == null)
                return;

            Id = @class.ID;
            Name = @class.Name;
            Description = @class.Description;
            MovementType = @class.MovementType;
        }

        protected override bool TryBuildAsset(out IAsset? asset)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                ErrorMessage = "The ID cannot be empty.";
                asset = null;
                return false;
            }

            int proficiency = WeaponProficiencies.Where(option => option.IsSelected).Sum(option => (int)option.Type);

            JsonObject json = new JsonObject
            {
                ["ID"] = Id.Trim(),
                ["Name"] = Name.Trim(),
                ["Description"] = Description.Trim(),
                ["BaseStats"] = Stats.GetStats().ToJson(),
                ["WeaponProficiency"] = proficiency,
                ["MovementType"] = (int)MovementType
            };

            DClass result = new DClass();
            result.FromJson(json);

            asset = result;
            ErrorMessage = null;
            return true;
        }
    }

    public partial class WeaponProficiencyOptionViewModel : ObservableObject
    {
        public DWeapon.WeaponType Type { get; }

        [ObservableProperty]
        private bool isSelected;

        public string DisplayName => Type.ToString();

        public WeaponProficiencyOptionViewModel(DWeapon.WeaponType type, bool isSelected)
        {
            Type = type;
            this.isSelected = isSelected;
        }
    }
}
