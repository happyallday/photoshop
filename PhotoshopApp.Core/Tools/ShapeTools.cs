using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoshopApp.Core.Tools;

/// <summary>
/// 形状工具基类
/// </summary>
public abstract class ShapeTool : ToolBase
{
    protected bool _fillShape = false;
    protected float _lineWidth = 2f;
    
    protected void DrawShape(Image<Rgba32> canvas, PointF start, PointF current, 
        bool preview = false)
    {
        var color = Options.Color;
        var brushColor = new Rgba32(color.R, color.G, color.B, (byte)(255 * Options.Opacity));
        var pen = new Pen(brushColor, _lineWidth);
        
        if (_fillShape && !preview)
        {
            // Draw filled shape
            var fillColor = new Rgba32(color.R, color.G, color.B, (byte)(200 * Options.Opacity));
            DrawFilledShape(canvas, start, current, fillColor);
        }
        else
        {
            // Draw outline
            DrawOutlinedShape(canvas, start, current, pen);
        }
    }
    
    protected abstract void DrawOutlinedShape(Image<Rgba32> canvas, PointF start, PointF end, Pen pen);
    protected abstract void DrawFilledShape(Image<Rgba32> canvas, PointF start, PointF end, Rgba32 fillColor);
}

/// <summary>
/// 矩形工具
/// </summary>
public class RectangleTool : ShapeTool
{
    public override ToolType ToolType => ToolType.Rectangle;
    public override string Name => "Rectangle";
    public override string Description => "Draw rectangles";
    
    protected override void DrawOutlinedShape(Image<Rgba32> canvas, PointF start, PointF end, Pen pen)
    {
        var x = Math.Min(start.X, end.X);
        var y = Math.Min(start.Y, end.Y);
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);
        
        canvas.Mutate(x => x.Draw(pen, new SixLabors.ImageSharp.Drawing.RectangleF(x, y, width, height)));
    }
    
    protected override void DrawFilledShape(Image<Rgba32> canvas, PointF start, PointF end, Rgba32 fillColor)
    {
        var x = Math.Min(start.X, end.X);
        var y = Math.Min(start.Y, end.Y);
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);
        
        canvas.Mutate(x => x.Fill(fillColor, new SixLabors.ImageSharp.Drawing.RectangleF(x, y, width, height)));
    }
}

/// <summary>
/// 椭圆工具
/// </summary>
public class EllipseTool : ShapeTool
{
    private bool _isCircleMode = false;
    
    public override ToolType ToolType => ToolType.Circle;
    public override string Name => "Ellipse";
    public override string Description => "Draw ellipses and circles";
    
    public bool IsCircleMode
    {
        get => _isCircleMode;
        set => _isCircleMode = value;
    }
    
    protected override void DrawOutlinedShape(Image<Rgba32> canvas, PointF start, PointF end, Pen pen)
    {
        var x = Math.Min(start.X, end.X);
        var y = Math.Min(start.Y, end.Y);
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);
        
        if (_isCircleMode)
        {
            // Force circle mode
            var diameter = Math.Max(width, height);
            canvas.Mutate(x => x.Draw(pen, 
                new SixLabors.ImageSharp.Drawing.EllipsePolygon((int)(x + diameter/2), (int)(y + diameter/2), 
                                                                  (int)(diameter/2), (int)(diameter/2))));
        }
        else
        {
            canvas.Mutate(x => x.Draw(pen, 
                new SixLabors.ImageSharp.Drawing.EllipsePolygon((int)(x + width/2), (int)(y + height/2), 
                                                                  (int)(width/2), (int)(height/2))));
        }
    }
    
    protected override void DrawFilledShape(Image<Rgba32> canvas, PointF start, PointF end, Rgba32 fillColor)
    {
        var x = Math.Min(start.X, end.X);
        var y = Math.Min(start.Y, end.Y);
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);
        
        if (_isCircleMode)
        {
            var diameter = Math.Max(width, height);
            canvas.Mutate(x => x.Fill(fillColor,
                new SixLabors.ImageSharp.Drawing.EllipsePolygon((int)(x + diameter/2), (int)(y + diameter/2), 
                                                                  (int)(diameter/2), (int)(diameter/2))));
        }
        else
        {
            canvas.Mutate(x => x.Fill(fillColor,
                new SixLabors.ImageSharp.Drawing.EllipsePolygon((int)(x + width/2), (int)(y + height/2), 
                                                                  (int)(width/2), (int)(height/2))));
        }
    }
}

/// <summary>
/// 线条工具
/// </summary>
public class LineTool : ShapeTool
{
    public override ToolType ToolType => ToolType.Line;
    public override string Name => "Line";
    public override string Description => "Draw straight lines";
    
    private bool _isArrowMode = false;
    
    public bool IsArrowMode
    {
        get => _isArrowMode;
        set => _isArrowMode = value;
    }
    
    protected override void DrawOutlinedShape(Image<Rgba32> canvas, PointF start, PointF end, Pen pen)
    {
        canvas.Mutate(x => x.DrawLines(pen, new PointF[] { start, end }));
        
        if (_isArrowMode)
        {
            DrawArrowhead(canvas, start, end, pen);
        }
    }
    
    protected override void DrawFilledShape(Image<Rgba32> canvas, PointF start, PointF end, Rgba32 fillColor)
    {
        // Lines don't have fill mode
        DrawOutlinedShape(canvas, start, end, new Pen(fillColor, _lineWidth));
    }
    
    private void DrawArrowhead(Image<Rgba32> canvas, PointF start, PointF end, Pen pen)
    {
        var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
        var arrowLength = 15f;
        var arrowAngle = Math.PI / 6;
        
        // Calculate arrowhead points
        var x1 = end.X - arrowLength * Math.Cos(angle - arrowAngle);
        var y1 = end.Y - arrowLength * Math.Sin(angle - arrowAngle);
        var x2 = end.X - arrowLength * Math.Cos(angle + arrowAngle);
        var y2 = end.Y - arrowLength * Math.Sin(angle + arrowAngle);
        
        canvas.Mutate(x => x.DrawLines(pen, new PointF[] {
            new PointF((float)x1, (float)y1),
            end,
            new PointF((float)x2, (float)y2)
        }));
    }
}