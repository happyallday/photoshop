using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PhotoshopApp.UI.Controls;

using PhotoshopApp.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public partial class LayerItemControl : UserControl
{
    public LayerItemControl()
    {
        InitializeComponent();
    }

    public void UpdateThumbnail(Image<Rgba32>? image)
    {
        if (image != null)
        {
            var bitmapSource = BitmapConverter.ToBitmapSource(image);
            ThumbnailImage.Source = bitmapSource;
        }
        else
        {
            ThumbnailImage.Source = null;
        }
    }
}