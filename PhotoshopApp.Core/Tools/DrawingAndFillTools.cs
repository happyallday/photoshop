using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using System.IO;

namespace PhotoshopApp.Core.Tools;

/// <summary>
/// 文本工具
/// </summary>
public class TextTool : ToolBase
{
    public override ToolType ToolType => ToolType.Text;
    public override string Name => "Text";
    public override string Description => "Add text to the image";
    
    private string _text = "Sample Text";
    private string _fontFamily = "Arial";
    private int _fontSize = 24;
    
    public string Text
    {
        get => _text;
        set => _text = value ?? "Sample Text";
    }
    
    public string FontFamily
    {
        get => _fontFamily;
        set => _fontFamily = value ?? "Arial";
    }
    
    public int FontSize
    {
        get => _fontSize;
        set => _fontSize = Math.Max(1, value);
    }
    
    /// <summary>
    /// 绘制文本到图像
    /// </summary>
    public Image<Rgba32>? DrawTextOnImage(Image<Rgba32> image, float x, float y, string? text = null)
    {
        if (image == null) return null;
        
        var textToDraw = text ?? _text;
        if (string.IsNullOrEmpty(textToDraw)) return null;
        
        var result = image.Clone();
        
        try
        {
            var color = Options.Color;
            var fontColor = new Rgba32(color.R, color.G, color.B, (byte)(255 * Options.Opacity));
            
            // Create a simple font rendering (this is a basic implementation)
            // For advanced fonts, would need font file access and FontCollection
            var font = SystemFonts.CreateFont(_fontFamily, _fontSize);
            
            // Render text using text a
            var textOptions = new TextOptions(font)
            {
                Origin = new SixLabors.ImageSharp.PointF(x, y),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            
            result.Mutate(x => x.DrawText(textOptions, textToDraw, fontColor));
            
            return result;
        }
        catch (Exception)
        {
            // Fallback to simple rendering if font fails
            return DrawTextFallback(result, x, y, textToDraw);
        }
    }
    
    private Image<Rgba32> DrawTextFallback(Image<Rgba32> image, float x, float y, string text)
    {
        var result = image.Clone();
        
        // This is a very basic fallback implementation
        // In a real implementation, you'd use proper font rendering
        var color = Options.Color;
        var textColor = new Rgba32(color.R, color.G, color.B, (byte)(255 * Options.Opacity));
        
        // Draw a simple rectangle as placeholder for text
        // Real implementation would need font libraries
        result.Mutate(img => img.Draw(textColor, _fontSize,
            new SixLabors.ImageSharp.Drawing.RectangleF((int)x, (int)y, 
                _fontSize * text.Length, _fontSize * 1.2f)));
        
        return result;
    }
    
    /// <summary>
    /// 测量文本大小（近似）
    /// </summary>
    public SizeF MeasureText(string? text = null)
    {
        var textToMeasure = text ?? _text;
        var width = textToMeasure.Length * _fontSize * 0.6f;
        var height = _fontSize * 1.2f;
        return new SizeF(width, height);
    }
}

/// <summary>
/// 填充工具
/// </summary>
public class FillTool : ToolBase
{
    public override ToolType ToolType => ToolType.Fill;
    public override string Name => "Fill";
    public override string Description => "Fill areas with color";
    
    private bool _isPatternMode = false;
    private int _tolerance = 0;
    
    public bool IsPatternMode => _isPatternMode;
    public int Tolerance
    {
        get => _tolerance;
        set => _tolerance = Math.Clamp(value, 0, 100);
    }
    
    public Image<Rgba32>? FillArea(Image<Rgba32> image, float startX, float startY, 
        System.Drawing.Color? fillColor = null)
    {
        if (image == null) return null;
        
        var result = image.Clone();
        var fillColorToUse = fillColor ?? Options.Color;
        var fillPixel = new Rgba32(fillColorToUse.R, fillColorToUse.G, fillColorToUse.B, 
            (byte)(255 * Options.Opacity));
        
        int x = (int)startX;
        int y = (int)startY;
        
        // Clamp coordinates
        x = Math.Max(0, Math.Min(x, image.Width - 1));
        y = Math.Max(0, Math.Min(y, image.Height - 1));
        
        var targetColor = image[x, y];
        
        if (ColorsMatch(targetColor, fillPixel))
        {
            return null; // Nothing to fill
        }
        
        // Implement flood fill algorithm
        FloodFill(result, x, y, targetColor, fillPixel, _tolerance);
        
        return result;
    }
    
    private void FloodFill(Image<Rgba32> image, int startX, int startY, 
        Rgba32 targetColor, Rgba32 fillColor, int tolerance)
    {
        var width = image.Width;
        var height = image.Height;
        var visited = new bool[width, height];
        var stack = new Stack<(int x, int y)>();
        
        stack.Push((startX, startY));
        visited[startX, startY] = true;
        
        var toleranceFactor = tolerance / 100.0f;
        
        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            
            image[x, y] = fillColor;
            
            // Check 4-neighbors
            var neighbors = new (int dx, int dy)[]
            {
                (-1, 0), (1, 0), (0, -1), (0, 1)
            };
            
            foreach (var (dx, dy) in neighbors)
            {
                var nx = x + dx;
                var ny = y + dy;
                
                if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny])
                {
                    var pixel = image[nx, ny];
                    if (ColorsMatchWithTolerance(pixel, targetColor, toleranceFactor))
                    {
                        visited[nx, ny] = true;
                        stack.Push((nx, ny));
                    }
                }
            }
        }
    }
    
    private bool ColorsMatch(Rgba32 color1, Rgba32 color2)
    {
        return color1.R == color2.R && color1.G == color2.G && 
               color1.B == color2.B && color1.A == color2.A;
    }
    
    private bool ColorsMatchWithTolerance(Rgba32 color1, Rgba32 color2, float tolerance)
    {
        var rDiff = Math.Abs(color1.R - color2.R);
        var gDiff = Math.Abs(color1.G - color2.G);
        var bDiff = Math.Abs(color1.B - color2.B);
        var aDiff = Math.Abs(color1.A - color2.A);
        
        var maxDiff = Math.Max(rDiff, Math.Max(gDiff, Math.Max(bDiff, aDiff)));
        return maxDiff <= tolerance * 255;
    }
}

/// <summary>
/// 吸管工具
/// </summary>
public class EyedropperTool : ToolBase
{
    public override ToolType ToolType => ToolType.Eyedropper;
    public override string Name => "Eyedropper";
    public override string Description => "Pick colors from the image";
    
    public event EventHandler<System.Drawing.Color>? ColorPicked;
    
    public System.Drawing.Color? PickColor(Image<Rgba32> image, float x, float y)
    {
        if (image == null) return null;
        
        int pixelX = (int)Math.Clamp(x, 0, image.Width - 1);
        int pixelY = (int)Math.Clamp(y, 0, image.Height - 1);
        
        var pixel = image[pixelX, pixelY];
        var color = System.Drawing.Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
        
        ColorPicked?.Invoke(this, color);
        
        return color;
    }
}