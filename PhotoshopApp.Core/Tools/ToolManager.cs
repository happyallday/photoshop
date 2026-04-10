namespace PhotoshopApp.Core.Tools;

/// <summary>
/// 工具管理器
/// </summary>
public class ToolManager
{
    private readonly Dictionary<ToolType, ITool> _tools;
    private ToolType _currentTool;
    
    public ToolManager()
    {
        _tools = new Dictionary<ToolType, ITool>();
        _currentTool = ToolType.None;
        
        // 注册默认工具
        RegisterDefaultTools();
    }
    
    /// <summary>
    /// 注册默认工具
    /// </summary>
    private void RegisterDefaultTools()
    {
        RegisterTool(new SelectionTool());
        RegisterTool(new CropTool());
        RegisterTool(new MoveTool());
        RegisterTool(new TransformTool());
    }
    
    /// <summary>
    /// 注册工具
    /// </summary>
    public void RegisterTool(ITool tool)
    {
        _tools[tool.ToolType] = tool;
    }
    
    /// <summary>
    /// 获取工具
    /// </summary>
    public ITool? GetTool(ToolType toolType)
    {
        return _tools.TryGetValue(toolType, out var tool) ? tool : null;
    }
    
    /// <summary>
    /// 当前选中的工具
    /// </summary>
    public ITool? CurrentTool
    {
        get => GetTool(_currentTool);
        set
        {
            if (value != null)
            {
                _currentTool = value.ToolType;
            }
        }
    }
    
    /// <summary>
    /// 切换工具
    /// </summary>
    public bool SwitchTool(ToolType toolType)
    {
        if (_tools.ContainsKey(toolType))
        {
            _currentTool = toolType;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 获取所有工具
    /// </summary>
    public IEnumerable<ITool> GetAllTools()
    {
        return _tools.Values;
    }
    
    /// <summary>
    /// 当前工具类型
    /// </summary>
    public ToolType CurrentToolType => _currentTool;
    
    /// <summary>
    /// 是否有选中工具
    /// </summary>
    public bool HasCurrentTool => _currentTool != ToolType.None;
}