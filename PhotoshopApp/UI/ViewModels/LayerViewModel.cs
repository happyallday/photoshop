using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.Layers;
using System.Windows.Input;

namespace PhotoshopApp.UI.ViewModels;

public partial class LayerViewModel : ObservableObject, ILayerViewModel
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

    [ObservableProperty]
    private bool _isActive;

    public ICommand SelectLayerCommand { get; }

    public LayerViewModel(ILayer layer, ILayerManager layerManager, Action? onSelected = null)
    {
        _layer = layer;
        _layerManager = layerManager;
        _onSelected = onSelected ?? (() => {});
        
        SelectLayerCommand = new RelayCommand(ExecuteSelectLayer);
    }

    private void ExecuteSelectLayer()
    {
        _layerManager.ActiveLayer = _layer;
        _onSelected?.Invoke();
    }
}