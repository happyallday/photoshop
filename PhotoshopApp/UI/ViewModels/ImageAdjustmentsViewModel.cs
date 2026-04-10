using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoshopApp.UI.ViewModels;

public class ImageAdjustmentsViewModel : ViewModelBase
{
    private readonly ImageAdjuster _adjuster;
    private readonly Action _applyAction;
    
    [ObservableProperty]
    private float _brightness = 0f;
    
    [ObservableProperty]
    private float _contrast = 0f;
    
    [ObservableProperty]
    private float _saturation = 0f;
    
    [ObservableProperty]
    private float _hue = 0f;
    
    [ObservableProperty]
    private float _exposure = 0f;
    
    [ObservableProperty]
    private float _highlights = 0f;
    
    [ObservableProperty]
    private float _shadows = 0f;
    
    [ObservableProperty]
    private float _temperature = 0f;
    
    [ObservableProperty]
    private float _tint = 0f;
    
    public ICommand ApplyAdjustmentsCommand { get; }
    public ICommand ResetAdjustmentsCommand { get; }
    public ICommand PreviewAdjustmentsCommand { get; }
    
    public ImageAdjustmentsViewModel(ObservableProperty<bool> isBusyProp, Action applyAction)
    {
        _adjuster = new ImageAdjuster();
        _applyAction = applyAction;
        
        ApplyAdjustmentsCommand = new RelayCommand(ExecuteApplyAdjustments);
        ResetAdjustmentsCommand = new RelayCommand(ExecuteResetAdjustments);
        PreviewAdjustmentsCommand = new RelayCommand<Image<Rgba32>>(ExecutePreviewAdjustments);
    }
    
    private void ExecuteApplyAdjustments()
    {
        var adjustments = GetCurrentAdjustments();
        if (adjustments.HasAdjustments())
        {
            _applyAction?.Invoke();
        }
    }
    
    private void ExecuteResetAdjustments()
    {
        Brightness = 0f;
        Contrast = 0f;
        Saturation = 0f;
        Hue = 0f;
        Exposure = 0f;
        Highlights = 0f;
        Shadows = 0f;
        Temperature = 0f;
        Tint = 0f;
    }
    
    private void ExecutePreviewAdjustments(Image<Rgba32? sourceImage)
    {
        // This will trigger a preview update
        // The preview will be handled by the main view model
    }
    
    public ImageAdjuster.Adjustments GetCurrentAdjustments()
    {
        return new ImageAdjuster.Adjustments
        {
            Brightness = Brightness,
            Contrast = Contrast,
            Saturation = Saturation,
            Hue = Hue,
            Exposure = Exposure,
            Highlights = Highlights,
            Shadows = Shadows,
            Temperature = Temperature,
            Tint = Tint
        };
    }
    
    public Image<Rgba32> ApplyToImage(Image<Rgba32> sourceImage)
    {
        var adjustments = GetCurrentAdjustments();
        return _adjuster.ApplyAdjustments(sourceImage, adjustments);
    }
    
    public bool HasActiveAdjustments()
    {
        return GetCurrentAdjustments().HasAdjustments();
    }
}