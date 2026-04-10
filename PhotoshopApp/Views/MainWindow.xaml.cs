using System;
using System.Threading.Tasks;
using PhotoshopApp.UI.ViewModels;
using PhotoshopApp.Services;
using PhotoshopApp.Core.Layers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoshopApp;

public partial class MainWindow : System.Windows.Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        // Connect image display
        _viewModel.DisplayImage = DisplayImageOnCanvas;
        
        ImageCanvasControl.ImageDropped += ImageCanvasControl_ImageDropped;
        
        // Populate blend modes
        PopulateBlendModes();
    }

    private void PopulateBlendModes()
    {
        foreach (var mode in Enum.GetValues(typeof(BlendMode)))
        {
            BlendModeComboBox.Items.Add(mode);
        }
        
        if (BlendModeComboBox.Items.Count > 0)
            BlendModeComboBox.SelectedIndex = 0;
    }

    private void DisplayImageOnCanvas(Image<Rgba32> image)
    {
        ImageCanvasControl.DisplayImage(image);
    }

    private async void ImageCanvasControl_ImageDropped(object? sender, string filePath)
    {
        await _viewModel.OpenImageAsync(filePath);
    }
    
    private void OpacitySlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        if (_viewModel.Layers.Count > 0 && _viewModel.Layers.FirstOrDefault(l => l.IsActive) is ILayerViewModel activeLayer)
        {
            activeLayer.Opacity = e.NewValue / 100.0;
            OpacityValueText.Text = $"{(int)e.NewValue}%";
        }
    }
    
    private void BlendModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_viewModel.Layers.Count > 0 && _viewModel.Layers.FirstOrDefault(l => l.IsActive) is ILayerViewModel activeLayer)
        {
            if (BlendModeComboBox.SelectedItem is BlendMode selectedMode)
            {
                activeLayer.Layer.BlendMode = selectedMode;
            }
        }
    }
}