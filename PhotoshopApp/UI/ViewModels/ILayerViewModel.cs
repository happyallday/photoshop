namespace PhotoshopApp.UI.ViewModels;

using PhotoshopApp.Core.Layers;

public interface ILayerViewModel
{
    ILayer Layer { get; }
    string Name { get; set; }
    bool IsVisible { get; set; }
    double Opacity { get; set; }
    bool IsActive { get; set; }
    bool IsLocked { get; set; }
    System.Windows.Media.Imaging.BitmapSource? Thumbnail { get; set; }
}