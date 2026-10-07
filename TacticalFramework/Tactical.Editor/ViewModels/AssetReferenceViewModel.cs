using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tactical.Editor.ViewModels
{
    public partial class AssetReferenceViewModel : ViewModelBase
    {
        private readonly Action<AssetReferenceViewModel> removeCallback;

        public AssetIdPickerViewModel Picker { get; }

        public AssetReferenceViewModel(MainViewModel owner, Type assetType, string selectedId, bool allowCustomValue, Action<AssetReferenceViewModel> removeCallback)
        {
            Picker = new AssetIdPickerViewModel(owner, assetType, selectedId, allowCustomValue);
            this.removeCallback = removeCallback;
        }

        [RelayCommand]
        private void Remove()
        {
            removeCallback(this);
        }
    }
}
