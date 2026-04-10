using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.Tools;

/// <summary>
/// 移动工具
/// </summary>
public class MoveTool : ToolBase
{
    public override ToolType ToolType => ToolType.Move;
    public override string Name => "Move";
    public override string Description => "Move the selected image or layer";
    
    private Image<Rgba32>? _originalImage;
    private float _originalX, _originalY;
    
    public override void OnMouseDown(Image<Rgba32> image, float x, float y, bool isLeftButton)
    {
        base.OnMouseDown(image, x, y, isLeftButton);
        if (isLeftButton)
        {
            _originalImage = image.Clone();
            _originalX = x;
            _originalY = y;
        }
    }
    
    public override void OnMouseMove(Image<Rgba32> image, float x, float y)
    {
        base.OnMouseMove(image, x, y);
        
        if (_isMouseDown && _originalImage != null)
        {
            var deltaX = x - _originalX;
            var deltaY = y - _originalY;
            
            // Apply translation effect (simplified - just show bounds)
        }
    }
    
    public override void OnMouseUp(Image<Rgba32> image, float x, float y)
    {
        base.OnMouseUp(image, x, y);
        _originalImage = null;
    }
}

/// <summary>
/// 变换工具
/// </summary>
public class TransformTool : ToolBase
{
    public enum TransformMode
    {
        None,
        Move,
        Scale,
        Rotate
    }
    
    public override ToolType ToolType => ToolType.Selection;
    public override string Name => "Transform";
    public override string Description => "Transform the selected area (scale, rotate, move)";
    
    private TransformMode _transformMode = TransformMode.None;
    private Image<Rgba32>? _originalImage;
    private SelectionRect _selectionBounds;
    private float _scaleX = 1f;
    private float _scaleY = 1f;
    private float _rotation = 0f;
    private float _translationX = 0f;
    private float _translationY = 0f;
    
    public override void OnMouseDown(Image<Rgba32> image, float x, float y, bool isLeftButton)
    {
        base.OnMouseDown(image, x, y, isLeftButton);
        
        if (isLeftButton)
        {
            _originalImage = image.Clone();
            _selectionBounds = new SelectionRect(_startX, _startY, 
                image.Width - _startX, image.Height - _startY);
            
            _transformMode = GetTransformMode(x, y);
            _scaleX = 1f;
            _scaleY = 1f;
            _rotation = 0f;
            _translationX = 0f;
            _translationY = 0f;
        }
    }
    
    public override void OnMouseMove(Image<Rgba32> image, float x, float y)
    {
        base.OnMouseMove(image, x, y);
        
        if (_isMouseDown)
        {
            switch (_transformMode)
            {
                case TransformMode.Scale:
                    CalculateScale(x, y);
                    break;
                case TransformMode.Rotate:
                    CalculateRotation(x, y);
                    break;
                case TransformMode.Move:
                    CalculateTranslation(x, y);
                    break;
            }
        }
    }
    
    public override Image<Rgba32>? GetPreview(Image<Rgba32> originalImage)
    {
        if (!_isMouseDown || _originalImage == null)
            return null;
        
        var transformed = _originalImage.Clone();
        
        // Apply transformations
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
        
        // Draw transformation bounds
        var centerX = _selectionBounds.X + _selectionBounds.Width / 2;
        var centerY = _selectionBounds.Y + _selectionBounds.Height / 2;
        
        transformed.Mutate(x => x.Draw(
            SixLabors.ImageSharp.Color.Red,
            2f,
            new SixLabors.ImageSharp.Drawing.RectangleF(
                centerX - _selectionBounds.Width / 2 * _scaleX - _translationX,
                centerY - _selectionBounds.Height / 2 * _scaleY - _translationY,
                _selectionBounds.Width * _scaleX,
                _selectionBounds.Height * _scaleY
            )
        ));
        
        return transformed;
    }
    
    public Image<Rgba32>? ApplyTransform(Image<Rgba32> originalImage)
    {
        if (_originalImage == null)
            return null;
        
        var result = originalImage.Clone();
        
        if (_rotation != 0)
        {
            result.Mutate(x => x.Rotate(_rotation));
        }
        
        if (_scaleX != 1f || _scaleY != 1f)
        {
            var newWidth = (int)(result.Width * _scaleX);
            var newHeight = (int)(result.Height * _scaleY);
            if (newWidth > 0 && newHeight > 0)
            {
                result.Mutate(x => x.Resize(newWidth, newHeight));
            }
        }
        
        return result;
    }
    
    private TransformMode GetTransformMode(float x, float y)
    {
        // Simplified logic - in real implementation, check mouse position relative to handles
        if (x > _selectionBounds.X + _selectionBounds.Width - 20 && 
            y > _selectionBounds.Y + _selectionBounds.Height - 20)
        {
            return TransformMode.Scale;
        }
        else if (x > _selectionBounds.X + _selectionBounds.Width / 2 - 10 && 
                 x < _selectionBounds.X + _selectionBounds.Width / 2 + 10 &&
                 y > _selectionBounds.Y + _selectionBounds.Height)
        {
            return TransformMode.Rotate;
        }
        else
        {
            return TransformMode.Move;
        }
    }
    
    private void CalculateScale(float mouseX, float mouseY)
    {
        var centerRight = _selectionBounds.X + _selectionBounds.Width;
        var bottomCenter = _selectionBounds.Y + _selectionBounds.Height;
        
        _scaleX = Math.Max(0.1f, mouseX / centerRight);
        _scaleY = Math.Max(0.1f, mouseY / bottomCenter);
    }
    
    private void CalculateRotation(float mouseX, float mouseY)
    {
        var centerX = _selectionBounds.X + _selectionBounds.Width / 2;
        var centerY = _selectionBounds.Y + _selectionBounds.Height / 2;
        
        var angle = Math.Atan2(mouseY - centerY, mouseX - centerX);
        _rotation = (float)(angle * 180 / Math.PI);
    }
    
    private void CalculateTranslation(float mouseX, float mouseY)
    {
        _translationX = mouseX - _startX;
        _translationY = mouseY - _startY;
    }
}