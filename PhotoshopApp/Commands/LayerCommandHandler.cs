using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.History;
using PhotoshopApp.Core.Layers;
using System;

namespace PhotoshopApp.Commands;

public class LayerCommandHandler
{
    private readonly ILayerManager _layerManager;
    private readonly Action _updateLayersAction;

    public LayerCommandHandler(ILayerManager layerManager, Action updateLayersAction)
    {
        _layerManager = layerManager;
        _updateLayersAction = updateLayersAction;
    }

    public void AddNewLayer()
    {
        var layer = new Layer
        {
            Name = $"Layer {_layerManager.Layers.Count + 1}",
            IsVisible = true,
            Opacity = 1.0,
            BlendMode = BlendMode.Normal,
            IsLocked = false
        };
        
        _layerManager.AddLayer(layer);
        _updateLayersAction?.Invoke();
    }

    public void DeleteActiveLayer()
    {
        if (_layerManager.ActiveLayer != null && !_layerManager.ActiveLayer.IsLocked)
        {
            _layerManager.RemoveLayer(_layerManager.ActiveLayer);
            _updateLayersAction?.Invoke();
        }
    }

    public void MoveLayerUp(ILayer layer)
    {
        var currentIndex = _layerManager.Layers.IndexOf(layer);
        if (currentIndex > 0)
        {
            _layerManager.MoveLayer(layer, currentIndex - 1);
            _updateLayersAction?.Invoke();
        }
    }

    public void MoveLayerDown(ILayer layer)
    {
        var currentIndex = _layerManager.Layers.IndexOf(layer);
        if (currentIndex < _layerManager.Layers.Count - 1)
        {
            _layerManager.MoveLayer(layer, currentIndex + 1);
            _updateLayersAction?.Invoke();
        }
    }

    public void SetLayerVisibility(ILayer layer, bool isVisible)
    {
        layer.IsVisible = isVisible;
    }

    public void SetLayerOpacity(ILayer layer, double opacity)
    {
        layer.Opacity = Math.Max(0, Math.Min(1, opacity));
    }
}