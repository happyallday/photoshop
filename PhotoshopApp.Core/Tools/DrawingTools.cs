using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.Tools;

/// <summary>
/// 画笔工具
/// </summary>
public class BrushTool : ToolBase
{
    public override ToolType ToolType => ToolType.Brush;
    public override string Name => "Brush";
    public override string Description => "Draw with brush strokes";
    
    private Image<Rgba32>? _drawingLayer;
    private List<PointF> _currentStroke = new();
    
    /// <summary>
    /// 笔刷硬度 (0-100)
    /// </summary>
    public float Hardness { get; set; } = 100f;
    
    public override void OnMouseDown(Image<Rgba32> image, float x, float y, bool isLeftButton)
    {
        base.OnMouseDown(image, x, y, isLeftButton);
        
        if (isLeftButton && image != null)
        {
            // Create a temporary drawing layer if needed
            if (_drawingLayer == null || _drawingLayer.Width != image.Width || _drawingLayer.Height != image.Height)
            {
                _drawingLayer = new Image<Rgba32>(image.Width, image.Height, Color.Transparent);
            }
            
            _currentStroke.Clear();
            _currentStroke.Add(new PointF(x, y));
            
            // Draw initial point
            DrawBrushStroke(_drawingLayer, x, y);
        }
    }
    
    public override void OnMouseMove(Image<Rgba32> image, float x, float y)
    {
        base.OnMouseMove(image, x, y);
        
        if (_isMouseDown && image != null && _drawingLayer != null)
        {
            _currentStroke.Add(new PointF(x, y));
            
            // Draw continuous stroke
            if (_currentStroke.Count >= 2)
            {
                var prevPoint = _currentStroke[_currentStroke.Count - 2];
                DrawBrushStrokeLine(_drawingLayer, prevPoint.X, prevPoint.Y, x, y);
            }
        }
    }
    
    public override void OnMouseUp(Image<Rgba32> image, float x, float y)
    {
        base.OnMouseUp(image, x, y);
        
        if (_drawingLayer != null && image != null)
        {
            // Apply the drawing to the main image
            if (Options.Opacity > 0)
            {
                var finalImage = _drawingLayer.Clone();
                if (Options.Opacity < 1.0f)
                {
                    ApplyOpacity(finalImage, Options.Opacity);
                }
                
                // Composite with main image
                CompositeWithImage(image, finalImage);
            }
            
            _currentStroke.Clear();
        }
    }
    
    private void DrawBrushStroke(Image<Rgba32> canvas, float x, float y)
    {
        int size = Options.Size;
        var color = Options.Color;
        var brushColor = new Rgba32(color.R, color.G, color.B, (byte)(255 * Options.Opacity));
        
        // Apply hardness
        if (Hardness < 100f)
        {
            var softSize = size * (1f + (100f - Hardness) / 100f);
            var centerX = (int)x;
            var centerY = (int)y;
            
            canvas.Mutate(img => img.Draw(brushColor, size, 
                new SixLabors.ImageSharp.Drawing.EllipsePolygon(centerX, centerY, (int)(size / 2), (int)(size / 2))
            ));
        }
        else
        {
            // Hard brush - circular
            canvas.Mutate(img => img.Draw(brushColor, size,
                new SixLabors.ImageSharp.Drawing.EllipsePolygon((int)x, (int)y, (int)(size / 2), (int)(size / 2))
            ));
        }
    }
    
    private void DrawBrushStrokeLine(Image<Rgba32> canvas, float x1, float y1, float x2, float y2)
    {
        // Interpolate points for smooth stroke
        var distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        var steps = (int)Math.Ceiling(distance / 2); // Sample every 2 pixels
        
        for (int i = 1; i <= steps; i++)
        {
            var t = (float)i / steps;
            var x = x1 + (x2 - x1) * t;
            var y = y1 + (y2 - y1) * t;
            DrawBrushStroke(canvas, x, y);
        }
    }
    
    private void ApplyOpacity(Image<Rgba32> image, float opacity)
    {
        image.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int i = 0; i < pixelRow.Length; i++)
            {
                ref var pixel = ref pixelRow[i];
                pixel.W *= opacity;
            }
        }));
    }
    
    private void CompositeWithImage(Image<Rgba32> destination, Image<Rgba32> source)
    {
        destination.Mutate(x => x.DrawImage(source, 0, 0, 1f));
    }
    
    public override Image<Rgba32>? GetPreview(Image<Rgba32> originalImage)
    {
        if (!_isMouseDown || _drawingLayer == null)
            return null;
        
        var preview = originalImage.Clone();
        preview.Mutate(x => x.DrawImage(_drawingLayer, 0, 0, 1f));
        return preview;
    }
    
    public void ClearDrawingLayer()
    {
        if (_drawingLayer != null)
        {
            _drawingLayer.Mutate(x => x.Clear(Color.Transparent));
        }
        _currentStroke.Clear();
    }
}

/// <summary>
/// 橡皮擦工具
/// </summary>
public class EraserTool : BrushTool
{
    public override ToolType ToolType => ToolType.Eraser;
    public override string Name => "Eraser";
    public override string Description => "Erase parts of the image";
    
    public EraserTool()
    {
        Options.Color = System.Drawing.Color.Transparent;
        Options.Opacity = 1.0f;
    }
    
    public override Image<Rgba32>? GetPreview(Image<Rgba32> originalImage)
    {
        if (!_isMouseDown || _drawingLayer == null)
            return null;
        
        var preview = originalImage.Clone();
        preview.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int y = 0; y < pixelRow.Height; y++)
            {
                for (int x = 0; x < pixelRow.Width; x++)
                {
                    var sourcePixel = _drawingLayer[x, y];
                    if (sourcePixel.A > 0)
                    {
                        // Erase by making transparent
                        ref var pixel = ref pixelRow[x, y];
                        pixel.W = pixel.W * (1f - sourcePixel.W / 255f);
                    }
                }
            }
        }));
        
        return preview;
    }
}