using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.Filters;

using PhotoshopApp.Core.Effects;

/// <summary>
/// 黑白滤镜
/// </summary>
public class GrayscaleFilter : ImageFilter
{
    public override string Name => "Grayscale";
    
    public GrayscaleMode Mode { get; set; } = GrayscaleMode.Standard;
    
    public GrayscaleFilter(GrayscaleMode mode = GrayscaleMode.Standard)
    {
        Mode = mode;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        switch (Mode)
        {
            case GrayscaleMode.Standard:
                image.Mutate(x => x.Grayscale());
                break;
                
            case GrayscaleMode.Luminance:
                image.Mutate(x => x.Luminance());
                break;
                
            case GrayscaleMode.Average:
                ApplyAverageGrayscale(image);
                break;
                
            case GrayscaleMode.Lightness:
                ApplyLightnessGrayscale(image);
                break;
        }
    }
    
    private void ApplyAverageGrayscale(Image<Rgba32> image)
    {
        image.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int i = 0; i < pixelRow.Length; i++)
            {
                ref var pixel = ref pixelRow[i];
                var gray = (pixel.X + pixel.Y + pixel.Z) / 3f;
                pixel.X = gray;
                pixel.Y = gray;
                pixel.Z = gray;
            }
        }));
    }
    
    private void ApplyLightnessGrayscale(Image<Rgba32> image)
    {
        image.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int i = 0; i < pixelRow.Length; i++)
            {
                ref var pixel = ref pixelRow[i];
                var gray = 0.299f * pixel.X + 0.587f * pixel.Y + 0.114f * pixel.Z;
                pixel.X = gray;
                pixel.Y = gray;
                pixel.Z = gray;
            }
        }));
    }
    
    public override ImageFilter Clone()
    {
        return new GrayscaleFilter(Mode);
    }
}

/// <summary>
黑白滤镜模式
/// </summary>
public enum GrayscaleMode
{
    Standard,
    Luminance,
    Average,
    Lightness
}

/// <summary>
/// 棕褐色滤镜
/// </summary>
public class SepiaFilter : ImageFilter
{
    public override string Name => "Sepia";
    
    public float Intensity { get; set; } = 1.0f;
    
    public SepiaFilter(float intensity = 1.0f)
    {
        Intensity = intensity;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        image.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int i = 0; i < pixelRow.Length; i++)
            {
                ref var pixel = ref pixelRow[i];
                var r = pixel.X;
                var g = pixel.Y;
                var b = pixel.Z;
                
                // Sepia conversion formula
                var newR = (r * 0.393f + g * 0.769f + b * 0.189f) * Intensity + r * (1 - Intensity);
                var newG = (r * 0.349f + g * 0.686f + b * 0.168f) * Intensity + g * (1 - Intensity);
                var newB = (r * 0.272f + g * 0.534f + b * 0.131f) * Intensity + b * (1 - Intensity);
                
                pixel.X = Math.Clamp(newR, 0f, 1f);
                pixel.Y = Math.Clamp(newG, 0f, 1f);
                pixel.Z = Math.Clamp(newB, 0f, 1f);
            }
        }));
    }
    
    public override ImageFilter Clone()
    {
        return new SepiaFilter(Intensity);
    }
}

/// <summary>
反转滤镜
/// </summary>
public class InvertFilter : ImageFilter
{
    public override string Name => "Invert";
    
    public override void Apply(Image<Rgba32> image)
    {
        image.Mutate(x => x.Invert());
    }
    
    public override ImageFilter Clone()
    {
        return new InvertFilter();
    }
}

/// <summary>
色相偏移滤镜
/// </summary>
public class HueRotateFilter : ImageFilter
{
    public override string Name => "Hue Rotate";
    
    public float Angle { get; set; } = 0f; // 角度，-180 到 180
    
    public HueRotateFilter(float angle = 0f)
    {
        Angle = angle;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        var radians = Angle * (float)Math.PI / 180f;
        
        image.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int i = 0; i < pixelRow.Length; i++)
            {
                ref var pixel = ref pixelRow[i];
                var (h, s, v) = RgbToHsv(pixel.X, pixel.Y, pixel.Z);
                
                // Rotate hue
                h = (h + radians / (2f * (float)Math.PI)) % 1f;
                if (h < 0) h += 1f;
                
                // HSV to RGB
                var (r, g, b) = HsvToRgb(h, s, v);
                
                pixel.X = r;
                pixel.Y = g;
                pixel.Z = b;
            }
        }));
    }
    
    private (float h, float s, float v) RgbToHsv(float r, float g, float b)
    {
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;
        
        // Hue
        float h = 0;
        if (delta > 0)
        {
            if (max == r)
                h = ((g - b) / delta) % 6f;
            else if (max == g)
                h = (b - r) / delta + 2f;
            else
                h = (r - g) / delta + 4f;
            h /= 6f;
            if (h < 0) h += 1f;
        }
        
        // Saturation
        var s = max == 0 ? 0 : delta / max;
        
        // Value
        var v = max;
        
        return (h, s, v);
    }
    
    private (float r, float g, float b) HsvToRgb(float h, float s, float v)
    {
        float r, g, b;
        
        var i = h * 6f;
        var c = v * s;
        var x = c * (1f - Math.Abs((i % 2f) - 1f));
        var m = v - c;
        
        switch ((int)i)
        {
            case 0: r = c; g = x; b = 0; break;
            case 1: r = x; g = c; b = 0; break;
            case 2: r = 0; g = c; b = x; break;
            case 3: r = 0; g = x; b = c; break;
            case 4: r = x; g = 0; b = c; break;
            case 5: r = c; g = 0; b = x; break;
            default: r = 0; g = 0; b = 0; break;
        }
        
        r = Math.Clamp(r + m, 0f, 1f);
        g = Math.Clamp(g + m, 0f, 1f);
        b = Math.Clamp(b + m, 0f, 1f);
        
        return (r, g, b);
    }
    
    public override ImageFilter Clone()
    {
        return new HueRotateFilter(Angle);
    }
}

/// <summary>
饱和度滤镜
/// </summary>
public class SaturationFilter : ImageFilter
{
    public override string Name => "Saturation";
    
    public float Amount { get; set; } = 0f; // -100 到 100
    
    public SaturationFilter(float amount = 0f)
    {
        Amount = amount;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        image.Mutate(x => x.Saturate(1f + Amount / 100f));
    }
    
    public override ImageFilter Clone()
    {
        return new SaturationFilter(Amount);
    }
}