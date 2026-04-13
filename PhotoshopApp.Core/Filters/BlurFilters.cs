using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.Filters;

using PhotoshopApp.Core.Effects;

/// <summary>
/// 模糊滤镜
/// </summary>
public class GaussianBlurFilter : ImageFilter
{
    public override string Name => "Gaussian Blur";
    
    public float Sigma { get; set; } = 3f;
    public float Radius { get; set; } = 5f;
    
    public GaussianBlurFilter(float sigma = 3f)
    {
        Sigma = sigma;
        Radius = sigma * 2f;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        image.Mutate(x => x.GaussianBlur(Sigma));
    }
    
    public override ImageFilter Clone()
    {
        return new GaussianBlurFilter(Sigma);
    }
    
    public Image<Rgba32> GetPreview(Image<Rgba32> sourceImage)
    {
        var preview = sourceImage.Clone();
        Apply(preview);
        return preview;
    }
}

/// <summary>
锐化滤镜
/// </summary>
public class SharpenFilter : ImageFilter
{
    public override string Name => "Sharpen";
    
    public float Amount { get; set; } = 5f;
    public int Radius { get; set; } = 1;
    
    public SharpenFilter(float amount = 5f)
    {
        Amount = amount;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        image.Mutate(x => x.DetectEdges().Draw(Color.FromArgb(0, 0, 0, 0), 1f));
        
        // Apply sharpening effect
        var sharpened = image.Clone();
        sharpened.Mutate(x => x.GaussianBlur(1f));
        
        // Blend original with sharpened version
        image.Mutate(x => x.DrawImage(sharpened, 0, 0, 0.5f));
    }
    
    public override ImageFilter Clone()
    {
        return new SharpenFilter(Amount);
    }
}

/// <summary>
运动模糊滤镜
/// </summary>
public class MotionBlurFilter : ImageFilter
{
    public override string Name => "Motion Blur";
    
    public float Angle { get; set; } = 0f;     // 角度，单位度
    public float Length { get; set; } = 10f;    // 模糊长度
    
    public MotionBlurFilter(float angle = 0f, float length = 10f)
    {
        Angle = angle;
        Length = length;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        ApplyMotionBlur(image, Angle, Length);
    }
    
    private void ApplyMotionBlur(Image<Rgba32> image, float angle, float length)
    {
        var radians = angle * (float)Math.PI / 180f;
        var dx = MathF.Cos(radians) * length;
        var dy = MathF.Sin(radians) * length;
        
        // Sample-based motion blur
        var blurred = image.Clone();
        var samples = (int)Math.Ceiling(length) + 1;
        
        blurred.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int y = 0; y < pixelRow.Height; y++)
            {
                for (int x = 0; x < pixelRow.Width; x++)
                {
                    var sumR = 0f;
                    var sumG = 0f;
                    var sumB = 0f;
                    var sumA = 0f;
                    
                    // Sample along motion direction
                    for (int i = 0; i < samples; i++)
                    {
                        var t = (float)i / (samples - 1);
                        var sampleX = x + dx * t;
                        var sampleY = y + dy * t;
                        
                        // Clamp to bounds
                        sampleX = Math.Clamp(sampleX, 0, pixelRow.Width - 1);
                        sampleY = Math.Clamp(sampleY, 0, pixelRow.Height - 1);
                        
                        var pixel = pixelRow[(int)sampleY, (int)sampleX];
                        sumR += pixel.X;
                        sumG += pixel.Y;
                        sumB += pixel.Z;
                        sumA += pixel.W;
                    }
                    
                    ref var result = ref pixelRow[x, y];
                    result.X = sumR / samples;
                    result.Y = sumG / samples;
                    result.Z = sumB / samples;
                    result.W = sumA / samples;
                }
            }
        }));
        
        image.Mutate(x => x.DrawImage(blurred, 0, 0, 1f));
    }
    
    public override ImageFilter Clone()
    {
        return new MotionBlurFilter(Angle, Length);
    }
}

/// <summary>
方框模糊滤镜
/// </summary>
public class BoxBlurFilter : ImageFilter
{
    public override string Name => "Box Blur";
    
    public int Radius { get; set; } = 5;
    
    public BoxBlurFilter(int radius = 5)
    {
        Radius = radius;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        // Box blur is essentially Gaussian blur with a specific kernel
        // For simplicity, we'll use multiple passes with small blur
        image.Mutate(x => x.BoxBlur(Radius, Radius));
    }
    
    public override ImageFilter Clone()
    {
        return new BoxBlurFilter(Radius);
    }
}