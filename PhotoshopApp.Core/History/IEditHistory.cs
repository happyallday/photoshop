namespace PhotoshopApp.Core.History;

public interface IEditHistory
{
    void RecordAction(IEditAction action);
    void Undo();
    void Redo();
    bool CanUndo { get; }
    bool CanRedo { get; }
    event EventHandler? HistoryChanged;
}