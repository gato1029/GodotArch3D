using Godot;
using System;

[GlobalClass]
public partial class KuroDraggableControl : Control
{
    [Export] public string GroupName = "draggable_items";

    public override void _Ready()
    {
        AddToGroup(GroupName);
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        var preview = (Control)Duplicate();
        preview.Modulate = new Color(1, 1, 1, 0.6f);
        SetDragPreview(preview);

        return this;
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return data.Obj is Control;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var draggedItem = data.As<Control>();
        if (draggedItem == null) return;

        if (GetParent() is not Container container)
            return;

        int dropIndex = container.GetChildren().IndexOf(this);

        // 🔥 mejora: detectar lado
        var rect = GetRect();
        if (atPosition.X > rect.Size.X / 2)
            dropIndex++;

        container.MoveChild(draggedItem, dropIndex);
        UpdatePositions(container);
    }

    protected virtual void UpdatePositions(Node container)
    {
        for (int i = 0; i < container.GetChildCount(); i++)
        {
            if (container.GetChild(i) is KuroDraggableControl item)
                item.OnPositionChanged(i);
        }
    }

    protected virtual void OnPositionChanged(int index)
    {
        // override en hijos
    }
}
