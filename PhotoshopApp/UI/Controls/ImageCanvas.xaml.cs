using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PhotoshopApp.UI.Controls;

using PhotoshopApp.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public partial class ImageCanvas : UserControl
{
    private WriteableBitmap? _currentBitmap;

    public ImageCanvas()
    {
        InitializeComponent();
        AllowDrop = true;
        Drop += ImageCanvas_Drop;
        DragEnter += ImageCanvas_DragEnter;
        DragLeave += ImageCanvas_DragLeave;
    }

    public void DisplayImage(Image<Rgba32> image)
    {
        _currentBitmap = (WriteableBitmap)BitmapConverter.ToBitmapSource(image);
        DisplayImage.Source = _currentBitmap;
        PlaceholderText.Visibility = Visibility.Collapsed;
    }

    public void Clear()
    {
        DisplayImage.Source = null;
        _currentBitmap = null;
        PlaceholderText.Visibility = Visibility.Visible;
    }

    private void ImageCanvas_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
    }

    private void ImageCanvas_DragLeave(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
    }

    private void ImageCanvas_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                ImageDropped?.Invoke(this, files[0]);
            }
        }
    }

    public event EventHandler<string>? ImageDropped;
}