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

    public CampaignAssets CampaignAssets => campaignAssets;

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
                SelectedEditor = new ConsumableEditorViewModel(this, consumable);
                break;

            case DClass @class:
                SelectedEditor = new ClassEditorViewModel(this, @class);
                break;

            case DUnit unit:
                SelectedEditor = new UnitEditorViewModel(this, unit);
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
        CampaignAssets normalizedAssets = new CampaignAssets();

        foreach (IAsset asset in assets.GetAllAssets())
            AddAssetTyped(normalizedAssets, asset);

        campaignAssets = normalizedAssets;
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
                if (ContainsAssetId(asset.ID))
                {
                    editor.ErrorMessage = $"An asset with ID '{asset.ID}' already exists.";
                    return;
                }

                AddAssetTyped(campaignAssets, asset);
            }
            else
            {
                if (original.ID != asset.ID && ContainsAssetId(asset.ID))
                {
                    editor.ErrorMessage = $"An asset with ID '{asset.ID}' already exists.";
                    return;
                }

                ReplaceAssetTyped(campaignAssets, original.ID, asset);
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

    private static void AddAssetTyped(CampaignAssets assets, IAsset asset)
    {
        switch (asset)
        {
            case DUnit unit:
                assets.Add(unit);
                break;

            case DClass @class:
                assets.Add(@class);
                break;

            case DWeapon weapon:
                assets.Add(weapon);
                break;

            case DItemConsumable consumable:
                assets.Add(consumable);
                break;

            case DEffect effect:
                assets.Add(effect);
                break;

            case DAbility ability:
                assets.Add(ability);
                break;

            case DBattle battle:
                assets.Add(battle);
                break;

            default:
                throw new InvalidOperationException($"Unsupported asset type '{asset.GetType().Name}'.");
        }
    }

    private static void ReplaceAssetTyped(CampaignAssets assets, string originalID, IAsset asset)
    {
        switch (asset)
        {
            case DUnit unit:
                assets.Replace(originalID, unit);
                break;

            case DClass @class:
                assets.Replace(originalID, @class);
                break;

            case DWeapon weapon:
                assets.Replace(originalID, weapon);
                break;

            case DItemConsumable consumable:
                assets.Replace(originalID, consumable);
                break;

            case DEffect effect:
                assets.Replace(originalID, effect);
                break;

            case DAbility ability:
                assets.Replace(originalID, ability);
                break;

            case DBattle battle:
                assets.Replace(originalID, battle);
                break;

            default:
                throw new InvalidOperationException($"Unsupported asset type '{asset.GetType().Name}'.");
        }
    }

    private bool ContainsAssetId(string id)
    {
        foreach (IAsset asset in campaignAssets.GetAllAssets())
        {
            if (asset.ID == id)
                return true;
        }

        return false;
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
        SelectedAsset = null;
        SelectedEditor = new UnitEditorViewModel(this);
        StatusMessage = "Creating unit.";
    }

    [RelayCommand]
    private void AddClass()
    {
        SelectedAsset = null;
        SelectedEditor = new ClassEditorViewModel(this);
        StatusMessage = "Creating class.";
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
        SelectedEditor = new ConsumableEditorViewModel(this);
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