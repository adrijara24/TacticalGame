using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using Tactical.Core.Domain;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;

namespace Tactical.Editor.ViewModels
{
    public partial class AbilityEditorViewModel : AssetEditorViewModel
    {
        [ObservableProperty]
        private string id = "";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private EAbilityType abilityType = EAbilityType.ACTIVE;

        [ObservableProperty]
        private int manaCost;

        [ObservableProperty]
        private int minRange = 1;

        [ObservableProperty]
        private int maxRange = 1;

        [ObservableProperty]
        private EAbilityTarget target = EAbilityTarget.ENEMY;

        public EAbilityType[] AbilityTypes { get; } = Enum.GetValues<EAbilityType>();

        public EAbilityTarget[] Targets { get; } = Enum.GetValues<EAbilityTarget>();

        public ObservableCollection<AbilityAreaPointViewModel> Area { get; } = new();

        public ObservableCollection<CombatActionEditorViewModel> Actions { get; } = new();

        public string Title => IsNew ? "New Ability" : "Edit Ability";

        public AbilityEditorViewModel(MainViewModel owner) : this(owner, null) { }

        public AbilityEditorViewModel(MainViewModel owner, DAbility? ability) : base(owner, ability)
        {
            if (ability == null)
                return;

            Id = ability.ID;
            Name = ability.Name;
            Description = ability.Description;
            AbilityType = ability.AbilityType;
            ManaCost = ability.ManaCost;
            MinRange = ability.MinRange;
            MaxRange = ability.MaxRange;
            Target = ability.Target;

            foreach (Vector3 point in ability.Area)
                Area.Add(new AbilityAreaPointViewModel(point, RemoveAreaPoint));

            foreach (DAbilityAction action in ability.actions)
                Actions.Add(new CombatActionEditorViewModel(owner, Enum.GetValues<EAbilityTrigger>(), action.trigger, action.damageDefinition, action.stats, action.addEffects, action.remEffects, true, RemoveAction));
        }

        [RelayCommand]
        private void AddAreaPoint()
        {
            Area.Add(new AbilityAreaPointViewModel(Vector3.Zero, RemoveAreaPoint));
        }

        private void RemoveAreaPoint(AbilityAreaPointViewModel point)
        {
            Area.Remove(point);
        }

        [RelayCommand]
        private void AddAction()
        {
            Actions.Add(new CombatActionEditorViewModel(Owner, Enum.GetValues<EAbilityTrigger>(), EAbilityTrigger.ONUSE, new DamageDefinition(), new Stats(), Array.Empty<string>(), Array.Empty<string>(), true, RemoveAction));
        }

        private void RemoveAction(CombatActionEditorViewModel action)
        {
            Actions.Remove(action);
        }

        protected override bool TryBuildAsset(out IAsset? asset)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                ErrorMessage = "The ID cannot be empty.";
                asset = null;
                return false;
            }

            if (ManaCost < 0)
            {
                ErrorMessage = "Mana cost cannot be negative.";
                asset = null;
                return false;
            }

            if (MinRange < 0 || MaxRange < 0)
            {
                ErrorMessage = "Range cannot be negative.";
                asset = null;
                return false;
            }

            if (MinRange > MaxRange)
            {
                ErrorMessage = "Minimum range cannot be greater than maximum range.";
                asset = null;
                return false;
            }

            DAbility result = new DAbility();

            result.abilityID = Id.Trim();
            result.name = Name.Trim();
            result.description = Description.Trim();
            result.type = AbilityType;
            result.manaCost = ManaCost;
            result.minRange = MinRange;
            result.maxRange = MaxRange;
            result.target = Target;

            result.area.Clear();

            foreach (AbilityAreaPointViewModel point in Area)
                result.area.Add(point.Build());

            result.actions.Clear();

            foreach (CombatActionEditorViewModel action in Actions)
                result.actions.Add(action.BuildAbilityAction());

            asset = result;
            ErrorMessage = null;
            return true;
        }
    }

    public partial class AbilityAreaPointViewModel : ObservableObject
    {
        [ObservableProperty]
        private decimal? x;

        [ObservableProperty]
        private decimal? y;

        [ObservableProperty]
        private decimal? z;

        private readonly Action<AbilityAreaPointViewModel> removeCallback;

        public AbilityAreaPointViewModel(Vector3 point, Action<AbilityAreaPointViewModel> removeCallback)
        {
            x = (decimal)point.X;
            y = (decimal)point.Y;
            z = (decimal)point.Z;
            this.removeCallback = removeCallback;
        }

        [RelayCommand]
        private void Remove()
        {
            removeCallback(this);
        }

        public Vector3 Build()
        {
            return new Vector3((float)(X ?? 0), (float)(Y ?? 0), (float)(Z ?? 0));
        }
    }
}
