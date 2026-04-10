namespace PhotoshopApp.UI.ViewModels;

using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class ToolsViewModel : ViewModelBase
{
    private readonly ToolManager _toolManager;
    private readonly Action _updateCanvasAction;
    
    [ObservableProperty]
    private ITool? _currentTool;
    
    [ObservableProperty]
    private bool _isToolActive;
    
    public ICommand SelectToolCommand { get; }
    public ICommand ApplyToolCommand { get; }
    
    public ToolsViewModel(ToolManager toolManager, Action updateCanvasAction)
    {
        _toolManager = toolManager;
        _updateCanvasAction = updateCanvasAction;
        
        SelectToolCommand = new RelayCommand<ToolType>(ExecuteSelectTool);
        ApplyToolCommand = new RelayCommand(ExecuteApplyTool, CanExecuteApplyTool);
    }
    
    private void ExecuteSelectTool(ToolType? toolType)
    {
        if (toolType.HasValue)
        {
            _toolManager.SwitchTool(toolType.Value);
            CurrentTool = _toolManager.CurrentTool;
            IsToolActive = _toolManager.HasCurrentTool;
        }
    }
    
    private void ExecuteApplyTool()
    {
        if (CurrentTool != null)
        {
            // Apply current tool
            // This will be called based on tool-specific actions
        }
    }
    
    private bool CanExecuteApplyTool()
    {
        return CurrentTool != null;
    }
    
    /// <summary>
    /// 处理工具鼠标事件
    /// </summary>
    public void HandleMouseDown(Image<Rgba32> image, float x, float y, bool isLeftButton)
    {
        CurrentTool?.OnMouseDown(image, x, y, isLeftButton);
        _updateCanvasAction?.Invoke();
    }
    
    public void HandleMouseMove(Image<Rgba32> image, float x, float y)
    {
        if (CurrentTool != null)
        {
            CurrentTool.OnMouseMove(image, x, y);
            if (_isToolActive)
            {
                _updateCanvasAction?.Invoke();
            }
        }
    }
    
    public void HandleMouseUp(Image<Rgba32> image, float x, float y)
    {
        CurrentTool?.OnMouseUp(image, x, y);
        _updateCanvasAction?.Invoke();
    }
    
    /// <summary>
    /// 获取工具预览
    /// </summary>
    public Image<Rgba32>? GetToolPreview(Image<Rgba32> originalImage)
    {
        return CurrentTool?.GetPreview(originalImage);
    }
    
    /// <summary>
    /// 获取当前选择
    /// </summary>
    public SelectionRect? GetCurrentSelection()
    {
        return CurrentTool?.GetSelection();
    }
}