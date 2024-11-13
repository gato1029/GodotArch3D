using Godot;

using System;

public partial class CameraController : Camera2D
{
    [Export] private float moveSpeed = 400f;
    [Export] private float zoomSpeed = 0.1f;
    [Export] private float minZoom = 0.2f;
    [Export] private float maxZoom = 2f;
    [Export] private float borderSize = 10f;

    private Vector2 _screenSize;
    private Viewport _viewport;
    private bool isMouseInsideViewport = false;
    
    public override void _Ready()
    {

        _viewport = GetViewport();
        string nombre = _viewport.Name;
        _screenSize = GetViewportRect().Size;
        _viewport.Connect("size_changed", new Callable(this, "ViewPortChanged"));
        //_viewport.Connect("mouse_entered", new Callable(this, "OnMouseEnteredViewport"));
        //_viewport.Connect("mouse_exited", new Callable(this, "OnMouseExitedViewport"));
    }
    private void OnMouseEnteredViewport()
    {
        isMouseInsideViewport = true;
    }

    private void OnMouseExitedViewport()
    {
        isMouseInsideViewport = false;
    }

    private void ViewPortChanged()
    {
        _screenSize = GetViewportRect().Size;
    }

    public override void _Process(double delta)
    {
        if (RenderWindowGui.Instance.IsActive)
        {
            MoveCamera((float)delta);
        }
    }

    private void MoveCamera(float delta)
    {

        Vector2 movement = Vector2.Zero;

        if (Input.IsActionPressed("ui_up"))
            movement.Y -= 1;
        if (Input.IsActionPressed("ui_down"))
            movement.Y += 1;
        if (Input.IsActionPressed("ui_left"))
            movement.X -= 1;
        if (Input.IsActionPressed("ui_right"))
            movement.X += 1;

        Vector2 mousePosition = GetViewport().GetMousePosition();

        if (movement.Length() > 0)
        {
            movement = movement.Normalized() * moveSpeed * delta;
            Position += movement;
        }

        if (mousePosition.X < borderSize)
            Position += new Vector2(-moveSpeed * delta, 0);
        if (mousePosition.X > _screenSize.X - borderSize)
            Position += new Vector2(moveSpeed * delta, 0);
        if (mousePosition.Y < borderSize)
            Position += new Vector2(0, -moveSpeed * delta);
        if (mousePosition.Y > _screenSize.Y - borderSize)
            Position += new Vector2(0, moveSpeed * delta);


        Rect2 viewportRect = new Rect2(Position - _screenSize / 2, _screenSize);
      //  ClampCamera(viewportRect);
    }

    private void ClampCamera(Rect2 viewportRect)
    {


        if (Position.X < viewportRect.Position.X)
            Position = new Vector2(viewportRect.Position.X, Position.Y);
        if (Position.X > viewportRect.Position.X + viewportRect.Size.X)
            Position = new Vector2(viewportRect.Position.X + viewportRect.Size.X, Position.Y);
        if (Position.Y < viewportRect.Position.Y)
            Position = new Vector2(Position.X, viewportRect.Position.Y);
        if (Position.Y > viewportRect.Position.Y + viewportRect.Size.Y)
            Position = new Vector2(Position.X, viewportRect.Position.Y + viewportRect.Size.Y);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Pressed && eventKey.Keycode == Key.Escape)
            { GetTree().Quit(); }        
        }
  

    }

    public override void _Input(InputEvent @event)
    {
       

        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.WheelDown && mouseButtonEvent.Pressed)
            {
                Zoom -= new Vector2(zoomSpeed, zoomSpeed);
            }
            else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp && mouseButtonEvent.Pressed)
            {
                Zoom += new Vector2(zoomSpeed, zoomSpeed);
            }
            Zoom = new Vector2(Mathf.Clamp(Zoom.X, minZoom, maxZoom), Mathf.Clamp(Zoom.Y, minZoom, maxZoom)); 
        }
    }

}
