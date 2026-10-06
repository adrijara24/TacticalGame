using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using Tactical.Core;
using Tactical.Core.Domain.Items;
using Tactical.Core.Persistence;
using Tactical.Editor.ViewModels;
using Tactical.Editor.Views.Dialogs;
using System.Linq;

namespace Tactical.Editor.Views;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void NewCampaign_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.NewCampaign();
    }

    private async void SaveCampaign_Click(object? sender, RoutedEventArgs e)
    {
        string campaignName = ViewModel.CurrentCampaignName;
        string campaignRoute = ViewModel.CurrentCampaignRoute;

        if (string.IsNullOrWhiteSpace(campaignName) || string.IsNullOrWhiteSpace(campaignRoute))
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Select campaign parent folder", AllowMultiple = false });

            if (folders.Count == 0)
                return;

            campaignRoute = folders[0].Path.LocalPath;

            var dialog = new TextInputDialog("New Campaign", "Campaign name:");
            string? result = await dialog.ShowDialog<string?>(this);

            if (string.IsNullOrWhiteSpace(result))
                return;

            campaignName = result;
        }

        try
        {
            ViewModel.SaveCampaign(campaignName, campaignRoute);
        }
        catch (Exception ex)
        {
            ViewModel.StatusMessage = $"Save failed: {ex.Message}";
        }
    }

    private async void LoadCampaign_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Select campaign folder", AllowMultiple = false });

        if (folders.Count == 0)
            return;

        string campaignDirectory = folders[0].Path.LocalPath;
        string indexPath = Path.Combine(campaignDirectory, "index.json");

        if (!File.Exists(indexPath))
        {
            ViewModel.StatusMessage = "The selected folder does not contain index.json.";
            return;
        }

        DirectoryInfo directory = new DirectoryInfo(campaignDirectory);
        string campaignName = directory.Name;
        string campaignRoute = directory.Parent?.FullName ?? "";

        try
        {
            CampaignAssets assets = CampaignSerializer.LoadCampaign(campaignName, campaignRoute);
            ViewModel.LoadCampaign(assets, campaignName, campaignRoute);
        }
        catch (Exception ex)
        {
            ViewModel.StatusMessage = $"Load failed: {ex.Message}";
        }
    }
}