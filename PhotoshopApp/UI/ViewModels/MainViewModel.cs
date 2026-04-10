namespace PhotoshopApp.UI.ViewModels;

using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Commands;
using PhotoshopApp.Core.History;
using PhotoshopApp.Core.ImageProcessing;
using PhotoshopApp.Core.Layers;
using PhotoshopApp.Services;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

public partial class MainViewModel : ViewModelBase
{
    private readonly ILayerManager _layerManager;
    private readonly IEditHistory _editHistory;
    private readonly IFileDialogService _fileDialogService;
    private readonly IImageProcessor _imageProcessor;
    
    [ObservableProperty]
    private string _title = "Photoshop Clone";
    
    [ObservableProperty]
    private string? _imagePath;

    [ObservableProperty]
    private string _imageInfo = "No image loaded";
    
    public Action<SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>>? DisplayImage { get; set; }

    public ObservableCollection<ILayerViewModel> Layers { get; } = new();
    
    public event EventHandler? ImageLoaded;
    
    public ICommand OpenImageCommand { get; }
    public ICommand SaveImageCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }
    public ICommand Rotate90Command { get; }
    public ICommand Rotate180Command { get; }
    public ICommand Rotate270Command { get; }
    public ICommand FlipHorizontalCommand { get; }
    public ICommand FlipVerticalCommand { get; }
    public ICommand AddLayerCommand { get; }
    public ICommand DeleteLayerCommand { get; }
    public ICommand MoveLayerUpCommand { get; }
    public ICommand MoveLayerDownCommand { get; }
    
    public ICommand SelectLayerCommand { get; private set; }

    public MainViewModel(ILayerManager layerManager, IEditHistory editHistory, 
        IFileDialogService fileDialogService, IImageProcessor imageProcessor)
    {
        _layerManager = layerManager;
        _editHistory = editHistory;
        _fileDialogService = fileDialogService;
        _imageProcessor = imageProcessor;
        
        var rotateCommand = new RotateImageCommand(layerManager, editHistory, imageProcessor);
        var flipCommand = new FlipImageCommand(layerManager, editHistory, imageProcessor);
        var layerCommandHandler = new LayerCommandHandler(layerManager, UpdateLayerViewModels);
        
        OpenImageCommand = new RelayCommand(ExecuteOpenImage);
        SaveImageCommand = new RelayCommand(ExecuteSaveImage);
        UndoCommand = new RelayCommand(ExecuteUndo, CanExecuteUndo);
        RedoCommand = new RelayCommand(ExecuteRedo, CanExecuteRedo);
        Rotate90Command = new RelayCommand(() => rotateCommand.Execute(90));
        Rotate180Command = new RelayCommand(() => rotateCommand.Execute(180));
        Rotate270Command = new RelayCommand(() => rotateCommand.Execute(270));
        FlipHorizontalCommand = new RelayCommand(() => flipCommand.Execute(FlipMode.Horizontal));
        FlipVerticalCommand = new RelayCommand(() => flipCommand.Execute(FlipMode.Vertical));
        AddLayerCommand = new RelayCommand(layerCommandHandler.AddNewLayer);
        DeleteLayerCommand = new RelayCommand(layerCommandHandler.DeleteActiveLayer, () => layerManager.ActiveLayer != null);
        MoveLayerUpCommand = new RelayCommand(() => layerCommandHandler.MoveLayerUp(layerManager.ActiveLayer!), () => layerManager.ActiveLayer != null);
        MoveLayerDownCommand = new RelayCommand(() => layerCommandHandler.MoveLayerDown(layerManager.ActiveLayer!), () => layerManager.ActiveLayer != null);
        SelectLayerCommand = new RelayCommand<ILayer>(layer => 
        {
            if (layer != null)
            {
                layerManager.ActiveLayer = layer;
                UpdateLayerViewModels();
            }
        });
        
        _editHistory.HistoryChanged += (s, e) =>
        {
            ((RelayCommand)UndoCommand).NotifyCanExecuteChanged();
            ((RelayCommand)RedoCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DeleteLayerCommand).NotifyCanExecuteChanged();
            ((RelayCommand)MoveLayerUpCommand).NotifyCanExecuteChanged();
            ((RelayCommand)MoveLayerDownCommand).NotifyCanExecuteChanged();
        };
    }

    private async void ExecuteOpenImage()
    {
        var filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|PNG Files|*.png|JPEG Files|*.jpg;*.jpeg|Bitmap Files|*.bmp|All Files|*.*";
        var filePath = _fileDialogService.OpenFileDialog(filter, "Open Image");
        
        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                await OpenImageAsync(filePath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading image: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    public async Task OpenImageAsync(string filePath)
    {
        var image = await _imageProcessor.LoadAsync(filePath);
        
        ImagePath = filePath;
        ImageInfo = $"{image.Width}x{image.Height} pixels";
        Title = $"Photoshop Clone - {System.IO.Path.GetFileName(filePath)}";
        
        var layer = new Layer
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(filePath),
            Image = image
        };
        
        _layerManager.AddLayer(layer);
        UpdateLayerViewModels();
        
        DisplayImage?.Invoke(image);
        ImageLoaded?.Invoke(this, EventArgs.Empty);
    }

    private void ExecuteSaveImage()
    {
        var filter = "PNG Files|*.png|JPEG Files|*.jpg;*.jpeg|Bitmap Files|*.bmp|All Files|*.*";
        var filePath = _fileDialogService.SaveFileDialog(filter, "Save Image");
        
        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                Task.Run(async () => await SaveImageAsync(filePath));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving image: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private async Task SaveImageAsync(string filePath)
    {
        if (_layerManager.Layers.Count > 0)
        {
            var composedImage = _layerManager.Compose(new PhotoshopApp.Core.Layers.Rect(0, 0, 1000, 1000));
            await _imageProcessor.SaveAsync(composedImage, filePath);
        }
    }

    private void ExecuteUndo()
    {
        _editHistory.Undo();
    }

    private bool CanExecuteUndo()
    {
        return _editHistory.CanUndo;
    }

    private void ExecuteRedo()
    {
        _editHistory.Redo();
    }

    private bool CanExecuteRedo()
    {
        return _editHistory.CanRedo;
    }

    private void UpdateLayerViewModels()
    {
        Layers.Clear();
        foreach (var layer in _layerManager.Layers.Reverse())
        {
            var viewModel = new LayerViewModel(layer, _layerManager, OnLayerSelected);
            Layers.Add(viewModel);
        }
        
        if (_layerManager.ActiveLayer != null)
        {
            var activeViewModel = Layers.FirstOrDefault(l => l.Layer == _layerManager.ActiveLayer);
            if (activeViewModel != null)
            {
                foreach (var vm in Layers)
                    vm.IsActive = false;
                activeViewModel.IsActive = true;
            }
        }
        
        // Redraw canvas with updated layers
        RefreshCanvas();
    }
    
    private void OnLayerSelected()
    {
        foreach (var vm in Layers)
        {
            var layerVm = vm as LayerViewModel;
            if (layerVm != null)
                layerVm.IsActive = (layerVm.Layer == _layerManager.ActiveLayer);
        }
    }
    
    private void RefreshCanvas()
    {
        if (_layerManager.Layers.Count > 0)
        {
            var composedImage = _layerManager.Compose(new PhotoshopApp.Core.Layers.Rect(0, 0, 2000, 2000));
            DisplayImage?.Invoke(composedImage);
        }
    }
}