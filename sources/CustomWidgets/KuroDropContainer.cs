using Godot;

[GlobalClass]
public partial class KuroDropContainer : Container
{
    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return data.Obj is Control;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var dragged = data.As<Control>();
        if (dragged == null) return;

        if (dragged.GetParent() != this)
            return;

        int dropIndex = GetDropIndex(atPosition);

        MoveChild(dragged, dropIndex);
        QueueSort();
    }

    private int GetDropIndex(Vector2 localMousePos)
    {
        int index = 0;

        foreach (Node node in GetChildren())
        {
            if (node is Control c)
            {
                Rect2 rect = c.GetRect();

                if (localMousePos.Y < rect.Position.Y + rect.Size.Y / 2)
                    return index;

                index++;
            }
        }

        return index;
    }
}