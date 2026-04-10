using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.Tools;
using PhotoshopApp.Core.History;
using PhotoshopApp.Core.Layers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoshopApp.Commands;

/// <summary>
/// 图像操作命令
/// </summary>
public class ImageOperationCommands
{
    private readonly ILayerManager _layerManager;
    private readonly IEditHistory _editHistory;
    private readonly Core.Tools.ImageAdjuster _adjuster;

    public ImageOperationCommands(ILayerManager layerManager, IEditHistory editHistory, ImageAdjuster adjuster)
    {
        _layerManager = layerManager;
        _editHistory = editHistory;
        _adjuster = adjuster;
    }

    /// <summary>
    /// 应用图像调整
    /// </summary>
    public void ApplyAdjustments(ImageAdjuster.Adjustments adjustments)
    {
        if (_layerManager.ActiveLayer?.Image != null && adjustments.HasAdjustments())
        {
            var action = new AdjustmentAction(_layerManager.ActiveLayer, _adjuster, adjustments);
            _editHistory.RecordAction(action);
            action.Execute();
        }
    }

    /// <summary>
    /// 应用裁剪
    /// </summary>
    public void ApplyCrop(float x, float y, float width, float height)
    {
        if (_layerManager.ActiveLayer?.Image != null)
        {
            var action = new CropAction(_layerManager.ActiveLayer, x, y, width, height);
            _editHistory.RecordAction(action);
            action.Execute();
        }
    }

    /// <summary>
    /// 应用变换
    /// </summary>
    public void ApplyTransform(float rotation, float scaleX, float scaleY, float offsetX, float offsetY)
    {
        if (_layerManager.ActiveLayer?.Image != null)
        {
            var action = new TransformAction(_layerManager.ActiveLayer, rotation, scaleX, scaleY, offsetX, offsetY);
            _editHistory.RecordAction(action);
            action.Execute();
        }
    }

    private class AdjustmentAction : IEditAction
    {
        private readonly ILayer _layer;
        private readonly ImageAdjuster _adjuster;
        private readonly ImageAdjuster.Adjustments _adjustments;
        private Image<Rgba32>? _originalImage;

        public string Description => $"Apply adjustments to layer: {_layer.Name}";

        public AdjustmentAction(ILayer layer, ImageAdjuster adjuster, ImageAdjuster.Adjustments adjustments)
        {
            _layer = layer;
            _adjuster = adjuster;
            _adjustments = adjustments;
        }

        public void Execute()
        {
            if (_layer.Image == null) return;
            
            _originalImage = _layer.Image.Clone();
            _layer.Image = _adjuster.ApplyAdjustments(_layer.Image, _adjustments);
        }

        public void Undo()
        {
            if (_originalImage != null)
            {
                _layer.Image = _originalImage;
            }
        }
    }

    private class CropAction : IEditAction
    {
        private readonly ILayer _layer;
        private readonly float _x, _y, _width, _height;
        private Image<Rgba32>? _originalImage;

        public string Description => $"Crop layer: {_layer.Name}";

        public CropAction(ILayer layer, float x, float y, float width, float height)
        {
            _layer = layer;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public void Execute()
        {
            if (_layer.Image == null) return;
            
            _originalImage = _layer.Image.Clone();
            _layer.Image.Mutate(x => x.Crop(new SixLabors.ImageSharp.Rectangle(
                (int)_x, (int)_y, (int)_width, (int)_height)));
        }

        public void Undo()
        {
            if (_originalImage != null)
            {
                _layer.Image = _originalImage;
            }
        }
    }

    private class TransformAction : IEditAction
    {
        private readonly ILayer _layer;
        private readonly float _rotation, _scaleX, _scaleY, _offsetX, _offsetY;
        private Image<Rgba32>? _originalImage;

        public string Description => $"Transform layer: {_layer.Name}";

        public TransformAction(ILayer layer, float rotation, float scaleX, float scaleY, float offsetX, float offsetY)
        {
            _layer = layer;
            _rotation = rotation;
            _scaleX = scaleX;
            _scaleY = scaleY;
            _offsetX = offsetX;
            _offsetY = offsetY;
        }

        public void Execute()
        {
            if (_layer.Image == null) return;
            
            _originalImage = _layer.Image.Clone();
            
            var transformed = _layer.Image.Clone();
            
            if (_rotation != 0)
            {
                transformed.Mutate(x => x.Rotate(_rotation));
            }
            
            if (_scaleX != 1f || _scaleY != 1f)
            {
                var newWidth = (int)(transformed.Width * _scaleX);
                var newHeight = (int)(transformed.Height * _scaleY);
                if (newWidth > 0 && newHeight > 0)
                {
                    transformed.Mutate(x => x.Resize(newWidth, newHeight));
                }
            }
            
            _layer.Image = transformed;
        }

        public void Undo()
        {
            if (_originalImage != null)
            {
                _layer.Image = _originalImage;
            }
        }
    }
}