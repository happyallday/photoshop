using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Services;

using PhotoshopApp.Core.ImageProcessing;
using SixLabors.ImageSharp.PixelFormats;

public class ImageProcessor : IImageProcessor
{
    public async Task<Image<Rgba32>> LoadAsync(string filePath)
    {
        return await Task.Run(() => Image.Load<Rgba32>(filePath));
    }

    public async Task SaveAsync(Image<Rgba32> image, string filePath)
    {
        await Task.Run(() => image.Save(filePath));
    }

    public Image<Rgba32> Resize(Image<Rgba32> image, int width, int height)
    {
        var clone = image.Clone();
        clone.Mutate(x => x.Resize(width, height));
        return clone;
    }

    public Image<Rgba32> Rotate(Image<Rgba32> image, float degrees)
    {
        var clone = image.Clone();
        clone.Mutate(x => x.Rotate(degrees));
        return clone;
    }

    public Image<Rgba32> Flip(Image<Rgba32> image, FlipMode mode)
    {
        var clone = image.Clone();
        clone.Mutate(x => x.Flip(mode));
        return clone;
    }

    public Image<Rgba32> Crop(Image<Rgba32> image, int x, int y, int width, int height)
    {
        var clone = image.Clone();
        clone.Mutate(x => x.Crop(new SixLabors.ImageSharp.Rectangle(x, y, width, height)));
        return clone;
    }
}