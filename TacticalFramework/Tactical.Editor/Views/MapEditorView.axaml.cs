using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Tactical.Editor.ViewModels;

namespace Tactical.Editor.Views
{
    public partial class MapEditorView : UserControl
    {
        private static readonly DataFormat<string> TileDragFormat =
            DataFormat.CreateInProcessFormat<string>("tactical-editor-tile-id");

        public MapEditorView()
        {
            InitializeComponent();
        }

        private MapEditorViewModel? ViewModel => DataContext as MapEditorViewModel;

        private void MapCell_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (ViewModel == null)
                return;

            if (e.Source is not Control source || source.DataContext is not MapCellViewModel cell)
                return;

            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            ViewModel.SetCellTile(cell, ViewModel.SelectedTileId);
            e.Handled = true;
        }

        private void MapCell_DragOver(object? sender, DragEventArgs e)
        {
            if (e.DataTransfer.Contains(TileDragFormat))
            {
                e.DragEffects = DragDropEffects.Copy;
                return;
            }

            e.DragEffects = DragDropEffects.None;
        }

        private void MapCell_Drop(object? sender, DragEventArgs e)
        {
            if (ViewModel == null)
                return;

            if (e.Source is not Control source || source.DataContext is not MapCellViewModel cell)
            {
                e.DragEffects = DragDropEffects.None;
                return;
            }

            string? tileId = e.DataTransfer.TryGetValue(TileDragFormat);

            if (tileId == null)
            {
                e.DragEffects = DragDropEffects.None;
                return;
            }

            ViewModel.SetCellTile(cell, tileId);
            ViewModel.SelectTile(tileId);

            e.DragEffects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private async void TilePaletteItem_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Source is not Control source || source.DataContext is not TilePaletteItemViewModel tile)
                return;

            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            ViewModel?.SelectTile(tile.TileId);

            DataTransfer dataTransfer = new DataTransfer();
            dataTransfer.Add(DataTransferItem.Create(TileDragFormat, tile.TileId));

            await DragDrop.DoDragDropAsync(e, dataTransfer, DragDropEffects.Copy);

            e.Handled = true;
        }
    }
}
