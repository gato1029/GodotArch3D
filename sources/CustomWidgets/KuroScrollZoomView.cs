using Godot;

[Tool]
[GlobalClass]
public partial class KuroScrollZoomView : MarginContainer
{
    // 🔹 CONFIGURACIÓN EXTRA
    [Export] public bool PanEnabled { get; set; } = true;
    [Export] public bool ZoomOnlyAtCenter { get; set; } = false;

    // 🔥 NUEVO: modo pixel perfect
    [Export] public bool PixelPerfectZoom { get; set; } = false;

    // 🔹 INPUT CONFIG
    [Export(PropertyHint.Flags, "Left,Right,Middle,Back:128,Forward:256")]
    public MouseButtonMask PanButton = MouseButtonMask.Left | MouseButtonMask.Middle;

    private Vector2 _childSize = new Vector2(-1, -1);
    [Export]
    public Vector2 ChildSize
    {
        get => _childSize;
        set { _childSize = value; ApplyChildSize(); }
    }

    [Export] public Vector2 ZoomRange = new Vector2(0.125f, 8.0f);
    [Export] public float PanMinDistance = 4f;
    [Export(PropertyHint.Range, "1,2")] public float ZoomStep = 1.2f;
    [Export(PropertyHint.Range, "0,1")] public float ZoomInterpSpeed = 0.25f;
    [Export] public bool ZoomUseMouseAsPivot = true;

    // 🔹 STATE
    private float _zoomAmount = 1f;
    [Export]
    public float ZoomAmount
    {
        get => _zoomAmount;
        set
        {
            if (PixelPerfectZoom)
                _zoomAmount = Mathf.Clamp(Mathf.Round(value), ZoomRange.X, ZoomRange.Y);
            else
                _zoomAmount = Mathf.Clamp(value, ZoomRange.X, ZoomRange.Y);

            SetProcess(true);
        }
    }

    private Vector2 _scrollOffset = Vector2.Zero;
    [Export]
    public Vector2 ScrollOffset
    {
        get => _scrollOffset;
        set { _scrollOffset = value; ApplyScrollOffset(); }
    }

    // 🔹 INTERNAL
    private bool _inputDragging = false;
    private Vector2 _inputDragStart;
    private bool _inputDragCanMove = false;
    private Vector2 _zoomPivot = Vector2.Zero;
    private float _zoomVisible = 1f;

    public override void _Ready()
    {
        ApplyChildSize();
        ApplyScrollOffset();
        ApplyTextureFilter();
    }

    // 🔥 Fuerza texturas nítidas
    private void ApplyTextureFilter()
    {
        if (GetChildCount() == 0) return;

        if (GetChild(0) is CanvasItem child)
        {
            child.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        }
    }

    private void ApplyChildSize()
    {
        if (!IsInsideTree() || GetChildCount() == 0) return;

        if (GetChild(0) is Control child)
        {
            child.Size = new Vector2(
                _childSize.X >= 0 ? _childSize.X : Size.X,
                _childSize.Y >= 0 ? _childSize.Y : Size.Y
            );
        }
    }

    private void ApplyScrollOffset()
    {
        if (!IsInsideTree() || GetChildCount() == 0) return;

        if (GetChild(0) is Control child)
            child.Position = _scrollOffset.Round(); // 🔥 evita blur por subpíxel
    }

    private void ApplyZoom()
    {
        if (!IsInsideTree() || GetChildCount() == 0) return;

        if (GetChild(0) is Control child)
            child.Scale = new Vector2(_zoomVisible, _zoomVisible);
    }

    public override void _Process(double delta)
    {
        // 🔹 Pivote
        if (ZoomOnlyAtCenter || !ZoomUseMouseAsPivot)
            _zoomPivot = Size * 0.5f;

        float oldZoom = _zoomVisible;

        if (PixelPerfectZoom)
        {
            // 🔥 Sin interpolación = ultra nítido
            _zoomVisible = _zoomAmount;
        }
        else
        {
            // 🔹 Zoom suave
            _zoomVisible = Mathf.Lerp(_zoomVisible, _zoomAmount, ZoomInterpSpeed);

            // 🔥 Reduce blur por decimales extremos
            _zoomVisible = Mathf.Round(_zoomVisible * 100f) / 100f;
        }

        ApplyZoom();

        Vector2 scrollOffsetResult = _scrollOffset - _zoomPivot;
        scrollOffsetResult *= _zoomVisible / oldZoom;
        ScrollOffset = scrollOffsetResult + _zoomPivot;

        if (Mathf.IsEqualApprox(_zoomAmount, _zoomVisible))
        {
            _zoomVisible = _zoomAmount;
            SetProcess(false);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            if (!_inputDragging)
            {
                if (!ZoomOnlyAtCenter)
                    _zoomPivot = motion.Position;
                return;
            }

            if (!PanEnabled) return;

            if (!_inputDragCanMove)
            {
                if (((motion.Position - _inputDragStart) / _zoomAmount).LengthSquared() > PanMinDistance * PanMinDistance)
                    _inputDragCanMove = true;
                return;
            }

            ScrollOffset += motion.Relative;
        }

        if (@event is InputEventMouseButton mouse)
        {
            // 🔹 Paneo
            if ((PanButton & (MouseButtonMask)(1 << (int)(mouse.ButtonIndex - 1))) != 0)
            {
                if (PanEnabled)
                {
                    _inputDragging = mouse.Pressed;
                    _inputDragStart = mouse.Position;
                    _inputDragCanMove = false;
                }
            }

            // 🔹 Zoom
            if (mouse.ButtonIndex == MouseButton.WheelUp)
            {
                ZoomAmount = PixelPerfectZoom
                    ? _zoomAmount + 1
                    : _zoomAmount * (1f + (ZoomStep - 1f) * mouse.Factor);
            }
            else if (mouse.ButtonIndex == MouseButton.WheelDown)
            {
                ZoomAmount = PixelPerfectZoom
                    ? _zoomAmount - 1
                    : _zoomAmount / (1f + (ZoomStep - 1f) * mouse.Factor);
            }
        }
    }
}