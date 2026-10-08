using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Tactical.Core.Domain.Items;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;
using static Tactical.Core.Domain.Items.DWeapon;

namespace Tactical.Editor.ViewModels
{
    public partial class WeaponEditorViewModel : AssetEditorViewModel
    {
        [ObservableProperty]
        private string id;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private DWeapon.WeaponType weaponType;

        public StatsEditorViewModel Stats { get; }

        public string Title => IsNew ? "New Weapon" : "Edit Weapon";

        public WeaponEditorViewModel(MainViewModel owner) : this(owner, null)
        {
        }

        public WeaponEditorViewModel(MainViewModel owner, DWeapon? weapon) : base(owner, weapon)
        {
            id = weapon?.ID ?? "";
            name = weapon?.Name ?? "";
            description = weapon?.Description ?? "";
            weaponType = weapon?.Type ?? DWeapon.WeaponType.SWORD;
            Stats = new StatsEditorViewModel(weapon?.Stats ?? new Stats());
        }

        public DWeapon.WeaponType[] WeaponTypes { get; } = Enum.GetValues<DWeapon.WeaponType>();

        protected override bool TryBuildAsset(out IAsset? asset)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                ErrorMessage = "The ID cannot be empty.";
                asset = null;
                return false;
            }

            asset = new DWeapon(Id.Trim(), Name.Trim(), Description.Trim(), Stats.GetStats(), WeaponType);
            ErrorMessage = null;
            return true;
        }
    }
}
