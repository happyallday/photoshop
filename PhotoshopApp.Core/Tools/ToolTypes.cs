namespace PhotoshopApp.Core.Tools;

/// <summary>
/// 编辑工具类型枚举
/// </summary>
public enum ToolType
{
    None,
    Move,
    Selection,
    Crop,
    Brush,
    Eraser,
    Rectangle,
    Circle,
    Line,
    Text,
    Fill,
    Eyedropper,
    Zoom,
    Hand
}

/// <summary>
/// 工具选项
/// </summary>
public class ToolOptions
{
    public int Size { get; set; } = 10;
    public int Hardness { get; set; } = 100;
    public float Opacity { get; set; } = 1.0f;
    public System.Drawing.Color Color { get; set; } = System.Drawing.Color.Black;
    public string FontFamily { get; set; } = "Arial";
    public int FontSize { get; set; } = 16;
    
    /// <summary>
    /// 克隆工具选项
    /// </summary>
    public ToolOptions Clone() => new ToolOptions
    {
        Size = Size,
        Hardness = Hardness,
        Opacity = Opacity,
        Color = Color,
        FontFamily = FontFamily,
        FontSize = FontSize
    };
}

/// <summary>
/// 选择区域
/// </summary>
public struct SelectionRect
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    
    public bool IsValid => Width > 0 && Height > 0;
    
    public SelectionRect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    public bool Contains(float x, float y)
    {
        return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
    }
    
    public static SelectionRect Empty => new SelectionRect(0, 0, 0, 0);
}