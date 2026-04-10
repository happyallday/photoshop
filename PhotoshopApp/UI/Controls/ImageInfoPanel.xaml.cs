using System.Windows.Controls;

namespace PhotoshopApp.UI.Controls;

public interface IImageInfoPanel
{
    void UpdateImageInfo(string filePath, int width, int height);
    void Clear();
}

public partial class ImageInfoPanel : UserControl, IImageInfoPanel
{
    public ImageInfoPanel()
    {
        InitializeComponent();
    }

    public void UpdateImageInfo(string filePath, int width, int height)
    {
        DimensionsText.Text = $"Dimensions: {width}x{height} pixels";
        FilePathText.Text = $"File: {System.IO.Path.GetFileName(filePath)}";
        
        if (System.IO.File.Exists(filePath))
        {
            var fileInfo = new System.IO.FileInfo(filePath);
            FileSizeText.Text = $"Size: {FormatFileSize(fileInfo.Length)}";
        }
        else
        {
            FileSizeText.Text = "Size: -";
        }
    }

    public void Clear()
    {
        DimensionsText.Text = "Dimensions: -";
        FilePathText.Text = "File: Not loaded";
        FileSizeText.Text = "Size: -";
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}