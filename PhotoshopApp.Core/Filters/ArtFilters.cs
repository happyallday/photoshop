using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.Filters;

using PhotoshopApp.Core.Effects;

/// <summary>
/// 像素化滤镜
/// </summary>
public class PixelateFilter : ImageFilter
{
    public override string Name => "Pixelate";
    
    public int PixelSize { get; set; } = 10;
    
    public PixelateFilter(int pixelSize = 10)
    {
        PixelSize = pixelSize;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        if (PixelSize <= 1) return;
        
        var width = image.Width;
        var height = image.Height;
        
        image.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int y = 0; y < height; y += PixelSize)
            {
                for (int x = 0; x < width; x += PixelSize)
                {
                    // Calculate average color for pixel block
                    var sumR = 0f;
                    var sumG = 0f;
                    var sumB = 0f;
                    var sumA = 0f;
                    var count = 0;
                    
                    // Sample pixels in the block
                    for (int py = 0; py < PixelSize && y + py < height; py++)
                    {
                        for (int px = 0; px < PixelSize && x + px < width; px++)
                        {
                            var pixel = pixelRow[y + py, x + px];
                            sumR += pixel.X;
                            sumG += pixel.Y;
                            sumB += pixel.Z;
                            sumA += pixel.W;
                            count++;
                        }
                    }
                    
                    var avgR = sumR / count;
                    var avgG = sumG / count;
                    var avgB = sumB / count;
                    var avgA = sumA / count;
                    
                    // Apply average color to block
                    for (int py = 0; py < PixelSize && y + py < height; py++)
                    {
                        for (int px = 0; px < PixelSize && x + px < width; px++)
                        {
                            ref var pixel = ref pixelRow[y + py, x + px];
                            pixel.X = avgR;
                            pixel.Y = avgG;
                            pixel.Z = avgB;
                            pixel.W = avgA;
                        }
                    }
                }
            }
        }));
    }
    
    public override ImageFilter Clone()
    {
        return new PixelateFilter(PixelSize);
    }
}

/// <summary>
/// 边缘检测滤镜
/// </summary>
public class EdgeDetectionFilter : ImageFilter
{
    public override string Name => "Edge Detection";
    
    public float Sensitivity { get; set; } = 50f;
    public bool ColorEdges { get; set; } = false;
    
    public EdgeDetectionFilter(float sensitivity = 50f, bool colorEdges = false)
    {
        Sensitivity = sensitivity;
        ColorEdges = colorEdges;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        var width = image.Width;
        var height = image.Height;
        var edges = image.Clone();
        
        edges.Mutate(x => x.DetectEdges(Sensitivity / 255f));
        
        if (!ColorEdges)
        {
            // Keep edges as grayscale
            edges.Mutate(x => x.Grayscale());
        }
        
        image.Mutate(x => x.DrawImage(edges, 0, 0, 1f));
    }
    
    public override ImageFilter Clone()
    {
        return new EdgeDetectionFilter(Sensitivity, ColorEdges);
    }
}

/// <summary>
/// 描边效果滤镜
/// </summary>
public class EmbossFilter : ImageFilter
{
    public override string Name => "Emboss";
    
    public float Strength { get; set; } = 1f;
    public float Angle { get; set; } = 45f;
    
    public EmbossFilter(float strength = 1f, float angle = 45f)
    {
        Strength = strength;
        Angle = angle;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        var width = image.Width;
        var height = image.Height;
        var embossed = image.Clone();
        
        var radians = Angle * (float)Math.PI / 180f;
        var offsetX = (float)Math.Round(Math.Cos(radians));
        var offsetY = (float)Math.Round(Math.Sin(radians));
        
        embossed.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    var current = pixelRow[y, x];
                    
                    // Sample neighbors based on angle
                    var nx = (int)(x + offsetX);
                    var ny = (int)(y + offsetY);
                    
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        var neighbor = pixelRow[ny, nx];
                        
                        // Calculate intensity difference
                        var intensity1 = (current.X + current.Y + current.Z) / 3f;
                        var intensity2 = (neighbor.X + neighbor.Y + neighbor.Z) / 3f;
                        var difference = (intensity1 - intensity2) * Strength;
                        
                        // Apply emboss (add difference and make grayscale)
                        var grayValue = (difference + 0.5f);
                        ref var pixel = ref pixelRow[x, y];
                        pixel.X = grayValue;
                        pixel.Y = grayValue;
                        pixel.Z = grayValue;
                        pixel.W = current.W;
                    }
                }
            }
        }));
        
        image.Mutate(x => x.DrawImage(embossed, 0, 0, 1f));
    }
    
    public override ImageFilter Clone()
    {
        return new EmbossFilter(Strength, Angle);
    }
}

/// <summary>
油画效果滤镜
/// </summary>
public class OilPaintingFilter : ImageFilter
{
    public override string Name => "Oil Painting";
    
    public int BrushSize { get; set; } = 10;
    public int Roughness { get; set; } = 5;
    
    public OilPaintingFilter(int brushSize = 10, int roughness = 5)
    {
        BrushSize = brushSize;
        Roughness = roughness;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        var oilPainted = image.Clone();
        
        // Simplify colors and create painterly effect
        oilPainted.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            var random = new Random();
            
            for (int y = 0; y < pixelRow.Height; y += BrushSize)
            {
                for (int x = 0; x < pixelRow.Width; x += BrushSize)
                {
                    // Sample colors in brush area
                    var colors = new List<(float r, float g, float b)>();
                    
                    for (int py = 0; py < BrushSize && y + py < pixelRow.Height; py++)
                    {
                        for (int px = 0; px < BrushSize && x + px < pixelRow.Width; px++)
                        {
                            var pixel = pixelRow[y + py, x + px];
                            colors.Add((pixel.X, pixel.Y, pixel.Z));
                        }
                    }
                    
                    if (colors.Count > 0)
                    {
                        // Calculate dominant color
                        var avgR = colors.Average(c => c.r);
                        var avgG = colors.Average(c => c.g);
                        var avgB = colors.Average(c => c.b);
                        
                        // Add some color variation for painterly effect
                        var variation = Roughness / 100f;
                        var r = avgR + (random.NextDouble() - 0.5) * variation;
                        var g = avgG + (random.NextDouble() - 0.5) * variation;
                        var b = avgB + (random.NextDouble() - 0.5) * variation;
                        
                        var clampedR = Math.Clamp(r, 0f, 1f);
                        var clampedG = Math.Clamp(g, 0f, 1f);
                        var clampedB = Math.Clamp(b, 0f, 1f);
                        
                        // Apply color to entire brush area
                        for (int py = 0; py < BrushSize && y + py < pixelRow.Height; py++)
                        {
                            for (int px = 0; px < BrushSize && x + px < pixelRow.Width; px++)
                            {
                                ref var pixel = ref pixelRow[y + py, x + px];
                                pixel.X = clampedR;
                                pixel.Y = clampedG;
                                pixel.Z = clampedB;
                            }
                        }
                    }
                }
            }
        }));
        
        // Apply slight blur for artistic effect
        oilPainted.Mutate(x => x.GaussianBlur(0.5f));
        
        image.Mutate(x => x.DrawImage(oilPainted, 0, 0, 1f));
    }
    
    public override ImageFilter Clone()
    {
        return new OilPaintingFilter(BrushSize, Roughness);
    }
}