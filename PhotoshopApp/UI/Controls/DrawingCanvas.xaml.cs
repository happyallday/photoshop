using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace PhotoshopApp.UI.Controls;

using PhotoshopApp.Services;
using PhotoshopApp.Core.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public partial class DrawingCanvas : UserControl
{
    private Image<Rgba32>? _currentImage;
    private Image<Rgba32>? _originalImage;
    private ITool? _currentTool;
    private ToolOptions _currentToolOptions = new();
    
    public event EventHandler<Image<Rgba32>>? ImageChanged;
    public event EventHandler<SelectionRect?>? SelectionChanged;
    public event EventHandler<System.Drawing.Color>? ColorPicked;
    
    public DrawingCanvas()
    {
        InitializeComponent();
        
        // Enable mouse events
        MainDrawingImage.MouseWheel += DrawingImage_MouseWheel;
    }
    
    public void SetImage(Image<Rgba32> image)
    {
        _currentImage = image;
        _originalImage = image?.Clone();
        UpdateDisplay();
    }
    
    public void SetTool(ITool tool, ToolOptions options)
    {
        _currentTool = tool;
        _currentToolOptions = options;
        UpdateStatusText($"Tool: {tool.Name}");
    }
    
    public void Clear()
    {
        _currentImage = null;
        _originalImage = null;
        MainDrawingImage.Source = null;
        PreviewImage.Source = null;
        SelectionRectangle.Visibility = Visibility.Collapsed;
    }
    
    private void UpdateDisplay()
    {
        if (_currentImage != null)
        {
            MainDrawingImage.Source = BitmapConverter.ToBitmapSource(_currentImage);
        }
    }
    
    private void UpdatePreview()
    {
        if (_currentTool != null && _originalImage != null)
        {
            var preview = _currentTool.GetPreview(_originalImage);
            if (preview != null)
            {
                PreviewImage.Source = BitmapConverter.ToBitmapSource(preview);
                PreviewImage.Visibility = Visibility.Visible;
            }
        }
    }
    
    private void UpdateSelection(SelectionRect? selection)
    {
        if (selection.HasValue && selection.Value.IsValid)
        {
            SelectionRectangle.Visibility = Visibility.Visible;
            SelectionRectangle.Margin = new Thickness(
                selection.Value.X,
                selection.Value.Y,
                0,
                0);
            SelectionRectangle.Width = selection.Value.Width;
            SelectionRectangle.Height = selection.Value.Height;
        }
        else
        {
            SelectionRectangle.Visibility = Visibility.Collapsed;
        }
    }
    
    private void UpdateStatusText(string text)
    {
        StatusText.Text = text;
    }
    
    #region Mouse Events
    
    private void DrawingImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_currentImage == null || _currentTool == null) return;
        
        var position = e.GetPosition(MainDrawingImage);
        var imagePosition = ConvertScreenToImageCoordinates(position.X, position.Y);
        
        if (imagePosition.HasValue)
        {
            _currentTool.OnMouseDown(_currentImage, imagePosition.Value.X, imagePosition.Value.Y, true);
            UpdatePreview();
            e.Handled = true;
        }
    }
    
    private void DrawingImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_currentImage == null || _currentTool == null) return;
        
        var position = e.GetPosition(MainDrawingImage);
        var imagePosition = ConvertScreenToImageCoordinates(position.X, position.Y);
        
        if (imagePosition.HasValue)
        {
            _currentTool.OnMouseUp(_currentImage, imagePosition.Value.X, imagePosition.Value.Y);
            
            // Check for selection changes
            var selection = _currentTool.GetSelection();
            UpdateSelection(selection);
            SelectionChanged?.Invoke(this, selection);
            
            // Special handling for specific tools
            HandleToolCompletion(_currentTool, _currentImage, imagePosition.Value);
            
            // Update main display
            UpdateDisplay();
            PreviewImage.Source = null;
            ImageChanged?.Invoke(this, _currentImage);
            
            e.Handled = true;
        }
    }
    
    private void DrawingImage_MouseMove(object sender, MouseEventArgs e)
    {
        if (_currentImage == null) return;
        
        var position = e.GetPosition(MainDrawingImage);
        var imagePosition = ConvertScreenToImageCoordinates(position.X, position.Y);
        
        if (imagePosition.HasValue)
        {
            PositionText.Text = $"{(int)imagePosition.Value.X}, {(int)imagePosition.Value.Y}";
            
            if (_currentTool != null && e.LeftButton == MouseButtonState.Pressed)
            {
                _currentTool.OnMouseMove(_currentImage, imagePosition.Value.X, imagePosition.Value.Y);
                UpdatePreview();
                
                // Update selection preview
                var selection = _currentTool.GetSelection();
                UpdateSelection(selection);
            }
        }
    }
    
    private void DrawingImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_currentImage == null || _currentTool == null) return;
        
        var position = e.GetPosition(MainDrawingImage);
        var imagePosition = ConvertScreenToImageCoordinates(position.X, position.Y);
        
        if (imagePosition.HasValue)
        {
            _currentTool.OnMouseDown(_currentImage, imagePosition.Value.X, imagePosition.Value.Y, false);
            e.Handled = true;
        }
    }
    
    private void DrawingImage_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_currentTool is ZoomTool zoomTool)
        {
            if (e.Delta > 0)
            {
                zoomTool.ZoomIn();
            }
            else
            {
                zoomTool.ZoomOut();
            }
            
            ZoomText.Text = $"{(int)(zoomTool.CurrentZoom * 100)}%";
            
            // Apply zoom to display
            ApplyZoomToDisplay(zoomTool.CurrentZoom);
            
            e.Handled = true;
        }
    }
    
    #endregion
    
    private void ApplyZoomToDisplay(float zoomLevel)
    {
        if (_currentImage != null)
        {
            // Update layout transform for zoom
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(zoomLevel, zoomLevel));
            CanvasGrid.LayoutTransform = transformGroup;
        }
    }
    
    private (float X, float Y)? ConvertScreenToImageCoordinates(double screenX, double screenY)
    {
        if (_currentImage == null) return null;
        
        // Get actual image position on screen
        var imagePosition = MainDrawingImage.TransformToVisual(CanvasGrid).Transform(new Point(screenX, screenY));
        
        if (imagePosition.X >= 0 && imagePosition.X < _currentImage.Width &&
            imagePosition.Y >= 0 && imagePosition.Y < _currentImage.Height)
        {
            return ((float)imagePosition.X, (float)imagePosition.Y);
        }
        
        return null;
    }
    
    private void HandleToolCompletion(ITool tool, Image<Rgba32> image, (float X, float Y) position)
    {
        // Special handling for different tool types
        switch (tool.ToolType)
        {
            case ToolType.Brush:
                if (tool is BrushTool brushTool)
                {
                    // Apply brush stroke to original image
                    var brushImage = brushTool.GetPreview(_originalImage);
                    if (brushImage != null && _originalImage != null)
                    {
                        _originalImage.Mutate(x => x.DrawImage(brushImage, 0, 0, 1f));
                        brushTool.ClearDrawingLayer();
                    }
                }
                break;
                
            case ToolType.Eraser:
                if (tool is EraserTool eraserTool)
                {
                    var eraserImage = eraserTool.GetPreview(_originalImage);
                    if (eraserImage != null && _originalImage != null)
                    {
                        _originalImage.Mutate(x => x.DrawImage(eraserImage, 0, 0, 1f));
                        eraserTool.ClearDrawingLayer();
                    }
                }
                break;
                
            case ToolType.Eyedropper:
                if (tool is EyedropperTool eyedropper)
                {
                    var color = eyedropper.PickColor(image, position.X, position.Y);
                    if (color.HasValue)
                    {
                        ColorPicked?.Invoke(this, color.Value);
                        UpdateStatusText($"Picked color: {color.Value}");
                    }
                }
                break;
                
            case ToolType.Fill:
                if (tool is FillTool fillTool)
                {
                    var filledImage = fillTool.FillArea(_originalImage, position.X, position.Y, null);
                    if (filledImage != null)
                    {
                        _originalImage = filledImage;
                    }
                }
                break;
        }
    }
}