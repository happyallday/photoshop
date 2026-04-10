namespace PhotoshopApp.Core.History;

public interface IEditAction
{
    string Description { get; }
    void Execute();
    void Undo();
}