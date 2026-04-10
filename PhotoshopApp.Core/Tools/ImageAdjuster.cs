using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.Tools;

using PhotoshopApp.Effects;

/// <summary>
/// 图像调整器
/// </summary>
public class ImageAdjuster
{
    /// <summary>
    /// 调整参数
    /// </summary>
    public class Adjustments
    {
        public float Brightness { get; set; } = 0f;          // -100 to 100
        public float Contrast { get; set; } = 0f;            // -100 to 100
        public float Saturation { get; set; } = 0f;          // -100 to 100
        public float Hue { get; set; } = 0f;                  // -180 to 180
        public float Exposure { get; set; } = 0f;            // -100 to 100
        public float Highlights { get; set; } = 0f;          // -100 to 100
        public float Shadows { get; set; } = 0f;             // -100 to 100
        public float Temperature { get; set; } = 0f;          // -100 to 100
        public float Tint { get; set; } = 0f;                // -100 to 100
        
        /// <summary>
        /// 重置所有参数
        /// </summary>
        public void Reset()
        {
            Brightness = 0;
            Contrast = 0;
            Saturation = 0;
            Hue = 0;
            Exposure = 0;
            Highlights = 0;
            Shadows = 0;
            Temperature = 0;
            Tint = 0;
        }
        
        /// <summary>
        /// 检查是否有任何调整
        /// </summary>
        public bool HasAdjustments()
        {
            return Brightness != 0 || Contrast != 0 || Saturation != 0 || 
                   Hue != 0 || Exposure != 0 || Highlights != 0 || 
                   Shadows != 0 || Temperature != 0 || Tint != 0;
        }
    }
    
    /// <summary>
    /// 添加图层（非破坏性编辑）
    /// </summary>
    public ILayer CreateAdjustmentLayer(string layerName, Adjustments adjustments)
    {
        var layer = new Layer
        {
            Name = layerName,
            IsVisible = true,
            Opacity = 1.0,
            BlendMode = BlendMode.Normal,
            // Image will be set when applied
        };
        
        return layer;
    }
    
    /// <summary>
    /// 应用所有调整到图像
    /// </summary>
    public Image<Rgba32> ApplyAdjustments(Image<Rgba32> sourceImage, Adjustments adjustments)
    {
        var result = sourceImage.Clone();
        
        if (adjustments.Brightness != 0)
        {
            result.Mutate(x => x.Brightness(1f + adjustments.Brightness / 100f));
        }
        
        if (adjustments.Contrast != 0)
        {
            result.Mutate(x => x.Contrast(1f + adjustments.Contrast / 100f));
        }
        
        if (adjustments.Saturation != 0)
        {
            result.Mutate(x => x.Saturate(1f + adjustments.Saturation / 100f));
        }
        
        if (adjustments.Hue != 0)
        {
            ApplyHueRotation(result, adjustments.Hue);
        }
        
        if (adjustments.Exposure != 0)
        {
            ApplyExposure(result, adjustments.Exposure);
        }
        
        if (adjustments.Highlights != 0 || adjustments.Shadows != 0)
        {
            ApplyHighlightsShadows(result, adjustments.Highlights, adjustments.Shadows);
        }
        
        if (adjustments.Temperature != 0 || adjustments.Tint != 0)
        {
            ApplyTemperatureTint(result, adjustments.Temperature, adjustments.Tint);
        }
        
        return result;
    }
    
    private void ApplyHueRotation(Image<Rgba32> image, float degrees)
    {
        var radians = degrees * (float)Math.PI / 180f;
        
        image.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int x = 0; x < pixelRow.Length; x++)
            {
                ref var pixel = ref pixelRow[x];
                var r = pixel.X;
                var g = pixel.Y;
                var b = pixel.Z;
                
                // RGB to HSV conversion
                var (h, s, v) = RgbToHsv(r, g, b);
                
                // Rotate hue
                h = (h + radians / (2 * (float)Math.PI)) % 1f;
                if (h < 0) h += 1f;
                
                // HSV to RGB
                var (newR, newG, newB) = HsvToRgb(h, s, v);
                
                pixel.X = newR;
                pixel.Y = newG;
                pixel.Z = newB;
            }
        }));
    }
    
    private void ApplyExposure(Image<Rgba32> image, float exposure)
    {
        var factor = Math.Pow(2f, exposure / 100f);
        
        image.Mutate(x => x.Brightness(factor));
    }
    
    private void ApplyHighlightsShadows(Image<Rgba32> image, float highlights, float shadows)
    {
        image.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int i = 0; i < pixelRow.Length; i++)
            {
                ref var pixel = ref pixelRow[i];
                var luminance = 0.299f * pixel.X + 0.587f * pixel.Y + 0.114f * pixel.Z;
                
                if (luminance > 0.5f)
                {
                    // Highlights adjustment
                    var adjustment = (luminance - 0.5f) * 2f; // 0 to 1
                    var factor = 1f + (highlights / 100f) * adjustment;
                    pixel.X = Math.Clamp(pixel.X * factor, 0f, 1f);
                    pixel.Y = Math.Clamp(pixel.Y * factor, 0f, 1f);
                    pixel.Z = Math.Clamp(pixel.Z * factor, 0f, 1f);
                }
                else
                {
                    // Shadows adjustment
                    var adjustment = (0.5f - luminance) * 2f; // 0 to 1
                    var factor = 1f + (shadows / 100f) * adjustment;
                    pixel.X = Math.Clamp(pixel.X * factor, 0f, 1f);
                    pixel.Y = Math.Clamp(pixel.Y * factor, 0f, 1f);
                    pixel.Z = Math.Clamp(pixel.Z * factor, 0f, 1f);
                }
            }
        }));
    }
    
    private void ApplyTemperatureTint(Image<Rgba32> image, float temperature, float tint)
    {
        var tempR = temperature / 100f;
        var tempB = -temperature / 100f;
        var tintG = tint / 100f;
        var tintM = -tint / 100f;
        
        image.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int i = 0; i < pixelRow.Length; i++)
            {
                ref var pixel = ref pixelRow[i];
                
                pixel.X = Math.Clamp(pixel.X + tempR, 0f, 1f);
                pixel.Z = Math.Clamp(pixel.Z + tempB, 0f, 1f);
                
                if (tint > 0)
                {
                    pixel.Y = Math.Clamp(pixel.Y + tintG * (1f - pixel.Y), 0f, 1f);
                }
                else
                {
                    var magentaAmount = Math.Abs(tintG);
                    pixel.Y = Math.Clamp(pixel.Y - magentaAmount * pixel.Y, 0f, 1f);
                }
            }
        }));
    }
    
    private (float h, float s, float v) RgbToHsv(float r, float g, float b)
    {
        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float delta = max - min;
        
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
        float s = max == 0 ? 0 : delta / max;
        
        // Value
        float v = max;
        
        return (h, s, v);
    }
    
    private (float r, float g, float b) HsvToRgb(float h, float s, float v)
    {
        float r, g, b;
        
        float i = h * 6f;
        float c = v * s;
        float x = c * (1f - Math.Abs((i % 2f) - 1f));
        float m = v - c;
        
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
    
    /// <summary>
    /// 创建调整预览
    /// </summary>
    public Image<Rgba32> CreatePreviewImage(Image<Rgba32> sourceImage, Adjustments adjustments)
    {
        if (!adjustments.HasAdjustments())
            return sourceImage.Clone();
        
        return ApplyAdjustments(sourceImage, adjustments);
    }
}