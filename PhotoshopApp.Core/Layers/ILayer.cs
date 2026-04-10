using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoshopApp.Core.Layers;

public interface ILayer
{
    string Name { get; set; }
    bool IsVisible { get; set; }
    double Opacity { get; set; }
    BlendMode BlendMode { get; set; }
    Image<Rgba32>? Image { get; set; }
    System.Windows.Rect Bounds { get; }
    ILayer Clone();
}