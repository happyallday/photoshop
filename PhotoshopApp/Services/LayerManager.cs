using System.Collections.Generic;
using System.Linq;
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
    public bool IsLocked { get; set; } = false;
    public Image<Rgba32>? Image { get; set; }
    public Rect Bounds => Image != null 
        ? new Rect(0, 0, Image.Width, Image.Height) 
        : Rect.Empty;
    
    private Image<Rgba32>? _thumbnail;

    public Image<Rgba32>? Thumbnail
    {
        get
        {
            if (_thumbnail == null && Image != null)
            {
                GenerateThumbnail();
            }
            return _thumbnail;
        }
    }

    private void GenerateThumbnail()
    {
        if (Image == null) return;
        
        const int thumbSize = 128;
        int width, height;
        
        if (Image.Width > Image.Height)
        {
            width = thumbSize;
            height = (int)(Image.Height * (double)thumbSize / Image.Width);
        }
        else
        {
            height = thumbSize;
            width = (int)(Image.Width * (double)thumbSize / Image.Height);
        }
        
        _thumbnail = Image.Clone(x => x.Resize(width, height));
    }

    public ILayer Clone()
    {
        return new Layer
        {
            Name = Name,
            IsVisible = IsVisible,
            Opacity = Opacity,
            BlendMode = BlendMode,
            IsLocked = IsLocked,
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
        
        // Find the maximum bounds needed
        int maxWidth = 0, maxHeight = 0;
        foreach (var layer in _layers.Where(l => l.IsVisible && l.Image != null))
        {
            maxWidth = Math.Max(maxWidth, layer.Image.Width);
            maxHeight = Math.Max(maxHeight, layer.Image.Height);
        }
        
        maxWidth = Math.Max(maxWidth, width);
        maxHeight = Math.Max(maxHeight, height);
        
        var canvas = new Image<Rgba32>(maxWidth, maxHeight, Color.Transparent);

        // Process layers from bottom to top
        foreach (var layer in _layers.Where(l => l.IsVisible && l.Image != null))
        {
            var layerImage = layer.Image.Clone();
            
            // Create a temporary canvas with the same size
            var tempCanvas = new Image<Rgba32>(maxWidth, maxHeight, Color.Transparent);
            
            // Draw the layer image to temp canvas with proper positioning
            tempCanvas.Mutate(x => x.DrawImage(layerImage, 0, 0, 1f));
            
            // Apply blend mode with current canvas
            canvas = BlendModeProcessor.BlendLayers(canvas, tempCanvas, layer.BlendMode, layer.Opacity);
        }

        // Crop to the requested region
        if (width < maxWidth || height < maxHeight)
        {
            return CropImage(canvas, (int)region.X, (int)region.Y, width, height);
        }
        
        return canvas;
    }
    
    private Image<Rgba32> CropImage(Image<Rgba32> source, int x, int y, int width, int height)
    {
        var clone = source.Clone();
        clone.Mutate(img => img.Crop(new SixLabors.ImageSharp.Rectangle(x, y, width, height)));
        return clone;
    }
}