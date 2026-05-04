using Godot;
using System;

public class RuntimeWindowDockController
{
    public enum WindowDockPosition
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        Center,
        MiddleRight,

        BottomLeft,
        BottomCenter,
        BottomRight
    }

    private Window owner;
    private WindowDockPosition currentDock = WindowDockPosition.Center;
    private int currentMarginX = 10;
    private int currentMarginY = 10;

    public RuntimeWindowDockController(Window windowOwner)
    {
        owner = windowOwner;
        owner.GetTree().Root.SizeChanged += OnRootSizeChanged;
    }

    public void Dispose()
    {
        if (owner?.GetTree()?.Root != null)
            owner.GetTree().Root.SizeChanged -= OnRootSizeChanged;
    }

    private async void OnRootSizeChanged()
    {
        await owner.ToSignal(owner.GetTree(), SceneTree.SignalName.ProcessFrame);
        DockWindow(currentDock, currentMarginX, currentMarginY);
    }

    public void DockWindow(WindowDockPosition dockPosition, int marginX = 10, int marginY = 10)
    {
        currentDock = dockPosition;
        currentMarginX = marginX;
        currentMarginY = marginY;

        Vector2I rootSize = owner.GetTree().Root.Size;
        Vector2I mySize = owner.Size;

        int x = 0;
        int y = 0;

        switch (dockPosition)
        {
            case WindowDockPosition.TopLeft:
                x = marginX;
                y = marginY;
                break;

            case WindowDockPosition.TopCenter:
                x = (rootSize.X / 2) - (mySize.X / 2);
                y = marginY;
                break;

            case WindowDockPosition.TopRight:
                x = rootSize.X - mySize.X - marginX;
                y = marginY;
                break;

            case WindowDockPosition.MiddleLeft:
                x = marginX;
                y = (rootSize.Y / 2) - (mySize.Y / 2);
                break;

            case WindowDockPosition.Center:
                x = (rootSize.X / 2) - (mySize.X / 2);
                y = (rootSize.Y / 2) - (mySize.Y / 2);
                break;

            case WindowDockPosition.MiddleRight:
                x = rootSize.X - mySize.X - marginX;
                y = (rootSize.Y / 2) - (mySize.Y / 2);
                break;

            case WindowDockPosition.BottomLeft:
                x = marginX;
                y = rootSize.Y - mySize.Y - marginY;
                break;

            case WindowDockPosition.BottomCenter:
                x = (rootSize.X / 2) - (mySize.X / 2);
                y = rootSize.Y - mySize.Y - marginY;
                break;

            case WindowDockPosition.BottomRight:
                x = rootSize.X - mySize.X - marginX;
                y = rootSize.Y - mySize.Y - marginY;
                break;
        }

        owner.Position = new Vector2I(x, y);
    }
}