using PhotoshopApp.Core.History;
using PhotoshopApp.Core.ImageProcessing;
using PhotoshopApp.Core.Layers;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Commands;

public class FlipImageCommand
{
    private readonly ILayerManager _layerManager;
    private readonly IEditHistory _editHistory;
    private readonly IImageProcessor _imageProcessor;

    public FlipImageCommand(ILayerManager layerManager, IEditHistory editHistory, IImageProcessor imageProcessor)
    {
        _layerManager = layerManager;
        _editHistory = editHistory;
        _imageProcessor = imageProcessor;
    }

    public void Execute(FlipMode mode)
    {
        if (_layerManager.ActiveLayer?.Image == null) return;

        var action = new FlipAction(_layerManager.ActiveLayer, _imageProcessor, mode);
        _editHistory.RecordAction(action);
        action.Execute();
    }

    private class FlipAction : IEditAction
    {
        private readonly ILayer _layer;
        private readonly IImageProcessor _processor;
        private readonly FlipMode _mode;

        public string Description => $"Flip {_mode}";

        public FlipAction(ILayer layer, IImageProcessor processor, FlipMode mode)
        {
            _layer = layer;
            _processor = processor;
            _mode = mode;
        }

        public void Execute()
        {
            if (_layer.Image != null)
            {
                _layer.Image = _processor.Flip(_layer.Image, _mode);
            }
        }

        public void Undo()
        {
            if (_layer.Image != null)
            {
                _layer.Image = _processor.Flip(_layer.Image, _mode);
            }
        }
    }
}