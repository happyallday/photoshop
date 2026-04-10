using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Services;

using PhotoshopApp.Core.Layers;

public static class BlendModeProcessor
{
    public static Image<Rgba32> BlendLayers(Image<Rgba32> bottom, Image<Rgba32> top, BlendMode mode, double opacity)
    {
        var result = bottom.Clone();
        var width = Math.Min(result.Width, top.Width);
        var height = Math.Min(result.Height, top.Height);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var bottomPixel = result[x, y];
                var topPixel = top[x, y];
                
                var blended = ApplyBlendMode(bottomPixel, topPixel, mode, opacity);
                result[x, y] = blended;
            }
        }
        
        return result;
    }
    
    private static Rgba32 ApplyBlendMode(Rgba32 bottom, Rgba32 top, BlendMode mode, double opacity)
    {
        double blendFactor = top.A / 255.0 * opacity;
        double baseFactor = 1.0 - blendFactor;
        
        var bottomR = bottom.R;
        var bottomG = bottom.G;
        var bottomB = bottom.B;
        var bottomA = bottom.A;
        
        var topR = top.R;
        var topG = top.G;
        var topB = top.B;
        var topA = top.A;
        
        double r, g, b, a;
        
        switch (mode)
        {
            case BlendMode.Normal:
                r = topR * blendFactor + bottomR * baseFactor;
                g = topG * blendFactor + bottomG * baseFactor;
                b = topB * blendFactor + bottomB * baseFactor;
                break;
                
            case BlendMode.Multiply:
                r = MultiplyColor(bottomR, topR, blendFactor, baseFactor);
                g = MultiplyColor(bottomG, topG, blendFactor, baseFactor);
                b = MultiplyColor(bottomB, topB, blendFactor, baseFactor);
                break;
                
            case BlendMode.Screen:
                r = ScreenColor(bottomR, topR, blendFactor, baseFactor);
                g = ScreenColor(bottomG, topG, blendFactor, baseFactor);
                b = ScreenColor(bottomB, topB, blendFactor, baseFactor);
                break;
                
            case BlendMode.Overlay:
                r = OverlayColor(bottomR, topR, blendFactor, baseFactor);
                g = OverlayColor(bottomG, topG, blendFactor, baseFactor);
                b = OverlayColor(bottomB, topB, blendFactor, baseFactor);
                break;
                
            case BlendMode.Darken:
                r = Math.Min(bottomR, topR) * blendFactor + bottomR * baseFactor;
                g = Math.Min(bottomG, topG) * blendFactor + bottomG * baseFactor;
                b = Math.Min(bottomB, topB) * blendFactor + bottomB * baseFactor;
                break;
                
            case BlendMode.Lighten:
                r = Math.Max(bottomR, topR) * blendFactor + bottomR * baseFactor;
                g = Math.Max(bottomG, topG) * blendFactor + bottomG * baseFactor;
                b = Math.Max(bottomB, topB) * blendFactor + bottomB * baseFactor;
                break;
                
            case BlendMode.ColorDodge:
                r = ColorDodgeColor(bottomR, topR, blendFactor, baseFactor);
                g = ColorDodgeColor(bottomG, topG, blendFactor, baseFactor);
                b = ColorDodgeColor(bottomB, topB, blendFactor, baseFactor);
                break;
                
            case BlendMode.ColorBurn:
                r = ColorBurnColor(bottomR, topR, blendFactor, baseFactor);
                g = ColorBurnColor(bottomG, topG, blendFactor, baseFactor);
                b = ColorBurnColor(bottomB, topB, blendFactor, baseFactor);
                break;
                
            case BlendMode.HardLight:
                if (topR < 128)
                    r = MultiplyColor(bottomR, 2 * topR, blendFactor, baseFactor);
                else
                    r = ScreenColor(bottomR, 2 * (topR - 128), blendFactor, baseFactor);
                    
                if (topG < 128)
                    g = MultiplyColor(bottomG, 2 * topG, blendFactor, baseFactor);
                else
                    g = ScreenColor(bottomG, 2 * (topG - 128), blendFactor, baseFactor);
                    
                if (topB < 128)
                    b = MultiplyColor(bottomB, 2 * topB, blendFactor, baseFactor);
                else
                    b = ScreenColor(bottomB, 2 * (topB - 128), blendFactor, baseFactor);
                break;
                
            case BlendMode.SoftLight:
                r = SoftLightColor(bottomR, topR, blendFactor, baseFactor);
                g = SoftLightColor(bottomG, topG, blendFactor, baseFactor);
                b = SoftLightColor(bottomB, topB, blendFactor, baseFactor);
                break;
                
            case BlendMode.Difference:
                r = Math.Abs(bottomR - topR) * blendFactor + bottomR * baseFactor;
                g = Math.Abs(bottomG - topG) * blendFactor + bottomG * baseFactor;
                b = Math.Abs(bottomB - topB) * blendFactor + bottomB * baseFactor;
                break;
                
            case BlendMode.Exclusion:
                r = ExclusionColor(bottomR, topR, blendFactor, baseFactor);
                g = ExclusionColor(bottomG, topG, blendFactor, baseFactor);
                b = ExclusionColor(bottomB, topB, blendFactor, baseFactor);
                break;
                
            default:
                r = topR * blendFactor + bottomR * baseFactor;
                g = topG * blendFactor + bottomG * baseFactor;
                b = topB * blendFactor + bottomB * baseFactor;
                break;
        }
        
        a = Math.Min(255, topA * opacity + bottomA * baseFactor);
        
        return new Rgba32(
            (byte)Math.Clamp(r, 0, 255),
            (byte)Math.Clamp(g, 0, 255),
            (byte)Math.Clamp(b, 0, 255),
            (byte)Math.Clamp(a, 0, 255)
        );
    }
    
    private static double MultiplyColor(double bottom, double top, double blendFactor, double baseFactor)
    {
        return (bottom * top / 255.0) * blendFactor + bottom * baseFactor;
    }
    
    private static double ScreenColor(double bottom, double top, double blendFactor, double baseFactor)
    {
        return (255 - (255 - bottom) * (255 - top) / 255.0) * blendFactor + bottom * baseFactor;
    }
    
    private static double OverlayColor(double bottom, double top, double blendFactor, double baseFactor)
    {
        if (bottom < 128)
            return MultiplyColor(bottom, 2 * top, blendFactor, baseFactor);
        else
            return ScreenColor(bottom, 2 * (top - 128), blendFactor, baseFactor);
    }
    
    private static double ColorDodgeColor(double bottom, double top, double blendFactor, double baseFactor)
    {
        if (top == 255)
            return 255 * blendFactor + bottom * baseFactor;
        
        double result = bottom * 255.0 / (255.0 - top);
        return Math.Clamp(result, 0, 255) * blendFactor + bottom * baseFactor;
    }
    
    private static double ColorBurnColor(double bottom, double top, double blendFactor, double baseFactor)
    {
        if (top == 0)
            return 0 * blendFactor + bottom * baseFactor;
        
        double result = 255.0 - (255.0 - bottom) * 255.0 / top;
        return Math.Clamp(result, 0, 255) * blendFactor + bottom * baseFactor;
    }
    
    private static double SoftLightColor(double bottom, double top, double blendFactor, double baseFactor)
    {
        double result;
        if (top < 128)
        {
            result = bottom + (2 * top - 255) * (bottom - bottom * bottom / 255.0) / 255.0;
        }
        else
        {
            double d = bottom <= 127.5 ? bottom : 255.0 - bottom;
            result = bottom + (2 * top - 255) * (d - bottom * bottom / 255.0) / 255.0 + (2 * top - 255) * (Math.Sqrt(bottom * 255.0) - bottom) / 255.0;
        }
        return Math.Clamp(result, 0, 255) * blendFactor + bottom * baseFactor;
    }
    
    private static double ExclusionColor(double bottom, double top, double blendFactor, double baseFactor)
    {
        return (bottom + top - 2 * bottom * top / 255.0) * blendFactor + bottom * baseFactor;
    }
}