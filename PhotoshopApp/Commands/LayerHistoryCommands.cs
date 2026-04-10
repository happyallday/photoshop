using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.History;
using PhotoshopApp.Core.Layers;

namespace PhotoshopApp.Commands;

public class LayerHistoryCommands
{
    private readonly ILayerManager _layerManager;
    private readonly IEditHistory _editHistory;
    private readonly Action _updateLayersAction;

    public LayerHistoryCommands(ILayerManager layerManager, IEditHistory editHistory, Action updateLayersAction)
    {
        _layerManager = layerManager;
        _editHistory = editHistory;
        _updateLayersAction = updateLayersAction;
    }

    public void CreateNewLayer(string name, Image<Rgba32>? image = null)
    {
        var action = new CreateLayerAction(_layerManager, name, image, _updateLayersAction);
        _editHistory.RecordAction(action);
        action.Execute();
    }

    public void MoveLayer(ILayer layer, int newIndex)
    {
        var action = new MoveLayerAction(_layerManager, layer, newIndex, _updateLayersAction);
        _editHistory.RecordAction(action);
        action.Execute();
    }

    public void ChangeLayerProperties(ILayer layer, double? newOpacity, BlendMode? newBlendMode, bool? newIsVisible)
    {
        var action = new ChangeLayerPropertiesAction(layer, newOpacity, newBlendMode, newIsVisible, _updateLayersAction);
        _editHistory.RecordAction(action);
        action.Execute();
    }

    private class CreateLayerAction : IEditAction
    {
        private readonly ILayerManager _layerManager;
        private readonly string _name;
        private readonly Image<Rgba32>? _image;
        private readonly Action _updateAction;
        private ILayer? _createdLayer;

        public string Description => $"Create layer: {_name}";

        public CreateLayerAction(ILayerManager layerManager, string name, Image<Rgba32>? image, Action updateAction)
        {
            _layerManager = layerManager;
            _name = name;
            _image = image;
            _updateAction = updateAction;
        }

        public void Execute()
        {
            _createdLayer = new Layer
            {
                Name = _name,
                IsVisible = true,
                Opacity = 1.0,
                BlendMode = BlendMode.Normal,
                Image = _image?.Clone()
            };
            
            _layerManager.AddLayer(_createdLayer);
            _layerManager.ActiveLayer = _createdLayer;
            _updateAction?.Invoke();
        }

        public void Undo()
        {
            if (_createdLayer != null)
            {
                _layerManager.RemoveLayer(_createdLayer);
                _updateAction?.Invoke();
            }
        }
    }

    private class MoveLayerAction : IEditAction
    {
        private readonly ILayerManager _layerManager;
        private readonly ILayer _layer;
        private readonly int _newIndex;
        private readonly Action _updateAction;
        private int? _oldIndex;

        public string Description => $"Move layer: {_layer.Name}";

        public MoveLayerAction(ILayerManager layerManager, ILayer layer, int newIndex, Action updateAction)
        {
            _layerManager = layerManager;
            _layer = layer;
            _newIndex = newIndex;
            _updateAction = updateAction;
        }

        public void Execute()
        {
            _oldIndex = _layerManager.Layers.IndexOf(_layer);
            if (_oldIndex.HasValue)
            {
                _layerManager.MoveLayer(_layer, _newIndex);
                _updateAction?.Invoke();
            }
        }

        public void Undo()
        {
            if (_oldIndex.HasValue)
            {
                _layerManager.MoveLayer(_layer, _oldIndex.Value);
                _updateAction?.Invoke();
            }
        }
    }

    private class ChangeLayerPropertiesAction : IEditAction
    {
        private readonly ILayer _layer;
        private readonly double? _newOpacity;
        private readonly BlendMode? _newBlendMode;
        private readonly bool? _newIsVisible;
        private readonly Action _updateAction;
        
        private double? _oldOpacity;
        private BlendMode? _oldBlendMode;
        private bool? _oldIsVisible;

        public string Description => $"Change layer properties: {_layer.Name}";

        public ChangeLayerPropertiesAction(ILayer layer, double? newOpacity, BlendMode? newBlendMode, 
            bool? newIsVisible, Action updateAction)
        {
            _layer = layer;
            _newOpacity = newOpacity;
            _newBlendMode = newBlendMode;
            _newIsVisible = newIsVisible;
            _updateAction = updateAction;
        }

        public void Execute()
        {
            _oldOpacity = _layer.Opacity;
            _oldBlendMode = _layer.BlendMode;
            _oldIsVisible = _layer.IsVisible;

            if (_newOpacity.HasValue) _layer.Opacity = _newOpacity.Value;
            if (_newBlendMode.HasValue) _layer.BlendMode = _newBlendMode.Value;
            if (_newIsVisible.HasValue) _layer.IsVisible = _newIsVisible.Value;
            
            _updateAction?.Invoke();
        }

        public void Undo()
        {
            if (_oldOpacity.HasValue) _layer.Opacity = _oldOpacity.Value;
            if (_oldBlendMode.HasValue) _layer.BlendMode = _oldBlendMode.Value;
            if (_oldIsVisible.HasValue) _layer.IsVisible = _oldIsVisible.Value;
            
            _updateAction?.Invoke();
        }
    }
}