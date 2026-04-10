using System.Windows;
using System.Windows.Controls;

namespace PhotoshopApp.UI.Controls;

public partial class ToolOptionsPanel : UserControl
{
    public ToolOptionsPanel()
    {
        InitializeComponent();
        
        RotationSlider.ValueChanged += RotationSlider_ValueChanged;
        ScaleSlider.ValueChanged += ScaleSlider_ValueChanged;
        ToolSizeSlider.ValueChanged += ToolSizeSlider_ValueChanged;
        HardnessSlider.ValueChanged += HardnessSlider_ValueChanged;
        OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;
    }
    
    private void RotationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        RotationValueText.Text = $"{(int)e.NewValue}°";
    }
    
    private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        ScaleValueText.Text = $"{(int)e.NewValue}%";
    }
    
    private void ToolSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        ToolSizeValueText.Text = $"{(int)e.NewValue}px";
    }
    
    private void HardnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        HardnessValueText.Text = $"{(int)e.NewValue}%";
    }
    
    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        OpacityValueText.Text = $"{(int)e.NewValue}%";
    }
    
    private void ApplyCrop_Click(object sender, RoutedEventArgs e)
    {
        // Handle crop application
        var cropWidthText = CropWidthTextBox.Text;
        var cropHeightText = CropHeightTextBox.Text;
        
        if (!string.IsNullOrEmpty(cropWidthText) && !string.IsNullOrEmpty(cropHeightText))
        {
            if (int.TryParse(cropWidthText, out var width) && int.TryParse(cropHeightText, out var height))
            {
                // Apply crop with specified dimensions
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.ApplyCrop(width, height);
            }
        }
    }
    
    private void ApplyTransform_Click(object sender, RoutedEventArgs e)
    {
        var rotation = RotationSlider.Value;
        var scale = ScaleSlider.Value / 100.0;
        
        var mainWindow = Window.GetWindow(this) as MainWindow;
        mainWindow?.ApplyTransform(rotation, scale);
    }
    
    private void ResetTransform_Click(object sender, RoutedEventArgs e)
    {
        RotationSlider.Value = 0;
        ScaleSlider.Value = 100;
    }
}