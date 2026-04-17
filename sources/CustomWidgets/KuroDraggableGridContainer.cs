using Godot;
using System;

[Tool]
[GlobalClass]
public partial class KuroDraggableGridContainer : Container
{
    [Export] public int Columns = 4;
    [Export] public Vector2 Separation = new Vector2(5, 5);

    private Control _dragged;
    private int _targetIndex = -1;
    private Vector2 _dragOffset;

    // =========================================================
    // READY
    // =========================================================

    public override void _Ready()
    {
        SetProcessUnhandledInput(true);

        ChildEnteredTree += OnChildEnteredTree;

        UpdateChildrenMouseFilter();
    }
    private void OnChildEnteredTree(Node node)
    {
        if (node is Control c)
            c.MouseFilter = MouseFilterEnum.Ignore;
    }
    private void UpdateChildrenMouseFilter()
    {
        foreach (Node node in GetChildren())
        {
            if (node is Control c)
                c.MouseFilter = MouseFilterEnum.Ignore;
        }
    }

    // =========================================================
    // LAYOUT
    // =========================================================

    public override void _Notification(int what)
    {
        if (what == NotificationSortChildren)
            LayoutChildren();


    }

    private void LayoutChildren()
    {
        int index = 0;

        foreach (Node node in GetChildren())
        {
            if (node is Control c && c != _dragged)
            {
                Vector2 pos = GetPositionFromIndex(index);
                Vector2 size = GetCellSize();

                FitChildInRect(c, new Rect2(pos, size));
                index++;
            }
        }
    }

    private Vector2 GetPositionFromIndex(int index)
    {
        int col = index % Columns;
        int row = index / Columns;

        Vector2 cellSize = GetCellSize();

        return new Vector2(
            col * (cellSize.X + Separation.X),
            row * (cellSize.Y + Separation.Y)
        );
    }

    private Vector2 GetCellSize()
    {
        Vector2 max = Vector2.Zero;

        foreach (Node node in GetChildren())
        {
            if (node is Control c)
            {
                var size = c.GetCombinedMinimumSize();
                max.X = Mathf.Max(max.X, size.X);
                max.Y = Mathf.Max(max.Y, size.Y);
            }
        }

        return max;
    }

    // =========================================================
    // INPUT (UNHANDLED)
    // =========================================================

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
                StartDrag();
            else
                EndDrag();
        }
        else if (e is InputEventMouseMotion)
        {
            if (_dragged != null)
                UpdateDrag();
        }
    }

    private Vector2 GetMouse()
    {
        return GetViewport().GetMousePosition();
    }

    // =========================================================
    // DRAG LOGIC
    // =========================================================

    private void StartDrag()
    {
        foreach (Node node in GetChildren())
        {
            if (node is Control c)
            {
                if (c.GetGlobalRect().HasPoint(GetMouse()))
                {
                    _dragged = c;
                    _dragOffset = c.GlobalPosition - GetMouse();

                    _dragged.ZIndex = 1000;
                    _dragged.Modulate = new Color(1, 1, 1, 0.6f);

                    _targetIndex = c.GetIndex();

                    break;
                }
            }
        }
    }

    private void UpdateDrag()
    {
        if (_dragged == null) return;

        _dragged.GlobalPosition = GetMouse() + _dragOffset;

        int newIndex = GetIndexFromMouse();

        if (newIndex != -1 && newIndex != _targetIndex)
        {
            _targetIndex = newIndex;
            QueueRedraw();
        }
    }

    private void EndDrag()
    {
        if (_dragged == null) return;

        MoveChild(_dragged, _targetIndex);

        _dragged.ZIndex = 0;
        _dragged.Modulate = Colors.White;

        _dragged = null;

        QueueSort();
        QueueRedraw();
    }

    private int GetIndexFromMouse()
    {
        int index = 0;

        foreach (Node node in GetChildren())
        {
            if (node is Control c && c != _dragged)
            {
                if (c.GetGlobalRect().HasPoint(GetMouse()))
                    return index;

                index++;
            }
        }

        return index;
    }

    // =========================================================
    // DRAW
    // =========================================================

    public override void _Draw()
    {
        DrawIndices();
        DrawPlaceholder();
    }

    private void DrawIndices()
    {
        int i = 1;

        foreach (Node node in GetChildren())
        {
            if (node is Control c)
            {
                Vector2 center = c.Position + c.Size / 2;

                DrawString(
                    ThemeDB.FallbackFont,
                    center,
                    i.ToString(),
                    HorizontalAlignment.Center,
                    -1,
                    18,
                    Colors.White
                );

                i++;
            }
        }
    }

    private void DrawPlaceholder()
    {
        if (_dragged == null || _targetIndex == -1) return;

        int col = _targetIndex % Columns;
        int row = _targetIndex / Columns;

        Vector2 cellSize = GetCellSize();

        Rect2 rect = new Rect2(
            new Vector2(
                col * (cellSize.X + Separation.X),
                row * (cellSize.Y + Separation.Y)
            ),
            cellSize
        );

        DrawRect(rect, new Color(0, 1, 0, 0.25f), false, 2f);
    }
}