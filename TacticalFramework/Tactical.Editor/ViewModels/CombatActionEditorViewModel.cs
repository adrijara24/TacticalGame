using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Tactical.Core.Domain;
using Tactical.Core.Domain.Units;

namespace Tactical.Editor.ViewModels
{
    public partial class CombatActionEditorViewModel : ViewModelBase
    {
        private readonly MainViewModel owner;
        private readonly bool damageRequired;
        private readonly Action<CombatActionEditorViewModel> removeCallback;

        [ObservableProperty]
        private Enum trigger;

        [ObservableProperty]
        private bool hasDamage;

        public Array TriggerOptions { get; }

        public bool DamageOptional => !damageRequired;

        public StatsEditorViewModel Stats { get; }

        public DamageDefinitionEditorViewModel Damage { get; }

        public ObservableCollection<AssetReferenceViewModel> AddEffects { get; } = new();

        public ObservableCollection<AssetReferenceViewModel> RemoveEffects { get; } = new();

        public CombatActionEditorViewModel(MainViewModel owner, Array triggerOptions, Enum trigger, DamageDefinition? damageDefinition, Stats stats, IEnumerable<string> addEffects, IEnumerable<string> removeEffects, bool damageRequired, Action<CombatActionEditorViewModel> removeCallback)
        {
            this.owner = owner;
            this.damageRequired = damageRequired;
            this.removeCallback = removeCallback;

            TriggerOptions = triggerOptions;
            this.trigger = trigger;
            HasDamage = damageRequired || damageDefinition != null;

            Stats = new StatsEditorViewModel(stats);
            Damage = new DamageDefinitionEditorViewModel(damageDefinition);

            foreach (string effectId in addEffects)
                AddEffects.Add(new AssetReferenceViewModel(owner, typeof(DEffect), effectId, false, RemoveAddEffect));

            foreach (string pattern in removeEffects)
                RemoveEffects.Add(new AssetReferenceViewModel(owner, typeof(DEffect), pattern, true, RemoveRemoveEffect));
        }

        [RelayCommand]
        private void AddAddEffect()
        {
            AddEffects.Add(new AssetReferenceViewModel(owner, typeof(DEffect), "", false, RemoveAddEffect));
        }

        private void RemoveAddEffect(AssetReferenceViewModel reference)
        {
            AddEffects.Remove(reference);
        }

        [RelayCommand]
        private void AddRemoveEffect()
        {
            RemoveEffects.Add(new AssetReferenceViewModel(owner, typeof(DEffect), "", true, RemoveRemoveEffect));
        }

        private void RemoveRemoveEffect(AssetReferenceViewModel reference)
        {
            RemoveEffects.Remove(reference);
        }

        [RelayCommand]
        private void Remove()
        {
            removeCallback(this);
        }

        public DEffectAction BuildEffectAction()
        {
            DEffectAction action = new DEffectAction((EEffectTrigger)Trigger);
            action.damageDefinition = HasDamage ? Damage.BuildDefinition() : null;
            action.stats = Stats.GetStats();

            foreach (AssetReferenceViewModel reference in AddEffects)
            {
                if (!string.IsNullOrWhiteSpace(reference.Picker.SelectedId))
                    action.addEffects.Add(reference.Picker.SelectedId);
            }

            foreach (AssetReferenceViewModel reference in RemoveEffects)
            {
                if (!string.IsNullOrWhiteSpace(reference.Picker.SelectedId))
                    action.removeEffects.Add(reference.Picker.SelectedId);
            }

            return action;
        }

        public DAbilityAction BuildAbilityAction()
        {
            DAbilityAction action = new DAbilityAction((EAbilityTrigger)Trigger);
            action.damageDefinition = Damage.BuildDefinition();
            action.stats = Stats.GetStats();

            foreach (AssetReferenceViewModel reference in AddEffects)
            {
                if (!string.IsNullOrWhiteSpace(reference.Picker.SelectedId))
                    action.addEffects.Add(reference.Picker.SelectedId);
            }

            foreach (AssetReferenceViewModel reference in RemoveEffects)
            {
                if (!string.IsNullOrWhiteSpace(reference.Picker.SelectedId))
                    action.remEffects.Add(reference.Picker.SelectedId);
            }

            return action;
        }
    }
}
