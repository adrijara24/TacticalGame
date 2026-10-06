using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Tactical.Core;

namespace Tactical.Editor.ViewModels
{
    public abstract partial class AssetEditorViewModel : ViewModelBase
    {
        protected MainViewModel Owner { get; }
        protected IAsset? OriginalAsset { get; }

        public bool IsNew => OriginalAsset == null;

        [ObservableProperty]
        private string? errorMessage;

        protected AssetEditorViewModel(MainViewModel owner, IAsset? originalAsset)
        {
            Owner = owner;
            OriginalAsset = originalAsset;
        }

        public IAsset? GetOriginalAsset()
        {
            return OriginalAsset;
        }

        [RelayCommand]
        private void Save()
        {
            if (TryBuildAsset(out IAsset? asset))
                Owner.CommitAsset(this, asset!);
        }

        [RelayCommand]
        private void Cancel()
        {
            Owner.CloseEditor(this);
        }

        protected abstract bool TryBuildAsset(out IAsset? asset);
    }
}
