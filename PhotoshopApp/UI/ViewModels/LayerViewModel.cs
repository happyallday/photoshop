using System;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.Layers;
using System.Windows.Input;
using System.ComponentModel;

namespace PhotoshopApp.UI.ViewModels;

public partial class LayerViewModel : ObservableObject, ILayerViewModel, INotifyPropertyChanged
{
    private readonly ILayer _layer;
    private readonly ILayerManager _layerManager;
    private readonly Action _onSelected;

    public ILayer Layer => _layer;

    public string Name
    {
        get => _layer.Name;
        set
        {
            _layer.Name = value;
            OnPropertyChanged();
        }
    }

    public bool IsVisible
    {
        get => _layer.IsVisible;
        set
        {
            _layer.IsVisible = value;
            OnPropertyChanged();
        }
    }

    public double Opacity
    {
        get => _layer.Opacity;
        set
        {
            _layer.Opacity = value;
            OnPropertyChanged();
        }
    }
    
    public bool IsLocked
    {
        get => _layer.IsLocked;
        set
        {
            _layer.IsLocked = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private bool _isActive;
    
    [ObservableProperty]
    private BitmapSource? _thumbnail;

    public ICommand SelectLayerCommand { get; }
    public ICommand ToggleVisibilityCommand { get; }
    public ICommand ToggleLockCommand { get; }
    public ICommand DeleteLayerCommand { get; }

    public LayerViewModel(ILayer layer, ILayerManager layerManager, Action? onSelected = null)
    {
        _layer = layer;
        _layerManager = layerManager;
        _onSelected = onSelected ?? (() => {});
        
        SelectLayerCommand = new RelayCommand(ExecuteSelectLayer);
        ToggleVisibilityCommand = new RelayCommand(ExecuteToggleVisibility);
        ToggleLockCommand = new RelayCommand(ExecuteToggleLock);
        DeleteLayerCommand = new RelayCommand(ExecuteDeleteLayer, CanExecuteDeleteLayer);
        
        UpdateThumbnail();
    }
    
    public void UpdateThumbnail()
    {
        if (_layer.Image != null)
        {
            Thumbnail = BitmapConverter.ToBitmapSource(_layer.Image, 32, 32);
        }
    }

    private void ExecuteSelectLayer()
    {
        _layerManager.ActiveLayer = _layer;
        _onSelected?.Invoke();
    }
    
    private void ExecuteToggleVisibility()
    {
        IsVisible = !IsVisible;
    }
    
    private void ExecuteToggleLock()
    {
        IsLocked = !IsLocked;
    }
    
    private void ExecuteDeleteLayer()
    {
        if (!IsLocked)
        {
            _layerManager.RemoveLayer(_layer);
            _onSelected?.Invoke();
        }
    }
    
    private bool CanExecuteDeleteLayer()
    {
        return !IsLocked;
    }
}