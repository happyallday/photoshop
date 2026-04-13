using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.Filters;

using PhotoshopApp.Core.Effects;

/// <summary>
/// 添加噪点滤镜
/// </summary>
public class NoiseFilter : ImageFilter
{
    public override string Name => "Add Noise";
    
    public int Amount { get; set; } = 25; // 噪点数量 0-100
    public bool Monochrome { get; set; } = true; // 单色噪点
    
    public NoiseFilter(int amount = 25, bool monochrome = true)
    {
        Amount = amount;
        Monochrome = monochrome;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        var random = new Random();
        var noiseAmount = Amount / 100f;
        
        image.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int i = 0; i < pixelRow.Length; i++)
            {
                if (random.NextDouble() < noiseAmount * 0.1) // Only affect ~10% of pixels
                {
                    ref var pixel = ref pixelRow[i];
                    
                    if (Monochrome)
                    {
                        var noiseValue = (random.NextSingle() - 0.5f) * 2f;
                        pixel.X = Math.Clamp(pixel.X + noiseValue, 0f, 1f);
                        pixel.Y = Math.Clamp(pixel.Y + noiseValue, 0f, 1f);
                        pixel.Z = Math.Clamp(pixel.Z + noiseValue, 0f, 1f);
                    }
                    else
                    {
                        pixel.X = Math.Clamp(pixel.X + (random.NextSingle() - 0.5f), 0f, 1f);
                        pixel.Y = Math.Clamp(pixel.Y + (random.NextSingle() - 0.5f), 0f, 1f);
                        pixel.Z = Math.Clamp(pixel.Z + (random.NextSingle() - 0.5f), 0f, 1f);
                    }
                }
            }
        }));
    }
    
    public override ImageFilter Clone()
    {
        return new NoiseFilter(Amount, Monochrome);
    }
}

/// <summary>
/// 移除噪点滤镜
/// </summary>
public class DenoiseFilter : ImageFilter
{
    public override string Name => "Remove Noise";
    
    public float Strength { get; set; } = 0.5f;
    public int Radius { get; set; } = 1;
    
    public DenoiseFilter(float strength = 0.5f, int radius = 1)
    {
        Strength = strength;
        Radius = radius;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        // Apply median filter for denoising
        ApplyMedianFilter(image, Radius, Strength);
        
        // Apply slight blur for additional noise reduction
        image.Mutate(x => x.GaussianBlur(0.5f * Strength));
    }
    
    private void ApplyMedianFilter(Image<Rgba32> image, int radius, float strength)
    {
        var width = image.Width;
        var height = image.Height;
        var denoised = image.Clone();
        
        denoised.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            var samples = new List<(float r, float g, float b)>();
            
            for (int y = radius; y < height - radius; y++)
            {
                for (int x = radius; x < width - radius; x++)
                {
                    samples.Clear();
                    
                    // Sample neighboring pixels
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            var pixel = pixelRow[y + dy, x + dx];
                            samples.Add((pixel.X, pixel.Y, pixel.Z));
                        }
                    }
                    
                    // Calculate median
                    var sortedR = samples.OrderBy(s => s.r).ToList();
                    var sortedG = samples.OrderBy(s => s.g).ToList();
                    var sortedB = samples.OrderBy(s => s.b).ToList();
                    
                    var medianIndex = samples.Count / 2;
                    var rMedian = sortedR[medianIndex];
                    var gMedian = sortedG[medianIndex];
                    var bMedian = sortedB[medianIndex];
                    
                    ref var current = ref pixelRow[x, y];
                    
                    // Blend original with median based on strength
                    current.X = current.X * (1 - Strength) + rMedian * Strength;
                    current.Y = current.Y * (1 - Strength) + gMedian * Strength;
                    current.Z = current.Z * (1 - Strength) + bMedian * Strength;
                }
            }
        }));
        
        image.Mutate(x => x.DrawImage(denoised, 0, 0, 1f));
    }
    
    public override ImageFilter Clone()
    {
        return new DenoiseFilter(Strength, Radius);
    }
}

/// <summary>
晶格化滤镜
/// </summary>
public class CrystallizeFilter : ImageFilter
{
    public override string Name => "Crystallize";
    
    public int CellSize { get; set; } = 10;
    public float EdgeStrength { get; set; } = 0.5f;
    
    public CrystallizeFilter(int cellSize = 10, float edgeStrength = 0.5f)
    {
        CellSize = cellSize;
        EdgeStrength = edgeStrength;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        var width = image.Width;
        var height = image.Height;
        var crystallized = image.Clone();
        
        crystallized.Mutate(x => x.ProcessPixelRowsAsVector4(pixelRow =>
        {
            for (int y = 0; y < height; y += CellSize)
            {
                for (int x = 0; x < width; x += CellSize)
                {
                    // Find dominant color in cell
                    var colors = new List<(float r, float g, float b, int count)>();
                    
                    for (int py = 0; py < CellSize && y + py < height; py++)
                    {
                        for (int px = 0; px < CellSize && x + px < width; px++)
                        {
                            var pixel = pixelRow[y + py, x + px];
                            // Quantize color to reduce variations
                            var qr = Math.Round(pixel.X * 8f) / 8f;
                            var qg = Math.Round(pixel.Y * 8f) / 8f;
                            var qb = Math.Round(pixel.Z * 8f) / 8f;
                            
                            var existing = colors.FirstOrDefault(c => 
                                Math.Abs(c.r - qr) < 0.1f && 
                                Math.Abs(c.g - qg) < 0.1f && 
                                Math.Abs(c.b - qb) < 0.1f);
                            
                            if (existing.r > 0.1f || existing.g > 0.1f || existing.b > 0.1f)
                            {
                                colors.Add((existing.r, existing.g, existing.b, existing.count + 1));
                            }
                            else
                            {
                                colors.Add((qr, qg, qb, 1));
                            }
                        }
                    }
                    
                    if (colors.Count > 0)
                    {
                        // Get most common color
                        var dominantColor = colors.OrderByDescending(c => c.count).First();
                        
                        // Apply to entire cell
                        for (int py = 0; py < CellSize && y + py < height; py++)
                        {
                            for (int px = 0; px < CellSize && x + px < width; px++)
                            {
                                ref var pixel = ref pixelRow[y + py, x + px];
                                pixel.X = dominantColor.r;
                                pixel.Y = dominantColor.g;
                                pixel.Z = dominantColor.b;
                            }
                        }
                    }
                }
            }
        }));
        
        // Add edge enhancement
        crystallized.Mutate(x => x.DetectEdges(EdgeStrength / 50f));
        
        image.Mutate(x => x.DrawImage(crystallized, 0, 0, 1f));
    }
    
    public override ImageFilter Clone()
    {
        return new CrystallizeFilter(CellSize, EdgeStrength);
    }
}