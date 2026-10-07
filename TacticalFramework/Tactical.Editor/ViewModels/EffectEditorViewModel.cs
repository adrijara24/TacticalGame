using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Tactical.Core;
using Tactical.Core.Domain;
using Tactical.Core.Domain.Units;

namespace Tactical.Editor.ViewModels
{
    public partial class EffectEditorViewModel : AssetEditorViewModel
    {
        [ObservableProperty]
        private string id = "";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private EEffectDuration duration = EEffectDuration.SINGLE;

        [ObservableProperty]
        private int turns = 1;

        [ObservableProperty]
        private EEffectStacking stacking = EEffectStacking.IGNORE;

        [ObservableProperty]
        private int maxStacks = 1;

        public EEffectDuration[] Durations { get; } = Enum.GetValues<EEffectDuration>();

        public EEffectStacking[] Stackings { get; } = Enum.GetValues<EEffectStacking>();

        public ObservableCollection<CombatActionEditorViewModel> Actions { get; } = new();

        public string Title => IsNew ? "New Effect" : "Edit Effect";

        public EffectEditorViewModel(MainViewModel owner) : this(owner, null) { }

        public EffectEditorViewModel(MainViewModel owner, DEffect? effect) : base(owner, effect)
        {
            if (effect == null)
                return;

            Id = effect.ID;
            Name = effect.Name;
            Description = effect.Description;
            Duration = effect.Duration;
            Turns = effect.Turns;
            Stacking = effect.Stacking;
            MaxStacks = effect.MaxStacks;

            JsonObject source = effect.ToJson();

            foreach (JsonNode? node in source["Actions"]!.AsArray())
            {
                JsonObject actionJson = node!.AsObject();

                DamageDefinition? damage = null;

                if (actionJson["Damage"] != null)
                {
                    damage = new DamageDefinition();
                    damage.FromJson(actionJson["Damage"]!.AsObject());
                }

                Stats stats = new Stats();
                stats.FromJson(actionJson["Stats"]!.AsObject());

                List<string> addEffects = new List<string>();

                foreach (JsonNode? effectNode in actionJson["AddEffects"]!.AsArray())
                    addEffects.Add(effectNode!.GetValue<string>());

                List<string> removeEffects = new List<string>();

                foreach (JsonNode? effectNode in actionJson["RemEffects"]!.AsArray())
                    removeEffects.Add(effectNode!.GetValue<string>());

                EEffectTrigger trigger = (EEffectTrigger)actionJson["EffectTrigger"]!.GetValue<int>();

                Actions.Add(new CombatActionEditorViewModel(owner, Enum.GetValues<EEffectTrigger>(), trigger, damage, stats, addEffects, removeEffects, false, RemoveAction));
            }
        }

        [RelayCommand]
        private void AddAction()
        {
            Actions.Add(new CombatActionEditorViewModel(Owner, Enum.GetValues<EEffectTrigger>(), EEffectTrigger.ONAPPLY, null, new Stats(), Array.Empty<string>(), Array.Empty<string>(), false, RemoveAction));
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

            if (Duration == EEffectDuration.TURNS && Turns < 1)
            {
                ErrorMessage = "Turn duration must be at least 1.";
                asset = null;
                return false;
            }

            if (Stacking == EEffectStacking.STACK && MaxStacks < 1)
            {
                ErrorMessage = "Maximum stacks must be at least 1.";
                asset = null;
                return false;
            }

            JsonArray actionsJson = new JsonArray();

            foreach (CombatActionEditorViewModel actionEditor in Actions)
            {
                DEffectAction action = actionEditor.BuildEffectAction();

                JsonObject actionJson = new JsonObject
                {
                    ["EffectTrigger"] = (int)action.trigger,
                    ["Stats"] = action.stats.ToJson(),
                    ["AddEffects"] = new JsonArray(),
                    ["RemEffects"] = new JsonArray()
                };

                if (action.damageDefinition != null)
                    actionJson["Damage"] = action.damageDefinition.ToJson();

                foreach (string effectId in action.addEffects)
                    actionJson["AddEffects"]!.AsArray().Add(effectId);

                foreach (string pattern in action.removeEffects)
                    actionJson["RemEffects"]!.AsArray().Add(pattern);

                actionsJson.Add(actionJson);
            }

            JsonObject json = new JsonObject
            {
                ["ID"] = Id.Trim(),
                ["Name"] = Name.Trim(),
                ["Description"] = Description.Trim(),
                ["Duration"] = new JsonObject
                {
                    ["EffectDuration"] = (int)Duration,
                    ["Turns"] = Turns
                },
                ["Stacking"] = new JsonObject
                {
                    ["EffectStacking"] = (int)Stacking,
                    ["MaxStacks"] = MaxStacks
                },
                ["Actions"] = actionsJson
            };

            DEffect result = new DEffect();
            result.FromJson(json);

            asset = result;
            ErrorMessage = null;
            return true;
        }
    }
}
