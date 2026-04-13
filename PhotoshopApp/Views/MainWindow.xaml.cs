using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PhotoshopApp.UI.ViewModels;
using PhotoshopApp.UI.Controls;
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
    private readonly FiltersViewModel _filtersViewModel;
    
    private DrawingCanvas? _drawingCanvasControl;
    private Image<Rgba32>? _currentWorkingImage;

    public MainWindow(MainViewModel viewModel, ToolManager toolManager, ILayerManager layerManager, FiltersViewModel filtersViewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _toolManager = toolManager;
        _layerManager = layerManager;
        _filtersViewModel = filtersViewModel;
        DataContext = viewModel;
        
        // Find drawing canvas control
        _drawingCanvasControl = this.FindName("DrawingCanvasControl") as DrawingCanvas;
        if (_drawingCanvasControl != null)
        {
            // Subscribe to drawing canvas events
            _drawingCanvasControl.ImageChanged += DrawingCanvasControl_ImageChanged;
            _drawingCanvasControl.ColorPicked += DrawingCanvasControl_ColorPicked;
        }
        
        // Connect image display
        _viewModel.DisplayImage = DisplayImageOnCanvas;
        
        // Setup filters panel
        var filtersPanel = this.FindName("FiltersPanelControl") as FiltersPanel;
        if (filtersPanel != null)
        {
            var filtersDataContext = new PhotoshopApp.UI.ViewModels.FiltersViewModel(
                System.App.Current.Services.GetService<PhotoshopApp.Core.Filters.FilterManager>(),
                () => ApplyCurrentFilter());
            filtersPanel.DataContext = filtersDataContext;
            _filtersViewModel = filtersDataContext;
        }
        
        // Update filters preview when image changes
        _viewModel.ImageLoaded += (s, e) => UpdateFiltersPreview();
        
        ImageCanvasControl.ImageDropped += ImageCanvasControl_ImageDropped;
        
        // Populate blend modes
        PopulateBlendModes();
    }
    
    private void DrawingCanvasControl_ImageChanged(object? sender, SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> newImage)
    {
        // Update the current layer with the new image
        if (_layerManager.ActiveLayer != null)
        {
            _layerManager.ActiveLayer.Image = newImage;
            RefreshCanvas();
        }
    }
    
    private void DrawingCanvasControl_ColorPicked(object? sender, System.Drawing.Color color)
    {
        // Update current tool color
        _viewModel.CurrentToolColorString = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
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
            _currentWorkingImage = composedImage;
            _viewModel.DisplayImage?.Invoke(composedImage);
        }
    }
    
    private void UpdateFiltersPreview()
    {
        if (_currentWorkingImage != null && _filtersViewModel != null)
        {
            _filtersViewModel.UpdateFilterPreview(_currentWorkingImage);
        }
    }
    
    private void ApplyCurrentFilter()
    {
        if (_filtersViewModel != null && _currentWorkingImage != null)
        {
            var filteredImage = _filtersViewModel.ApplyCurrentFilter(_currentWorkingImage);
            if (filteredImage != null)
            {
                if (_layerManager?.ActiveLayer != null)
                {
                    _layerManager.ActiveLayer.Image = filteredImage;
                    RefreshCanvas();
                }
            }
        }
    }
        }
    }
}