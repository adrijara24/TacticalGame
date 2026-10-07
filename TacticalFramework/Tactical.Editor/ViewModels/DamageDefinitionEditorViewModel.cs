using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using Tactical.Core.Domain;

namespace Tactical.Editor.ViewModels
{
    public partial class DamageDefinitionEditorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string formula = "";

        [ObservableProperty]
        private EDamageType damageType = EDamageType.PHYSICAL;

        public EDamageType[] DamageTypes { get; } = Enum.GetValues<EDamageType>();

        public DamageDefinitionEditorViewModel() { }

        public DamageDefinitionEditorViewModel(DamageDefinition? damageDefinition)
        {
            if (damageDefinition == null)
                return;

            Formula = damageDefinition.Formula;
            DamageType = damageDefinition.DamageType;
        }

        public DamageDefinition BuildDefinition()
        {
            return new DamageDefinition(Formula, DamageType);
        }

        public void SetDefinition(DamageDefinition? damageDefinition)
        {
            if (damageDefinition == null)
            {
                Formula = "";
                DamageType = EDamageType.PHYSICAL;
                return;
            }

            Formula = damageDefinition.Formula;
            DamageType = damageDefinition.DamageType;
        }
    }
}
