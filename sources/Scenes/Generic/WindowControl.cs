using Godot;
using System;
[Tool]
public partial class WindowControl : VBoxContainer
{

    [Export] Container PlaceContainer;
    private Vector2 _originalSize;
    private Vector2 _originalPosition;

    private bool _isMaximized = false;
    private bool _isMinimized = false;

    private bool _isDragging = false;
    private Vector2 _dragOffset = Vector2.Zero;
    private const float ResizeMargin = 10f;

    private bool _resizing = false;
    private Vector2 _resizeStartMouse;
    private Vector2 _resizeStartSize;
    private Vector2 _resizeStartPos;
    private ResizeDirection _resizeDir = ResizeDirection.None;

    private Control _windowContent;
    private bool _isReady = false;
    private bool _updateContentPlacementFirstCall = true;
    [Flags]
    private enum ResizeDirection
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
    }
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI


        ButtonMax.Pressed += OnMaximizePressed;
        ButtonMin.Pressed += OnMinimizePressed;
        ButtonClose.Pressed += OnClosePressed;
        TitleBar.GuiInput += OnTitleBarGuiInput;
        _originalSize = Size;
        _originalPosition = Position;
        //ChildEnteredTree += _ChildEnteredTree;
        //ChildExitingTree += _ChildExitingTree;
        _isReady = true;
        UpdateContentPlacement();
    }
    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            UpdateContentPlacementDeferred();
    }
    private void UpdateContentPlacement()
    {
        if (!_updateContentPlacementFirstCall) return;

       // PopupRect = GetRect();
        _updateContentPlacementFirstCall = false;

        CallDeferred("UpdateContentPlacementDeferred");
        CallDeferred("ResetPlacementFlag");
    }

    private void ResetPlacementFlag()
    {
        _updateContentPlacementFirstCall = true;
    }

    private void UpdateContentPlacementDeferred()
    {
        if (!_isReady && !Engine.IsEditorHint()) return;

        if (Background == null)
        {
            //_contentPlaceholder = GetNode<ColorRect>("VBoxContainer/HBoxContainer2/CenterPanel/MarginContainer/ContentPlaceholder");
            if (Background == null) return;
        }

        if (GetChildCount() < 1) return;

        // Usamos el último hijo como contenido
        var child = GetChild(GetChildCount() - 1);
        if (child is Control content && content.Name != "Background")
        {
            _windowContent = content;
            _windowContent.GlobalPosition = Background.GlobalPosition;
            _windowContent.Size = Background.Size;
        }
    }

    private void OnGuiInputResizeLeft(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && motion.ButtonMask == MouseButtonMask.Left)
        {
            Size = new Vector2(Size.X - motion.Relative.X, Size.Y);
            Position = new Vector2(Position.X + motion.Relative.X, Position.Y);
            UpdateContentPlacement();
        }
    }
    private void OnGuiInputMove(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion && motion.ButtonMask == MouseButtonMask.Left)
        {
            Position += motion.Relative;
            UpdateContentPlacement();
        }
    }
    //private void _ChildExitingTree(Node node)
    //{
    //    if (!Engine.IsEditorHint()) { return; }
    //    GD.Print($"Child exiting tree: {node.Name}");
    //    MainContainer.RemoveChild(node);
    //}

    //private void _ChildEnteredTree(Node node)
    //{
    //    if (!Engine.IsEditorHint()) return;

    //    RemoveChild(node); // Aseguramos que no esté ya en el contenedor antes de añadirlo
    //    PlaceContainer.AddChild(node);

    //    GD.Print($"Child entered tree: {node.Name}");
    //}

    public override void _GuiInput(InputEvent @event)
    {
        if (_isMaximized || _isMinimized)
            return;

        var localMouse = GetLocalMousePosition();

        if (@event is InputEventMouseMotion motionEvent)
        {
            if (!_resizing)
            {
                var dir = GetResizeDirection(localMouse);
                UpdateMouseCursor(dir);
            }
            else
            {
                Vector2 mouseDelta = GetGlobalMousePosition() - _resizeStartMouse;
                Vector2 newSize = _resizeStartSize;
                Vector2 newPos = _resizeStartPos;

                if (_resizeDir.HasFlag(ResizeDirection.Right))
                    newSize.X = Mathf.Max(100, _resizeStartSize.X + mouseDelta.X);
                if (_resizeDir.HasFlag(ResizeDirection.Bottom))
                    newSize.Y = Mathf.Max(100, _resizeStartSize.Y + mouseDelta.Y);
                if (_resizeDir.HasFlag(ResizeDirection.Left))
                {
                    newSize.X = Mathf.Max(100, _resizeStartSize.X - mouseDelta.X);
                    newPos.X = _resizeStartPos.X + mouseDelta.X;
                }
                if (_resizeDir.HasFlag(ResizeDirection.Top))
                {
                    newSize.Y = Mathf.Max(100, _resizeStartSize.Y - mouseDelta.Y);
                    newPos.Y = _resizeStartPos.Y + mouseDelta.Y;
                }

                Position = newPos;
                Size = newSize;
            }
        }

        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    var dir = GetResizeDirection(localMouse);
                    if (dir != ResizeDirection.None)
                    {
                        _resizing = true;
                        _resizeDir = dir;
                        _resizeStartMouse = GetGlobalMousePosition();
                        _resizeStartSize = Size;
                        _resizeStartPos = Position;
                        GetViewport().SetInputAsHandled();
                    }
                }
                else
                {
                    _resizing = false;
                    _resizeDir = ResizeDirection.None;
                }
            }
        }
    }

    private ResizeDirection GetResizeDirection(Vector2 localMouse)
    {
        ResizeDirection dir = ResizeDirection.None;

        if (localMouse.X <= ResizeMargin)
            dir |= ResizeDirection.Left;
        else if (localMouse.X >= Size.X - ResizeMargin)
            dir |= ResizeDirection.Right;

        if (localMouse.Y <= ResizeMargin)
            dir |= ResizeDirection.Top;
        else if (localMouse.Y >= Size.Y - ResizeMargin)
            dir |= ResizeDirection.Bottom;

        return dir;
    }

    private void UpdateMouseCursor(ResizeDirection dir)
    {
        switch (dir)
        {
            case ResizeDirection.Left:
            case ResizeDirection.Right:
                MouseDefaultCursorShape = CursorShape.Hsize;
                break;
            case ResizeDirection.Top:
            case ResizeDirection.Bottom:
                MouseDefaultCursorShape = CursorShape.Vsize;
                break;
            case ResizeDirection.Top | ResizeDirection.Left:
            case ResizeDirection.Bottom | ResizeDirection.Right:
                MouseDefaultCursorShape = CursorShape.Fdiagsize;
                break;
            case ResizeDirection.Top | ResizeDirection.Right:
            case ResizeDirection.Bottom | ResizeDirection.Left:
                MouseDefaultCursorShape = CursorShape.Bdiagsize;
                break;
            default:
                MouseDefaultCursorShape = CursorShape.Arrow;
                break;
        }
    }
    private void OnTitleBarGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                // ⬅️ Primero, verificamos si fue doble clic
                if (mouseEvent.DoubleClick)
                {
                    OnMaximizePressed();
                    return; // Evitamos que inicie un drag
                }

                // ⬅️ Si no fue doble clic, iniciamos drag al presionar
                if (mouseEvent.Pressed)
                {
                    if (!_isMaximized)
                    {
                        _isDragging = true;
                        _dragOffset = GetGlobalMousePosition() - GlobalPosition;
                    }
                }
                else
                {
                    _isDragging = false;
                }
            }
        }

        if (@event is InputEventMouseMotion && _isDragging)
        {
            GlobalPosition = GetGlobalMousePosition() - _dragOffset;
        }
    }

    private void OnMaximizePressed()
    {
        if (_isMaximized)
        {
            Size = _originalSize;
            Position = _originalPosition;
            _isMaximized = false;
        }
        else
        {
            _originalSize = Size;
            _originalPosition = Position;

            var parent = GetParent<Control>();
            if (parent != null)
            {
                Position = Vector2.Zero;
                Size = parent.GetRect().Size;
                _isMaximized = true;
            }
        }
    }

    private void OnMinimizePressed()
    {
        if (_isMinimized)
        {
            Size = _originalSize;
            Position = _originalPosition;
            _isMinimized = false;
        }
        else
        {
            _originalSize = Size;
            _originalPosition = Position;

            float minimizedHeight = 30; // altura de la barra de título
            Position = new Vector2(0, GetViewportRect().Size.Y - minimizedHeight);
            Size = new Vector2(200, minimizedHeight); // ancho mínimo
            _isMinimized = true;
        }
    }

    private void OnClosePressed()
    {
        QueueFree(); // o Hide() si quieres que sea reutilizable
    }
}
