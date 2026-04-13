namespace PhotoshopApp.Core.Filters;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// 滤镜管理器
/// </summary>
public class FilterManager
{
    private readonly Dictionary<string, ImageFilter> _filters;
    
    public FilterManager()
    {
        _filters = new Dictionary<string, ImageFilter>();
        RegisterDefaultFilters();
    }
    
    /// <summary>
    /// 注册默认滤镜
    /// </summary>
    private void RegisterDefaultFilters()
    {
        // Blur Filters
        RegisterFilter(new GaussianBlurFilter(3f));
        RegisterFilter(new BoxBlurFilter(5));
        RegisterFilter(new MotionBlurFilter(0f, 10f));
        
        // Sharpen Filters
        RegisterFilter(new SharpenFilter(5f));
        
        // Art Filters
        RegisterFilter(new PixelateFilter(10));
        RegisterFilter(new EdgeDetectionFilter(50f, false));
        RegisterFilter(new EmbossFilter(1f, 45f));
        RegisterFilter(new OilPaintingFilter(10, 5));
        
        // Color Filters
        RegisterFilter(new GrayscaleFilter(GrayscaleMode.Standard));
        RegisterFilter(new SepiaFilter(1.0f));
        RegisterFilter(new InvertFilter());
        RegisterFilter(new HueRotateFilter(0f));
        RegisterFilter(new SaturationFilter(0f));
        
        // Noise Filters
        RegisterFilter(new NoiseFilter(25, true));
        RegisterFilter(new DenoiseFilter(0.5f, 1));
        RegisterFilter(new CrystallizeFilter(10, 0.5f));
    }
    
    /// <summary>
    /// 注册滤镜
    /// </summary>
    public void RegisterFilter(ImageFilter filter)
    {
        _filters[filter.Name] = filter;
    }
    
    /// <summary>
    /// 获取滤镜
    /// </summary>
    public ImageFilter? GetFilter(string filterName)
    {
        return _filters.TryGetValue(filterName, out var filter) ? filter.Clone() : null;
    }
    
    /// <summary>
    /// 获取所有滤镜名称
    /// </summary>
    public IEnumerable<string> GetFilterNames()
    {
        return _filters.Keys.OrderBy(n => n);
    }
    
    /// <summary>
    /// 按类别获取滤镜
    /// </summary>
    public Dictionary<string, IEnumerable<string>> GetFiltersByCategory()
    {
        return new Dictionary<string, IEnumerable<string>>
        {
            ["Blur"] = _filters.Where(f => f.Value.Name.Contains("Blur") || f.Value.Name.Contains("Sharpen"))
                                  .Select(f => f.Key).OrderBy(n => n),
            ["Artistic"] = _filters.Where(f => f.Value.Name.Contains("Pixelate") || 
                                           f.Value.Name.Contains("Edge") || 
                                           f.Value.Name.Contains("Emboss") ||
                                           f.Value.Name.Contains("Oil"))
                                    .Select(f => f.Key).OrderBy(n => n),
            ["Color"] = _filters.Where(f => f.Value.Name.Contains("Grayscale") || 
                                         f.Value.Name.Contains("Sepia") || 
                                         f.Value.Name.Contains("Invert") ||
                                         f.Value.Name.Contains("Hue") ||
                                         f.Value.Name.Contains("Saturation"))
                                 .Select(f => f.Key).OrderBy(n => n),
            ["Noise"] = _filters.Where(f => f.Value.Name.Contains("Noise") || 
                                        f.Value.Name.Contains("Denoise") ||
                                        f.Value.Name.Contains("Crystallize"))
                                  .Select(f => f.Key).OrderBy(n => n)
        };
    }
    
    /// <summary>
    /// 应用滤镜到图像
    /// </summary>
    public Image<Rgba32> ApplyFilter(string filterName, Image<Rgba32> sourceImage)
    {
        var filter = GetFilter(filterName);
        if (filter != null && sourceImage != null)
        {
            var result = sourceImage.Clone();
            filter.Apply(result);
            return result;
        }
        return sourceImage.Clone();
    }
    
    /// <summary>
    /// 生成滤镜预览
    /// </summary>
    public Image<Rgba32>? GeneratePreview(string filterName, Image<Rgba32> sourceImage)
    {
        var filter = GetFilter(filterName);
        if (filter != null && sourceImage != null)
        {
            // For preview, create a smaller version for performance
            const int previewSize = 256;
            var previewImage = sourceImage.Clone();
            
            // Resize for preview if needed
            if (sourceImage.Width > previewSize || sourceImage.Height > previewSize)
            {
                previewImage.Mutate(x => x.Resize(previewSize, previewSize));
            }
            
            filter.Apply(previewImage);
            return previewImage;
        }
        return null;
    }
    
    /// <summary>
    /// 创建滤镜链（多个滤镜依次应用）
    /// </summary>
    public Image<Rgba32> ApplyFilterChain(Image<Rgba32> sourceImage, params string[] filterNames)
    {
        var result = sourceImage.Clone();
        
        foreach (var filterName in filterNames)
        {
            result = ApplyFilter(filterName, result);
        }
        
        return result;
    }
    
    /// <summary>
    /// 验证滤镜参数
    /// </summary>
    public bool ValidateFilterParameters(string filterName, Dictionary<string, object> parameters)
    {
        var filter = GetFilter(filterName);
        if (filter == null) return false;
        
        try
        {
            // Parameter validation can be implemented here
            // For now, we'll just check if the filter exists
            return true;
        }
        catch
        {
            return false;
        }
    }
}