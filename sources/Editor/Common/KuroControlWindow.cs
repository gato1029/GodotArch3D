using Godot;
using System;
using System.Composition;

[Tool]

public partial class KuroControlWindow : PanelContainer
{
    private const int BORDER_SIZE = 8;
    private const int MIN_WIDTH = 300;
    private const int MIN_HEIGHT = 300;

    private bool _isDragging = false;
    private bool _isResizing = false;

    private Vector2I _dragOffset;

    private Vector2I _startMousePos;
    private Vector2I _startWindowPos;
    private Vector2I _startWindowSize;

    private string _nameWindow = "Kuro Window";

    [Godot.Export]
    public string NameWindow
    {
        get => _nameWindow;
        set
        {
            _nameWindow = value;

            if (Engine.IsEditorHint())
            {
                CallDeferred(nameof(UpdateWindowTitle));
            }
            else
            {
                UpdateWindowTitle();
            }
        }
    }



    private void UpdateWindowTitle()
    {
        if (!IsInsideTree())
            return;

        if (LabelNameWindow == null)
            return;

        LabelNameWindow.Text = _nameWindow;

        QueueRedraw();
    }

    [Flags]
    private enum ResizeEdge
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8
    }

    private ResizeEdge _currentEdge = ResizeEdge.None;

    public override void _Ready()
    {
        InitializeUI();
        UpdateWindowTitle();
        // IMPORTANTE
        MouseFilter = MouseFilterEnum.Pass;

        HeadTexture.MouseFilter = MouseFilterEnum.Stop;

        ButtonClose.Pressed += ButtonClose_Pressed;
        HeadTexture.GuiInput += HeadTexture_GuiInput;
    }

    // =========================================================
    // DRAG GLOBAL
    // =========================================================
    public override void _Input(InputEvent @event)
    {
        if (GetParent() is not Window window)
            return;

        // Mover ventana
        if (_isDragging && @event is InputEventMouseMotion)
        {
            Vector2I mousePos = DisplayServer.MouseGetPosition();
            window.Position = mousePos - _dragOffset;
        }

        // Soltar mouse
        if (@event is InputEventMouseButton button)
        {
            if (button.ButtonIndex == MouseButton.Left && !button.Pressed)
            {
                _isDragging = false;
                _isResizing = false;
            }
        }
    }

    // =========================================================
    // RESIZE
    // =========================================================
    public override void _UnhandledInput(InputEvent @event)
    {
        if (GetParent() is not Window window)
            return;

        if (@event is InputEventMouseMotion motion)
        {
            Vector2 localMouse = GetLocalMousePosition();

            // Detectar bordes solo si no estamos arrastrando
            if (!_isDragging && !_isResizing)
            {
                _currentEdge = GetEdge(localMouse);
                UpdateCursor();
            }

            // Resize en tiempo real
            if (_isResizing)
            {
                ResizeWindow(window);
            }
        }

        if (@event is InputEventMouseButton button)
        {
            if (button.ButtonIndex == MouseButton.Left)
            {
                if (button.Pressed)
                {
                    if (_currentEdge != ResizeEdge.None)
                    {
                        _isResizing = true;

                        _startMousePos = DisplayServer.MouseGetPosition();
                        _startWindowPos = window.Position;
                        _startWindowSize = window.Size;
                    }
                }
            }
        }
    }

    // =========================================================
    // HEADER DRAG
    // =========================================================
    private void HeadTexture_GuiInput(InputEvent @event)
    {
        if (GetParent() is not Window window)
            return;

        if (@event is InputEventMouseButton button)
        {
            if (button.ButtonIndex == MouseButton.Left)
            {
                if (button.Pressed)
                {
                    _isDragging = true;

                    Vector2I mousePos = DisplayServer.MouseGetPosition();
                    _dragOffset = mousePos - window.Position;

                    AcceptEvent();
                }
                else
                {
                    _isDragging = false;
                }
            }
        }
    }

    // =========================================================
    // DETECTAR BORDES
    // =========================================================
    private ResizeEdge GetEdge(Vector2 pos)
    {
        ResizeEdge edge = ResizeEdge.None;

        if (pos.X <= BORDER_SIZE)
            edge |= ResizeEdge.Left;

        if (pos.X >= Size.X - BORDER_SIZE)
            edge |= ResizeEdge.Right;

        if (pos.Y <= BORDER_SIZE)
            edge |= ResizeEdge.Top;

        if (pos.Y >= Size.Y - BORDER_SIZE)
            edge |= ResizeEdge.Bottom;

        return edge;
    }

    // =========================================================
    // CURSOR
    // =========================================================
    private void UpdateCursor()
    {
        CursorShape shape = CursorShape.Arrow;

        bool horizontal =
            _currentEdge.HasFlag(ResizeEdge.Left) ||
            _currentEdge.HasFlag(ResizeEdge.Right);

        bool vertical =
            _currentEdge.HasFlag(ResizeEdge.Top) ||
            _currentEdge.HasFlag(ResizeEdge.Bottom);

        if (horizontal && vertical)
        {
            shape = CursorShape.Fdiagsize;
        }
        else if (horizontal)
        {
            shape = CursorShape.Hsize;
        }
        else if (vertical)
        {
            shape = CursorShape.Vsize;
        }

        MouseDefaultCursorShape = shape;
    }

    // =========================================================
    // RESIZE LOGIC
    // =========================================================
    private void ResizeWindow(Window window)
    {
        Vector2I mouseDelta =
            DisplayServer.MouseGetPosition() - _startMousePos;

        Vector2I newPos = _startWindowPos;
        Vector2I newSize = _startWindowSize;

        // Derecha
        if (_currentEdge.HasFlag(ResizeEdge.Right))
        {
            newSize.X += mouseDelta.X;
        }

        // Abajo
        if (_currentEdge.HasFlag(ResizeEdge.Bottom))
        {
            newSize.Y += mouseDelta.Y;
        }

        // Izquierda
        if (_currentEdge.HasFlag(ResizeEdge.Left))
        {
            newPos.X += mouseDelta.X;
            newSize.X -= mouseDelta.X;
        }

        // Arriba
        if (_currentEdge.HasFlag(ResizeEdge.Top))
        {
            newPos.Y += mouseDelta.Y;
            newSize.Y -= mouseDelta.Y;
        }

        // Clamp tamaño mínimo
        newSize.X = Mathf.Max(newSize.X, MIN_WIDTH);
        newSize.Y = Mathf.Max(newSize.Y, MIN_HEIGHT);

        window.Position = newPos;
        window.Size = newSize;
    }

    // =========================================================
    // CLOSE
    // =========================================================
    private void ButtonClose_Pressed()
    {
        if (GetParent() is Window)
        {
            GetParent().QueueFree();
        }
        else
        {
            QueueFree();
        }
    }
}