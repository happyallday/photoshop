using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoshopApp.Core.Effects;

public abstract class ImageFilter
{
    public abstract string Name { get; }
    public abstract void Apply(Image<Rgba32> image);
    public abstract ImageFilter Clone();
}