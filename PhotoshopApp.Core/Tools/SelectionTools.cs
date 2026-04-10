using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.Tools;

/// <summary>
/// 选择工具
/// </summary>
public class SelectionTool : ToolBase
{
    public override ToolType ToolType => ToolType.Selection;
    public override string Name => "Selection";
    public override string Description => "Select rectangular areas of the image";
    
    public override void OnMouseMove(Image<Rgba32> image, float x, float y)
    {
        base.OnMouseMove(image, x, y);
    }
    
    public override void OnMouseUp(Image<Rgba32> image, float x, float y)
    {
        base.OnMouseUp(image, x, y);
        
        // Clamp coordinates to image bounds
        var clampedX1 = Math.Max(0, Math.Min(_startX, image.Width - 1));
        var clampedY1 = Math.Max(0, Math.Min(_startY, image.Height - 1));
        var clampedX2 = Math.Max(0, Math.Min(_currentX, image.Width - 1));
        var clampedY2 = Math.Max(0, Math.Min(_currentY, image.Height - 1));
        
        var x1 = Math.Min(clampedX1, clampedX2);
        var y1 = Math.Min(clampedY1, clampedY2);
        var width = Math.Abs(clampedX2 - clampedX1);
        var height = Math.Abs(clampedY2 - clampedY1);
        
        if (width > 1 && height > 1)
        {
            CurrentSelection = new SelectionRect(x1, y1, width, height);
        }
    }
    
    public override SelectionRect? GetSelection()
    {
        return CurrentSelection;
    }
    
    public SelectionRect? CurrentSelection { get; private set; }
    
    public override Image<Rgba32>? GetPreview(Image<Rgba32> originalImage)
    {
        if (!_isMouseDown || CurrentSelection == null)
        {
            return null;
        }
        
        var preview = originalImage.Clone();
        
        // Draw selection rectangle
        var x1 = Math.Min(_startX, _currentX);
        var y1 = Math.Min(_startY, _currentY);
        var width = Math.Abs(_currentX - _startX);
        var height = Math.Abs(_currentY - _startY);
        
        if (width > 0 && height > 0)
        {
            preview.Mutate(x => x.Draw(
                SixLabors.ImageSharp.Color.White, 
                1f, 
                new SixLabors.ImageSharp.Drawing.RectangleF(x1, y1, width, height)
            ));
            
            preview.Mutate(x => x.Draw(
                SixLabors.ImageSharp.Color.Red, 
                1f, 
                new SixLabors.ImageSharp.Drawing.Pen(
                    SixLabors.ImageSharp.Color.White, 
                    2f),
                new SixLabors.ImageSharp.Drawing.RectangleF(x1, y1, width, height)
            ));
        }
        
        return preview;
    }
}

/// <summary>
/// 裁剪工具
/// </summary>
public class CropTool : SelectionTool
{
    public override ToolType ToolType => ToolType.Crop;
    public override string Name => "Crop";
    public override string Description => "Crop the image to the selected area";
    
    public Image<Rgba32>? ApplyCrop(Image<Rgba32> originalImage)
    {
        if (CurrentSelection == null || !CurrentSelection.Value.IsValid)
            return null;
        
        var selection = CurrentSelection.Value;
        var x = (int)selection.X;
        var y = (int)selection.Y;
        var width = (int)selection.Width;
        var height = (int)selection.Height;
        
        // Ensure bounds are valid
        x = Math.Max(0, Math.Min(x, originalImage.Width - 1));
        y = Math.Max(0, Math.Min(y, originalImage.Height - 1));
        width = Math.Min(width, originalImage.Width - x);
        height = Math.Min(height, originalImage.Height - y);
        
        if (width <= 0 || height <= 0)
            return null;
        
        var croppedImage = originalImage.Clone();
        croppedImage.Mutate(img => img.Crop(new SixLabors.ImageSharp.Rectangle(x, y, width, height)));
        
        // Clear selection after crop
        CurrentSelection = null;
        
        return croppedImage;
    }
}