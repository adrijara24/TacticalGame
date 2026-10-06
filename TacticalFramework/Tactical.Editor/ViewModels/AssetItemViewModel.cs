using System;
using System.Collections.Generic;
using System.Text;
using Tactical.Core;

namespace Tactical.Editor.ViewModels
{
    public class AssetItemViewModel
    {
        public IAsset Asset { get; }

        public string Type => Asset.GetType().Name;
        public string ID => Asset.ID;
        public string Name => Asset.Name;

        public AssetItemViewModel(IAsset asset)
        {
            Asset = asset;
        }
    }
}
