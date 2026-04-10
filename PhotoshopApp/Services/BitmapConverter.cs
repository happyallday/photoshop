using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoshopApp.Services;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class BitmapConverter
{
    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    public static BitmapSource ToBitmapSource(Image<Rgba32> image)
    {
        var bitmap = image.CloneAs<Bgra32>();
        
        var bitmapData = bitmap.TryGetSinglePixelSpan(out var pixelSpan)
            ? pixelSpan.ToArray()
            : new byte[bitmap.Width * bitmap.Height * 4];

        var writeableBitmap = new WriteableBitmap(
            bitmap.Width,
            bitmap.Height,
            96,
            96,
            PixelFormats.Bgra32,
            null);

        writeableBitmap.WritePixels(
            new Int32Rect(0, 0, bitmap.Width, bitmap.Height),
            bitmapData,
            bitmap.Width * 4,
            0);

        writeableBitmap.Freeze();
        return writeableBitmap;
    }

    public static BitmapSource ToBitmapSource(Image<Rgba32> image, int width, int height)
    {
        var resized = image.Clone(x => x.Resize(width, height));
        return ToBitmapSource(resized);
    }
}