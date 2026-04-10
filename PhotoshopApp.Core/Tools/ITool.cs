using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoshopApp.Core.Tools;

/// <summary>
/// 编辑工具接口
/// </summary>
public interface ITool
{
    ToolType ToolType { get; }
    string Name { get; }
    string Description { get; }
    ToolOptions Options { get; set; }
    
    /// <summary>
    /// 鼠标按下事件
    /// </summary>
    void OnMouseDown(Image<Rgba32> image, float x, float y, bool isLeftButton);
    
    /// <summary>
    /// 鼠标移动事件
    /// </summary>
    void OnMouseMove(Image<Rgba32> image, float x, float y);
    
    /// <summary>
    /// 鼠标释放事件
    /// </summary>
    void OnMouseUp(Image<Rgba32> image, float x, float y);
    
    /// <summary>
    /// 获取当前选择区域
    /// </summary>
    SelectionRect? GetSelection();
    
    /// <summary>
    /// 预览效果
    /// </summary>
    Image<Rgba32>? GetPreview(Image<Rgba32> originalImage);
}

/// <summary>
/// 工具基类
/// </summary>
public abstract class ToolBase : ITool
{
    public abstract ToolType ToolType { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    
    public ToolOptions Options { get; set; } = new ToolOptions();
    
    protected bool _isMouseDown;
    protected float _startX, _startY;
    protected float _currentX, _currentY;
    
    public virtual void OnMouseDown(Image<Rgba32> image, float x, float y, bool isLeftButton)
    {
        _isMouseDown = true;
        _startX = x;
        _startY = y;
        _currentX = x;
        _currentY = y;
    }
    
    public virtual void OnMouseMove(Image<Rgba32> image, float x, float y)
    {
        if (_isMouseDown)
        {
            _currentX = x;
            _currentY = y;
        }
    }
    
    public virtual void OnMouseUp(Image<Rgba32> image, float x, float y)
    {
        _isMouseDown = false;
        _currentX = x;
        _currentY = y;
    }
    
    public virtual SelectionRect? GetSelection()
    {
        return null;
    }
    
    public virtual Image<Rgba32>? GetPreview(Image<Rgba32> originalImage)
    {
        return null;
    }
}