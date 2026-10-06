using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tactical.Core;
using Tactical.Core.Domain;
using Tactical.Core.Domain.Items;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;

namespace Tactical.Editor.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private CampaignAssets campaignAssets;

    [ObservableProperty]
    private string currentCampaignName = "";

    [ObservableProperty]
    private string currentCampaignRoute = "";

    [ObservableProperty]
    private string statusMessage = "No campaign loaded.";

    [ObservableProperty]
    private AssetItemViewModel? selectedAsset;

    [ObservableProperty]
    private ViewModelBase? selectedEditor;

    public ObservableCollection<AssetItemViewModel> Assets { get; } = new();

    public MainViewModel()
    {
        campaignAssets = new CampaignAssets();
        RefreshAssets();
    }

    partial void OnSelectedAssetChanged(AssetItemViewModel? value)
    {
        if (value == null)
        {
            SelectedEditor = null;
            return;
        }

        switch (value.Asset)
        {
            case DWeapon weapon:
                SelectedEditor = new WeaponEditorViewModel(this, weapon);
                break;

            case DItemConsumable consumable:
                SelectedEditor = null;
                StatusMessage = $"{value.Type} editor is not implemented yet.";
                //SelectedEditor = new ConsumableEditorViewModel(this, consumable);
                break;

            default:
                SelectedEditor = null;
                StatusMessage = $"{value.Type} editor is not implemented yet.";
                break;
        }
    }

    public void NewCampaign()
    {
        campaignAssets = new CampaignAssets();
        CurrentCampaignName = "";
        CurrentCampaignRoute = "";
        SelectedAsset = null;
        SelectedEditor = null;
        StatusMessage = "New campaign.";
        RefreshAssets();
    }

    public void SaveCampaign(string campaignName, string campaignRoute)
    {
        CampaignSerializer.SaveCampaign(campaignName, campaignRoute, campaignAssets);
        CurrentCampaignName = campaignName;
        CurrentCampaignRoute = campaignRoute;
        StatusMessage = $"Campaign saved: {campaignName}";
    }

    public void LoadCampaign(CampaignAssets assets, string campaignName, string campaignRoute)
    {
        campaignAssets = assets;
        CurrentCampaignName = campaignName;
        CurrentCampaignRoute = campaignRoute;
        SelectedAsset = null;
        SelectedEditor = null;
        StatusMessage = $"Campaign loaded: {campaignName}";
        RefreshAssets();
    }

    public void CommitAsset(AssetEditorViewModel editor, IAsset asset)
    {
        if (string.IsNullOrWhiteSpace(asset.ID))
        {
            editor.ErrorMessage = "The ID cannot be empty.";
            return;
        }

        IAsset? original = editor.GetOriginalAsset();

        try
        {
            if (original == null)
            {
                if (campaignAssets.Contains<IAsset>(asset.ID))
                {
                    editor.ErrorMessage = $"An asset with ID '{asset.ID}' already exists.";
                    return;
                }

                campaignAssets.Add(asset);
            }
            else
            {
                campaignAssets.Replace<IAsset>(original.ID, asset);
            }
        }
        catch (ArgumentException exception)
        {
            editor.ErrorMessage = exception.Message;
            return;
        }
        catch (KeyNotFoundException exception)
        {
            editor.ErrorMessage = exception.Message;
            return;
        }

        editor.ErrorMessage = null;
        StatusMessage = $"{asset.GetType().Name} saved: {asset.ID}";
        SelectedAsset = null;
        SelectedEditor = null;
        RefreshAssets();
    }

    public void CloseEditor(AssetEditorViewModel editor)
    {
        SelectedAsset = null;
        SelectedEditor = null;
        StatusMessage = "Changes discarded.";
    }

    private void RefreshAssets()
    {
        Assets.Clear();

        foreach (IAsset asset in campaignAssets.GetAllAssets())
            Assets.Add(new AssetItemViewModel(asset));
    }

    [RelayCommand]
    private void AddUnit()
    {
        StatusMessage = "Unit creation is not implemented yet.";
    }

    [RelayCommand]
    private void AddClass()
    {
        StatusMessage = "Class creation is not implemented yet.";
    }

    [RelayCommand]
    private void AddWeapon()
    {
        SelectedAsset = null;
        SelectedEditor = new WeaponEditorViewModel(this);
        StatusMessage = "Creating weapon.";
    }

    [RelayCommand]
    private void AddConsumable()
    {
        SelectedAsset = null;
        //SelectedEditor = new ConsumableEditorViewModel(this);
        StatusMessage = "Creating consumable.";
    }

    [RelayCommand]
    private void AddEffect()
    {
        StatusMessage = "Effect creation is not implemented yet.";
    }

    [RelayCommand]
    private void AddAbility()
    {
        StatusMessage = "Ability creation is not implemented yet.";
    }

    [RelayCommand]
    private void AddBattle()
    {
        StatusMessage = "Battle creation is not implemented yet.";
    }
}
