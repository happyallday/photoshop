using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.Layers;

namespace PhotoshopApp.Commands;

public class LayerContextMenuCommands
{
    private readonly ILayerManager _layerManager;
    private readonly Action _updateLayersAction;

    public LayerContextMenuCommands(ILayerManager layerManager, Action updateLayersAction)
    {
        _layerManager = layerManager;
        _updateLayersAction = updateLayersAction;
    }

    public void ToggleVisibility(ILayer layer)
    {
        if (layer != null)
        {
            layer.IsVisible = !layer.IsVisible;
            _updateLayersAction?.Invoke();
        }
    }

    public void ToggleLock(ILayer layer)
    {
        if (layer != null)
        {
            layer.IsLocked = !layer.IsLocked;
        }
    }

    public void DuplicateLayer(ILayer layer)
    {
        if (layer != null && layer.Image != null)
        {
            var duplicate = new Layer
            {
                Name = $"{layer.Name} Copy",
                IsVisible = layer.IsVisible,
                Opacity = layer.Opacity,
                BlendMode = layer.BlendMode,
                IsLocked = false,
                Image = layer.Image.Clone()
            };
            
            var index = _layerManager.Layers.IndexOf(layer);
            _layerManager.AddLayer(duplicate);
            _updateLayersAction?.Invoke();
        }
    }

    public void MergeDown(ILayer layer)
    {
        if (layer != null)
        {
            var index = _layerManager.Layers.IndexOf(layer);
            if (index > 0)
            {
                var lowerLayer = _layerManager.Layers[index - 1];
                // TODO: Implement merging logic
            }
        }
    }

    public void MergeVisible()
    {
        var visibleLayers = _layerManager.Layers.Where(l => l.IsVisible).ToList();
        if (visibleLayers.Count >= 2)
        {
            // TODO: Implement visible layers merging
        }
    }

    public void FlattenImage()
    {
        // TODO: Implement flatten image functionality
    }

    public void ClearLayer(ILayer layer)
    {
        if (layer != null && !layer.IsLocked)
        {
            if (layer.Image != null)
            {
                layer.Image.Mutate(x => x.Fill(SixLabors.ImageSharp.Color.Transparent));
            }
        }
    }

    public void DeleteLayer(ILayer layer)
    {
        if (layer != null)
        {
            _layerManager.RemoveLayer(layer);
            _updateLayersAction?.Invoke();
        }
    }
}