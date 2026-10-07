using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Tactical.Core;

namespace Tactical.Editor.ViewModels
{
    public partial class AssetIdPickerViewModel : ViewModelBase
    {
        private readonly MainViewModel owner;
        private readonly Type assetType;

        [ObservableProperty]
        private string selectedId = "";

        public ObservableCollection<string> ResourceIds { get; } = new();

        public AssetIdPickerViewModel(MainViewModel owner, Type assetType, string selectedId = "")
        {
            if (!typeof(IAsset).IsAssignableFrom(assetType))
                throw new ArgumentException("The selected type must implement IAsset.", nameof(assetType));

            this.owner = owner;
            this.assetType = assetType;
            this.selectedId = selectedId;

            Refresh();
        }

        public void Refresh()
        {
            ResourceIds.Clear();

            MethodInfo? getAllMethod = typeof(CampaignAssets).GetMethod(nameof(CampaignAssets.GetAll));

            if (getAllMethod == null)
                throw new InvalidOperationException("CampaignAssets.GetAll could not be found.");

            MethodInfo typedGetAllMethod = getAllMethod.MakeGenericMethod(assetType);
            object? result = typedGetAllMethod.Invoke(owner.CampaignAssets, null);

            if (result is not IEnumerable assets)
                throw new InvalidOperationException($"Could not retrieve assets of type {assetType.Name}.");

            foreach (object asset in assets)
            {
                if (asset is IAsset resource)
                    ResourceIds.Add(resource.ID);
            }

            var sortedIds = ResourceIds.OrderBy(id => id).ToArray();
            ResourceIds.Clear();

            foreach (string id in sortedIds)
                ResourceIds.Add(id);

            if (!string.IsNullOrEmpty(SelectedId) && !ResourceIds.Contains(SelectedId))
                ResourceIds.Add(SelectedId);
        }
    }
}
