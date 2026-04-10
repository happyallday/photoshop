using System;
using System.Collections.ObjectModel;

namespace PhotoshopApp.Services;

using PhotoshopApp.Core.History;

public class EditHistory : IEditHistory
{
    private readonly ObservableCollection<IEditAction> _undoStack = new();
    private readonly ObservableCollection<IEditAction> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public event EventHandler? HistoryChanged;

    public void RecordAction(IEditAction action)
    {
        _undoStack.Add(action);
        _redoStack.Clear();
        OnHistoryChanged();
    }

    public void Undo()
    {
        if (CanUndo)
        {
            var action = _undoStack.Last();
            _undoStack.RemoveAt(_undoStack.Count - 1);
            action.Undo();
            _redoStack.Add(action);
            OnHistoryChanged();
        }
    }

    public void Redo()
    {
        if (CanRedo)
        {
            var action = _redoStack.Last();
            _redoStack.RemoveAt(_redoStack.Count - 1);
            action.Execute();
            _undoStack.Add(action);
            OnHistoryChanged();
        }
    }

    protected virtual void OnHistoryChanged()
    {
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }
}