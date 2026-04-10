using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PhotoshopApp.UI.ViewModels;
using PhotoshopApp.Services;
using PhotoshopApp.Core.Layers;
using PhotoshopApp.Core.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using BlendMode = PhotoshopApp.Core.Layers.BlendMode;

namespace PhotoshopApp;

public partial class MainWindow : System.Windows.Window
{
    private readonly MainViewModel _viewModel;
    private readonly ToolManager _toolManager;
    private readonly ILayerManager _layerManager;

    public MainWindow(MainViewModel viewModel, ToolManager toolManager, ILayerManager layerManager)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _toolManager = toolManager;
        _layerManager = layerManager;
        DataContext = viewModel;
        
        // Connect image display
        _viewModel.DisplayImage = DisplayImageOnCanvas;
        
        ImageCanvasControl.ImageDropped += ImageCanvasControl_ImageDropped;
        
        // Populate blend modes
        PopulateBlendModes();
    }
    
    public void ApplyCrop(int width, int height)
    {
        if (_layerManager?.ActiveLayer?.Image != null)
        {
            var originalImage = _layerManager.ActiveLayer.Image;
            var croppedImage = originalImage.Clone();
            croppedImage.Mutate(x => x.Crop(new SixLabors.ImageSharp.Rectangle(0, 0, width, height)));
            
            _layerManager.ActiveLayer.Image = croppedImage;
            RefreshCanvas();
        }
    }
    
    public void ApplyTransform(double rotation, double scale)
    {
        if (_layerManager?.ActiveLayer?.Image != null)
        {
            var originalImage = _layerManager.ActiveLayer.Image;
            var transformedImage = originalImage.Clone();
            
            // Apply rotation
            if (rotation != 0)
            {
                transformedImage.Mutate(x => x.Rotate((float)rotation));
            }
            
            // Apply scale
            if (scale != 1.0)
            {
                var newWidth = (int)(transformedImage.Width * scale);
                var newHeight = (int)(transformedImage.Height * scale);
                if (newWidth > 0 && newHeight > 0)
                {
                    transformedImage.Mutate(x => x.Resize(newWidth, newHeight));
                }
            }
            
            _layerManager.ActiveLayer.Image = transformedImage;
            RefreshCanvas();
        }
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
            if (!activeLayer.Layer.IsLocked)
            {
                activeLayer.Opacity = e.NewValue / 100.0;
                OpacityValueText.Text = $"{(int)e.NewValue}%";
            }
            else
            {
                // Revert to original opacity value
                OpacitySlider.Value = activeLayer.Opacity * 100;
                OpacityValueText.Text = $"{(int)(activeLayer.Opacity * 100)}%";
            }
        }
    }
    
private void BlendModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_viewModel.Layers.Count > 0 && _viewModel.Layers.FirstOrDefault(l => l.IsActive) is ILayerViewModel activeLayer)
        {
            if (!activeLayer.Layer.IsLocked && BlendModeComboBox.SelectedItem is BlendMode selectedMode)
            {
                activeLayer.Layer.BlendMode = selectedMode;
            }
        }
    }
    
    private void RefreshCanvas()
    {
        if (_layerManager?.Layers.Count > 0)
        {
            var composedImage = _layerManager.Compose(new PhotoshopApp.Core.Layers.Rect(0, 0, 2000, 2000));
            _viewModel.DisplayImage?.Invoke(composedImage);
        }
    }
        }
    }
}