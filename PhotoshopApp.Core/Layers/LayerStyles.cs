using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.Layers;

public interface ILayerStyle
{
    string Name { get; }
    void Apply(Image<Rgba32> image);
    ILayerStyle Clone();
}

public class DropShadowStyle : ILayerStyle
{
    public string Name => "Drop Shadow";
    
    public int OffsetX { get; set; } = 5;
    public int OffsetY { get; set; } = 5;
    public int BlurRadius { get; set; } = 10;
    public float Opacity { get; set; } = 0.5f;
    public Rgba32 ShadowColor { get; set; } = new Rgba32(0, 0, 0, 128);

    public void Apply(Image<Rgba32> image)
    {
        if (BlurRadius <= 0) return;
        
        var shadow = image.Clone();
        
        // Apply blur to shadow
        shadow.Mutate(x => x.GaussianBlur(BlurRadius));
        
        // Apply color
        shadow.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
        {
            for (int x = 0; x < row.Length; x++)
            {
                ref var pixel = ref row[x];
                var alpha = pixel.W;
                pixel.X = ShadowColor.R / 255f;
                pixel.Y = ShadowColor.G / 255f;
                pixel.Z = ShadowColor.B / 255f;
                pixel.W = alpha * Opacity * (ShadowColor.A / 255f);
            }
        }));
        
        // Composite original image with shadow
        var result = new Image<Rgba32>(image.Width + Math.Abs(OffsetX) + BlurRadius * 2, 
                                        image.Height + Math.Abs(OffsetY) + BlurRadius * 2, 
                                        Color.Transparent);
        
        var shadowX = BlurRadius + Math.Max(0, OffsetX);
        var shadowY = BlurRadius + Math.Max(0, OffsetY);
        var originalX = BlurRadius + Math.Max(0, -OffsetX);
        var originalY = BlurRadius + Math.Max(0, -OffsetY);
        
        result.Mutate(x => x.DrawImage(shadow, new SixLabors.ImageSharp.Point(shadowX, shadowY), 1f));
        result.Mutate(x => x.DrawImage(image, new SixLabors.ImageSharp.Point(originalX, originalY), 1f));
        
        // Copy back to original (may need to resize)
        if (result.Width != image.Width || result.Height != image.Height)
        {
            image.Mutate(x => x.Resize(result.Width, result.Height));
        }
        
        result.Mutate(x => x.DrawImage(image, 0, 0, 1f));
    }

    public ILayerStyle Clone()
    {
        return new DropShadowStyle
        {
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            BlurRadius = BlurRadius,
            Opacity = Opacity,
            ShadowColor = ShadowColor
        };
    }
}

public class OuterGlowStyle : ILayerStyle
{
    public string Name => "Outer Glow";
    
    public int BlurRadius { get; set; } = 10;
    public float Opacity { get; set; } = 0.5f;
    public Rgba32 GlowColor { get; set; } = new Rgba32(255, 255, 255, 128);

    public void Apply(Image<Rgba32> image)
    {
        if (BlurRadius <= 0) return;
        
        var glow = image.Clone();
        
        // Apply blur
        glow.Mutate(x => x.GaussianBlur(BlurRadius));
        
        // Apply glow color
        glow.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
        {
            for (int x = 0; x < row.Length; x++)
            {
                ref var pixel = ref row[x];
                var alpha = pixel.W;
                pixel.X = GlowColor.R / 255f;
                pixel.Y = GlowColor.G / 255f;
                pixel.Z = GlowColor.B / 255f;
                pixel.W = alpha * Opacity * (GlowColor.A / 255f);
            }
        }));
        
        // Composite
        var result = new Image<Rgba32>(image.Width + BlurRadius * 2, 
                                        image.Height + BlurRadius * 2, 
                                        Color.Transparent);
        
        result.Mutate(x => x.DrawImage(glow, new SixLabors.ImageSharp.Point(BlurRadius, BlurRadius), 1f));
        result.Mutate(x => x.DrawImage(image, new SixLabors.ImageSharp.Point(BlurRadius, BlurRadius), 1f));
        
        if (result.Width != image.Width || result.Height != image.Height)
        {
            image.Mutate(x => x.Resize(result.Width, result.Height));
        }
        
        result.Mutate(x => x.DrawImage(image, 0, 0, 1f));
    }

    public ILayerStyle Clone()
    {
        return new OuterGlowStyle
        {
            BlurRadius = BlurRadius,
            Opacity = Opacity,
            GlowColor = GlowColor
        };
    }
}

public class InnerShadowStyle : ILayerStyle
{
    public string Name => "Inner Shadow";
    
    public int OffsetX { get; set; } = 2;
    public int OffsetY { get; set; } = 2;
    public int BlurRadius { get; set; } = 5;
    public float Opacity { get; set; } = 0.5f;
    public Rgba32 ShadowColor { get; set; } = new Rgba32(0, 0, 0, 128);

    public void Apply(Image<Rgba32> image)
    {
        if (BlurRadius <= 0) return;
        
        var shadow = image.Clone();
        
        // Inner shadow effect (simplified implementation)
        shadow.Mutate(x => x.GaussianBlur(BlurRadius));
        
        // Apply dark color
        shadow.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
        {
            for (int x = 0; x < row.Length; x++)
            {
                ref var pixel = ref row[x];
                pixel.X = ShadowColor.R / 255f;
                pixel.Y = ShadowColor.G / 255f;
                pixel.Z = ShadowColor.B / 255f;
                pixel.W = pixel.W * Opacity * (ShadowColor.A / 255f);
            }
        }));
        
        // Composite using overlay blend mode
        image.Mutate(x => x.DrawImage(shadow, 0, 0, new PixelColorBlendingMode(PixelColorBlendingMode.Normal)));
    }

    public ILayerStyle Clone()
    {
        return new InnerShadowStyle
        {
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            BlurRadius = BlurRadius,
            Opacity = Opacity,
            ShadowColor = ShadowColor
        };
    }
}