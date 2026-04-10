using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.History;
using PhotoshopApp.Core.ImageProcessing;
using PhotoshopApp.Core.Layers;

namespace PhotoshopApp.Commands;

public class RotateImageCommand
{
    private readonly ILayerManager _layerManager;
    private readonly IEditHistory _editHistory;
    private readonly IImageProcessor _imageProcessor;

    public RotateImageCommand(ILayerManager layerManager, IEditHistory editHistory, IImageProcessor imageProcessor)
    {
        _layerManager = layerManager;
        _editHistory = editHistory;
        _imageProcessor = imageProcessor;
    }

    public void Execute(float degrees)
    {
        if (_layerManager.ActiveLayer?.Image == null) return;

        var action = new RotateAction(_layerManager.ActiveLayer, _imageProcessor, degrees);
        _editHistory.RecordAction(action);
        action.Execute();
    }

    private class RotateAction : IEditAction
    {
        private readonly ILayer _layer;
        private readonly IImageProcessor _processor;
        private readonly float _degrees;

        public string Description => $"Rotate {_degrees}°";

        public RotateAction(ILayer layer, IImageProcessor processor, float degrees)
        {
            _layer = layer;
            _processor = processor;
            _degrees = degrees;
        }

        public void Execute()
        {
            if (_layer.Image != null)
            {
                _layer.Image = _processor.Rotate(_layer.Image, _degrees);
            }
        }

        public void Undo()
        {
            if (_layer.Image != null)
            {
                _layer.Image = _processor.Rotate(_layer.Image, -_degrees);
            }
        }
    }
}