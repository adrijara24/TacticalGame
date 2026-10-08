using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Tactical.Core.Persistence;

namespace Tactical.Editor.ViewModels
{
    public partial class AssetIdPickerViewModel : ViewModelBase
    {
        private readonly MainViewModel owner;
        private readonly Type assetType;

        [ObservableProperty]
        private string selectedId = "";

        [ObservableProperty]
        private string inputText = "";

        [ObservableProperty]
        private bool allowCustomValue;

        public ObservableCollection<string> ResourceIds { get; } = new();

        public AssetIdPickerViewModel(MainViewModel owner, Type assetType, string selectedId = "", bool allowCustomValue = false)
        {
            if (!typeof(IAsset).IsAssignableFrom(assetType))
                throw new ArgumentException("The selected type must implement IAsset.", nameof(assetType));

            this.owner = owner;
            this.assetType = assetType;
            this.allowCustomValue = allowCustomValue;
            this.selectedId = selectedId;
            inputText = selectedId;

            Refresh();
        }

        partial void OnInputTextChanged(string value)
        {
            if (AllowCustomValue)
                SelectedId = value;
        }

        partial void OnSelectedIdChanged(string value)
        {
            InputText = value;
        }

        public void Refresh()
        {
            ResourceIds.Clear();

            foreach (IAsset asset in owner.CampaignAssets.GetAllAssets())
            {
                if (assetType.IsInstanceOfType(asset))
                    ResourceIds.Add(asset.ID);
            }

            string[] sortedIds = ResourceIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToArray();

            ResourceIds.Clear();

            foreach (string id in sortedIds)
                ResourceIds.Add(id);

            if (!string.IsNullOrWhiteSpace(SelectedId) && !ResourceIds.Contains(SelectedId))
                ResourceIds.Add(SelectedId);
        }
    }
}
