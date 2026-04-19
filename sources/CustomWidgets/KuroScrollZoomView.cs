using Godot;
using System;

[Tool]
[GlobalClass]
public partial class KuroScrollZoomView : MarginContainer
{
    // 🔹 INPUT CONFIG
    [Export(PropertyHint.Flags, "Left,Right,Middle,Back:128,Forward:256")]
    public MouseButtonMask PanButton =
    MouseButtonMask.Left | MouseButtonMask.Middle;

    private Vector2 _childSize = new Vector2(-1, -1);
    [Export]
    public Vector2 ChildSize
    {
        get => _childSize;
        set
        {
            _childSize = value;
            ApplyChildSize();
        }
    }

    [Export] public Vector2 ZoomRange = new Vector2(0.125f, 8.0f);
    [Export] public float PanMinDistance = 4f;

    [Export(PropertyHint.Range, "1,2")]
    public float ZoomStep = 1.2f;

    [Export(PropertyHint.Range, "0,1")]
    public float ZoomInterpSpeed = 0.25f;

    [Export] public bool ZoomUseMouseAsPivot = true;

    // 🔹 STATE

    private float _zoomAmount = 1f;
    [Export]
    public float ZoomAmount
    {
        get => _zoomAmount;
        set
        {
            _zoomAmount = value;
            SetProcess(true);
        }
    }

    private Vector2 _scrollOffset = Vector2.Zero;
    [Export]
    public Vector2 ScrollOffset
    {
        get => _scrollOffset;
        set
        {
            _scrollOffset = value;
            ApplyScrollOffset();
        }
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
    }

    private void ApplyChildSize()
    {
        if (!IsInsideTree()) return;
        if (GetChildCount() == 0) return;

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
        if (!IsInsideTree()) return;
        if (GetChildCount() == 0) return;

        if (GetChild(0) is Control child)
        {
            child.Position = _scrollOffset;
        }
    }

    private void ApplyZoom()
    {
        if (!IsInsideTree()) return;
        if (GetChildCount() == 0) return;

        if (GetChild(0) is Control child)
        {
            child.Scale = new Vector2(_zoomVisible, _zoomVisible);
        }
    }

    public override void _Process(double delta)
    {
        if (!ZoomUseMouseAsPivot)
        {
            _zoomPivot = GetGlobalTransform().AffineInverse().BasisXform(Size * 0.5f);
        }

        float oldZoom = _zoomVisible;
        _zoomVisible = Mathf.Lerp(_zoomVisible, _zoomAmount, ZoomInterpSpeed);

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
                _zoomPivot = GetGlobalTransform().AffineInverse() * motion.GlobalPosition;
                return;
            }

            if (!_inputDragCanMove)
            {
                if (((motion.Position - _inputDragStart) / _zoomAmount).LengthSquared() >
                    PanMinDistance * PanMinDistance)
                {
                    _inputDragCanMove = true;
                }

                return;
            }

            ScrollOffset += motion.Relative;
        }

        if (@event is InputEventMouseButton mouse)
        {
            if (mouse.ButtonIndex == MouseButton.Middle || mouse.ButtonIndex == MouseButton.Left)
            {
                _inputDragging = mouse.Pressed;
                _inputDragStart = mouse.Position;
                _inputDragCanMove = false;
            }
            else if (mouse.ButtonIndex == MouseButton.WheelUp)
            {
                ZoomAmount = Mathf.Min(
                    _zoomAmount * (1f + (ZoomStep - 1f) * mouse.Factor),
                    ZoomRange.Y
                );
            }
            else if (mouse.ButtonIndex == MouseButton.WheelDown)
            {
                ZoomAmount = Mathf.Max(
                    _zoomAmount / (1f + (ZoomStep - 1f) * mouse.Factor),
                    ZoomRange.X
                );
            }
        }
    }
}