using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.ImageProcessing;

public interface IImageProcessor
{
    Task<Image<Rgba32>> LoadAsync(string filePath);
    Task SaveAsync(Image<Rgba32> image, string filePath);
    Image<Rgba32> Resize(Image<Rgba32> image, int width, int height);
    Image<Rgba32> Rotate(Image<Rgba32> image, float degrees);
    Image<Rgba32> Flip(Image<Rgba32> image, FlipMode mode);
    Image<Rgba32> Crop(Image<Rgba32> image, int x, int y, int width, int height);
}