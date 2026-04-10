using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoshopApp.Services;

using PhotoshopApp.Core.Layers;
using SixLabors.ImageSharp.Processing;

public class Layer : ILayer
{
    public string Name { get; set; } = "New Layer";
    public bool IsVisible { get; set; } = true;
    public double Opacity { get; set; } = 1.0;
    public BlendMode BlendMode { get; set; } = BlendMode.Normal;
    public Image<Rgba32>? Image { get; set; }
    public Rect Bounds => Image != null 
        ? new Rect(0, 0, Image.Width, Image.Height) 
        : Rect.Empty;

    public ILayer Clone()
    {
        return new Layer
        {
            Name = Name,
            IsVisible = IsVisible,
            Opacity = Opacity,
            BlendMode = BlendMode,
            Image = Image?.Clone()
        };
    }
}

public class LayerManager : ILayerManager
{
    private readonly List<ILayer> _layers = new();

    public IReadOnlyList<ILayer> Layers => _layers;

    public ILayer? ActiveLayer { get; set; }

    public void AddLayer(ILayer layer)
    {
        _layers.Add(layer);
        if (ActiveLayer == null)
            ActiveLayer = layer;
    }

    public void RemoveLayer(ILayer layer)
    {
        _layers.Remove(layer);
        if (ActiveLayer == layer)
            ActiveLayer = _layers.LastOrDefault();
    }

    public void MoveLayer(ILayer layer, int newIndex)
    {
        var currentIndex = _layers.IndexOf(layer);
        if (currentIndex >= 0)
        {
            _layers.RemoveAt(currentIndex);
            _layers.Insert(newIndex, layer);
        }
    }

    public Image<Rgba32> Compose(Rect region)
    {
        var width = (int)region.Width;
        var height = (int)region.Height;
        
        if (width <= 0 || height <= 0)
            return new Image<Rgba32>(100, 100);

        var canvas = new Image<Rgba32>(width, height, Color.Transparent);

        foreach (var layer in _layers.Where(l => l.IsVisible && l.Image != null))
        {
            var layerImage = layer.Image.Clone();
            
            if (layer.Opacity < 1.0)
            {
                layerImage.Mutate(x => x.Alpha((float)(layer.Opacity * 255)));
            }

            var position = new SixLabors.ImageSharp.Point(
                (int)region.X, 
                (int)region.Y
            );

            canvas.Mutate(x => x.DrawImage(layerImage, position, 1f));
        }

        return canvas;
    }
}