using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using Tactical.Core.Domain.Units;

namespace Tactical.Editor.ViewModels
{
    public partial class StatsEditorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private decimal? hp;

        [ObservableProperty]
        private decimal? mana;

        [ObservableProperty]
        private decimal? strength;

        [ObservableProperty]
        private decimal? magic;

        [ObservableProperty]
        private decimal? defense;

        [ObservableProperty]
        private decimal? magicDefense;

        [ObservableProperty]
        private decimal? precision;

        [ObservableProperty]
        private decimal? evasion;

        [ObservableProperty]
        private decimal? movement;

        [ObservableProperty]
        private decimal? range;

        [ObservableProperty]
        private decimal? speed;

        [ObservableProperty]
        private decimal? criticalRate;

        public StatsEditorViewModel()
        {
            SetStats(new Stats());
        }

        public StatsEditorViewModel(Stats stats)
        {
            SetStats(stats);
        }

        public void SetStats(Stats stats)
        {
            Hp = stats.hp;
            Mana = stats.mana;
            Strength = stats.strength;
            Magic = stats.magic;
            Defense = stats.defense;
            MagicDefense = stats.magicDefense;
            Precision = stats.precision;
            Evasion = stats.evasion;
            Movement = stats.movement;
            Range = stats.range;
            Speed = stats.speed;
            CriticalRate = stats.criticalRate;
        }

        public Stats GetStats()
        {
            Stats stats = new Stats();
            stats.hp = Convert.ToInt32(Hp ?? 0);
            stats.mana = Convert.ToInt32(Mana ?? 0);
            stats.strength = Convert.ToInt32(Strength ?? 0);
            stats.magic = Convert.ToInt32(Magic ?? 0);
            stats.defense = Convert.ToInt32(Defense ?? 0);
            stats.magicDefense = Convert.ToInt32(MagicDefense ?? 0);
            stats.precision = Convert.ToInt32(Precision ?? 0);
            stats.evasion = Convert.ToInt32(Evasion ?? 0);
            stats.movement = Convert.ToInt32(Movement ?? 0);
            stats.range = Convert.ToInt32(Range ?? 0);
            stats.speed = Convert.ToInt32(Speed ?? 0);
            stats.criticalRate = Convert.ToInt32(CriticalRate ?? 0);
            return stats;
        }
    }
}
