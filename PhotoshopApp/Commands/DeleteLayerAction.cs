using CommunityToolkit.Mvvm.Input;
using PhotoshopApp.Core.History;

namespace PhotoshopApp.Commands;

public class DeleteLayerAction : IEditAction
{
    private readonly ILayer _layer;
    private readonly Action<ILayer> _addLayerAction;
    private readonly Action<ILayer> _removeLayerAction;

    public string Description => $"Delete layer: {_layer.Name}";

    public DeleteLayerAction(ILayer layer, Action<ILayer> addLayerAction, Action<ILayer> removeLayerAction)
    {
        _layer = layer;
        _addLayerAction = addLayerAction;
        _removeLayerAction = removeLayerAction;
    }

    public void Execute()
    {
        _removeLayerAction?.Invoke(_layer);
    }

    public void Undo()
    {
        _addLayerAction?.Invoke(_layer);
    }
}