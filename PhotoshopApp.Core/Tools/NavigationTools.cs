using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoshopApp.Core.Tools;

/// <summary>
/// 缩放工具
/// </summary>
public class ZoomTool : ToolBase
{
    public override ToolType ToolType => ToolType.Zoom;
    public override string Name => "Zoom";
    public override string Description => "Zoom in/out of the image";
    
    public float CurrentZoom { get; private set; } = 1.0f;
    public const float MinZoom = 0.1f;
    public const float MaxZoom = 20.0f;
    
    public void SetZoom(float zoomLevel)
    {
        CurrentZoom = Math.Clamp(zoomLevel, MinZoom, MaxZoom);
    }
    
    public void ZoomIn()
    {
        SetZoom(CurrentZoom * 1.2f);
    }
    
    public void ZoomOut()
    {
        SetZoom(CurrentZoom / 1.2f);
    }
    
    public void ZoomToFit(Image<Rgba32>? image, float viewportWidth, float viewportHeight)
    {
        if (image == null) return;
        
        var scaleX = viewportWidth / image.Width;
        var scaleY = viewportHeight / image.Height;
        var fitZoom = Math.Min(scaleX, scaleY);
        
        SetZoom(fitZoom);
    }
    
    public override void OnMouseDown(Image<Rgba32> image, float x, float y, bool isLeftButton)
    {
        base.OnMouseDown(image, x, y, isLeftButton);
        
        if (isLeftButton)
        {
            ZoomIn();
        }
        else
        {
            ZoomOut();
        }
    }
    
    public Image<Rgba32>? ApplyZoom(Image<Rgba32> image)
    {
        if (image == null || CurrentZoom == 1.0f) return null;
        
        var newWidth = (int)(image.Width * CurrentZoom);
        var newHeight = (int)(image.Height * CurrentZoom);
        
        if (newWidth <= 0 || newHeight <= 0) return null;
        
        var zoomedImage = image.Clone();
        zoomedImage.Mutate(x => x.Resize(newWidth, newHeight));
        
        return zoomedImage;
    }
}

/// <summary>
/// 手型工具（平移）
/// </summary>
public class HandTool : ToolBase
{
    public override ToolType ToolType => ToolType.Hand;
    public override string Name => "Hand";
    public override string Description => "Pan the image";
    
    public float OffsetX { get; private set; }
    public float OffsetY { get; private set; }
    
    public override void OnMouseDown(Image<Rgba32> image, float x, float y, bool isLeftButton)
    {
        base.OnMouseDown(image, x, y, isLeftButton);
        OffsetX = 0;
        OffsetY = 0;
    }
    
    public override void OnMouseMove(Image<Rgba32> image, float x, float y)
    {
        base.OnMouseMove(image, x, y);
        
        if (_isMouseDown)
        {
            OffsetX = x - _startX;
            OffsetY = y - _startY;
        }
    }
    
    public override void OnMouseUp(Image<Rgba32> image, float x, float y)
    {
        base.OnMouseUp(image, x, y);
        
        // The final offset would be used to update view positioning
    }
    
    public void ResetOffset()
    {
        OffsetX = 0;
        OffsetY = 0;
    }
}