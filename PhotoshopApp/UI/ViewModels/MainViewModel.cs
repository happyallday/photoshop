namespace PhotoshopApp.UI.ViewModels;

using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.History;
using PhotoshopApp.Core.Layers;
using PhotoshopApp.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

public partial class MainViewModel : ViewModelBase
{
    private readonly ILayerManager _layerManager;
    private readonly IEditHistory _editHistory;
    
    [ObservableProperty]
    private string _title = "Photoshop Clone";

    public ObservableCollection<ILayerViewModel> Layers { get; } = new();
    
    public ICommand OpenImageCommand { get; }
    public ICommand SaveImageCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }

    public MainViewModel(ILayerManager layerManager, IEditHistory editHistory)
    {
        _layerManager = layerManager;
        _editHistory = editHistory;
        
        OpenImageCommand = new RelayCommand(ExecuteOpenImage);
        SaveImageCommand = new RelayCommand(ExecuteSaveImage);
        UndoCommand = new RelayCommand(ExecuteUndo, CanExecuteUndo);
        RedoCommand = new RelayCommand(ExecuteRedo, CanExecuteRedo);
        
        _editHistory.HistoryChanged += (s, e) =>
        {
            ((RelayCommand)UndoCommand).NotifyCanExecuteChanged();
            ((RelayCommand)RedoCommand).NotifyCanExecuteChanged();
        };
    }

    private void ExecuteOpenImage()
    {
        // TODO: Implement file opening logic
    }

    private void ExecuteSaveImage()
    {
        // TODO: Implement file saving logic
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
}