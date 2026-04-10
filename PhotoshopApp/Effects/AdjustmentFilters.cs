using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Effects;

using PhotoshopApp.Core.Effects;
using SixLabors.ImageSharp.PixelFormats;

public class BrightnessFilter : ImageFilter
{
    public override string Name => "Brightness";
    public int Amount { get; set; } = 0;
    
    public BrightnessFilter(int amount = 0)
    {
        Amount = amount;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        image.Mutate(x => x.Brightness(Amount / 100.0));
    }
    
    public override ImageFilter Clone()
    {
        return new BrightnessFilter(Amount);
    }
}

public class ContrastFilter : ImageFilter
{
    public override string Name => "Contrast";
    public int Amount { get; set; } = 0;
    
    public ContrastFilter(int amount = 0)
    {
        Amount = amount;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        image.Mutate(x => x.Contrast(Amount / 100.0));
    }
    
    public override ImageFilter Clone()
    {
        return new ContrastFilter(Amount);
    }
}

public class SaturationFilter : ImageFilter
{
    public override string Name => "Saturation";
    public int Amount { get; set; } = 0;
    
    public SaturationFilter(int amount = 0)
    {
        Amount = amount;
    }
    
    public override void Apply(Image<Rgba32> image)
    {
        image.Mutate(x => x.Saturate(Amount / 100.0));
    }
    
    public override ImageFilter Clone()
    {
        return new SaturationFilter(Amount);
    }
}