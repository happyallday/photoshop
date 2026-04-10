using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoshopApp.Core.Layers;

public interface ILayerManager
{
    IReadOnlyList<ILayer> Layers { get; }
    ILayer? ActiveLayer { get; set; }
    void AddLayer(ILayer layer);
    void RemoveLayer(ILayer layer);
    void MoveLayer(ILayer layer, int newIndex);
    Image<Rgba32> Compose(System.Windows.Rect region);
}