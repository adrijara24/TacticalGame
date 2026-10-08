using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Threading.Tasks;
using Tactical.Editor.ViewModels;

namespace Tactical.Editor.Views
{
    public partial class MapEditorView : UserControl
    {
        private static readonly DataFormat<string> TileDragFormat = DataFormat.CreateStringApplicationFormat("tactical-editor-tile");

        private Border? dragOverBorder;

        public MapEditorView()
        {
            InitializeComponent();

            AddHandler(InputElement.PointerPressedEvent, MapCell_PointerPressed, RoutingStrategies.Bubble);
            AddHandler(InputElement.PointerPressedEvent, TilePaletteItem_PointerPressed, RoutingStrategies.Tunnel);

            DragDrop.AddDragOverHandler(this, MapCell_DragOver);
            DragDrop.AddDragLeaveHandler(this, MapCell_DragLeave);
            DragDrop.AddDropHandler(this, MapCell_Drop);
        }

        private MapEditorViewModel? ViewModel => DataContext as MapEditorViewModel;

        private void MapCell_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (ViewModel == null)
                return;

            MapCellViewModel? cell = FindDataContext<MapCellViewModel>(e.Source);

            if (cell == null)
                return;

            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            ViewModel.SetCellTile(cell, ViewModel.SelectedTileId);
            e.Handled = true;
        }

        private void TilePaletteItem_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            TilePaletteItemViewModel? tile = FindDataContext<TilePaletteItemViewModel>(e.Source);

            if (tile == null)
                return;

            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            ViewModel?.SelectTile(tile.TileId);

            _ = StartTileDrag(e, tile.TileId);
        }

        private async Task StartTileDrag(PointerPressedEventArgs e, string tileId)
        {
            DataTransfer dataTransfer = new DataTransfer();
            dataTransfer.Add(DataTransferItem.Create(TileDragFormat, tileId));

            await DragDrop.DoDragDropAsync(e, dataTransfer, DragDropEffects.Copy);
        }

        private void MapCell_DragOver(object? sender, DragEventArgs e)
        {
            MapCellViewModel? cell = FindDataContext<MapCellViewModel>(e.Source);
            string? tileId = e.DataTransfer.TryGetValue(TileDragFormat);

            if (cell == null || tileId == null)
            {
                e.DragEffects = DragDropEffects.None;
                ClearDragOverCell();
                return;
            }

            Border? border = FindParentBorder(e.Source);

            if (border != null && border != dragOverBorder)
            {
                ClearDragOverCell();
                border.Classes.Add("drop-target");
                dragOverBorder = border;
            }

            e.DragEffects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void MapCell_DragLeave(object? sender, RoutedEventArgs e)
        {
            ClearDragOverCell();
        }

        private void MapCell_Drop(object? sender, DragEventArgs e)
        {
            if (ViewModel == null)
                return;

            MapCellViewModel? cell = FindDataContext<MapCellViewModel>(e.Source);
            string? tileId = e.DataTransfer.TryGetValue(TileDragFormat);

            if (cell == null || tileId == null)
            {
                e.DragEffects = DragDropEffects.None;
                ClearDragOverCell();
                return;
            }

            ViewModel.SetCellTile(cell, tileId);
            ViewModel.SelectTile(tileId);

            e.DragEffects = DragDropEffects.Copy;
            e.Handled = true;

            ClearDragOverCell();
        }

        private void ClearDragOverCell()
        {
            if (dragOverBorder == null)
                return;

            dragOverBorder.Classes.Remove("drop-target");
            dragOverBorder = null;
        }

        private static T? FindDataContext<T>(object? source) where T : class
        {
            Visual? visual = source as Visual;

            while (visual != null)
            {
                if (visual is Control control && control.DataContext is T result)
                    return result;

                visual = visual.GetVisualParent<Visual>();
            }

            return null;
        }

        private static Border? FindParentBorder(object? source)
        {
            Visual? visual = source as Visual;

            while (visual != null)
            {
                if (visual is Border border)
                    return border;

                visual = visual.GetVisualParent<Visual>();
            }

            return null;
        }
    }
}