using Godot;
using System;

public partial class TilesSubViewport : SubViewport
{
    [Export] float zoomStep = 0.1f;
    [Export] Vector2 minZoom = new Vector2(0.3f, 0.3f);
    [Export] Vector2 maxZoom = new Vector2(4f, 4f);
    [Export] Camera2D tilesCamera2d;
    private bool _isDragging = false;
    private Vector2 _lastMousePos;

    [Export] float edgeScrollSpeed = 500f; // velocidad base en píxeles por segundo
    [Export] int edgeThreshold = 20;       // distancia al borde para activar el desplazamiento

    [Export ] bool enableEdgeMovement = false;
    [Export] bool  enabledrag = false;
    public delegate void EventNotifySelectionCamera(float zoom);
    public event EventNotifySelectionCamera OnNotifySelectionCameraZoom;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        SizeChanged += viewportSizeChange; 
        CenterCameraOnViewport();
    }

    private void viewportSizeChange()
    {
        CenterCameraOnViewport();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (enableEdgeMovement)
        {
            HandleEdgeMovement(delta);
        }
    }


    public override void _Input(InputEvent @event)
    {
        HandleZoom(@event);
        if (enabledrag)
        {
            HandleDrag(@event);
        }
        
    }
    // ----------------------------
    // 🔍 ZOOM CONTROL
    // ----------------------------
    private void HandleZoom(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
                ZoomAtScreenCenter(zoomStep);
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)                
                ZoomAtScreenCenter(-zoomStep);
        }
    }
    // ----------------------------
    // ✋ DRAG CONTROL
    // ----------------------------
    private void HandleDrag(InputEvent @event)
    {
        // Iniciar / detener arrastre
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Middle && mouseEvent.Pressed)
            {
                _isDragging = true;
                _lastMousePos = GetMousePosition();
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Middle && !mouseEvent.Pressed)
            {
                _isDragging = false;
            }
        }

        // Movimiento del ratón mientras se arrastra
        if (@event is InputEventMouseMotion motionEvent && _isDragging)
        {
            Vector2 mousePos = motionEvent.Position;
            Vector2 delta = mousePos - _lastMousePos;

            // Invertimos el movimiento para que se sienta natural
            tilesCamera2d.Position -= delta / tilesCamera2d.Zoom;

            _lastMousePos = mousePos;
            GD.Print("Moving camera to: " + tilesCamera2d.Position);
        }
    }

    private void HandleEdgeMovement(double delta)
    {
        if (_isDragging)
            return; // no mover si estás arrastrando manualmente

        Vector2 mousePos = GetMousePosition();
        Vector2 moveDir = Vector2.Zero;

        // Detectar si el mouse está dentro del área del SubViewport
        if (mousePos.X < 0 || mousePos.Y < 0 || mousePos.X > Size.X || mousePos.Y > Size.Y)
            return;

        // Revisar bordes
        if (mousePos.X < edgeThreshold)
            moveDir.X -= 1;
        else if (mousePos.X > Size.X - edgeThreshold)
            moveDir.X += 1;

        if (mousePos.Y < edgeThreshold)
            moveDir.Y -= 1;
        else if (mousePos.Y > Size.Y - edgeThreshold)
            moveDir.Y += 1;

        // Si hay movimiento, aplicar desplazamiento
        if (moveDir != Vector2.Zero)
        {
            moveDir = moveDir.Normalized();
            tilesCamera2d.Position += moveDir * (edgeScrollSpeed * (float)delta) / tilesCamera2d.Zoom;
         
        }
    }
    public void ZoomAtScreenCenter(float zoomChange)
    {
        Vector2 newZoom = (tilesCamera2d.Zoom + new Vector2(zoomChange, zoomChange)).Clamp(minZoom, maxZoom);
        tilesCamera2d.Zoom = newZoom;       
        OnNotifySelectionCameraZoom?.Invoke(newZoom.X);
    }

    public void CenterCameraOnViewport()
    {
        var vp = GetViewport();
        if (vp == null)
            return;
        // Tomamos el centro del viewport en coordenadas de pantalla del SubViewport
        Vector2 screenCenter = vp.GetVisibleRect().Size / 2;

        // Colocamos la cámara en ese punto
        tilesCamera2d.Position = screenCenter;
    }

    Vector2 zoomOrigin = new Vector2(1,1);
    internal void SetCameraPosition(float x, float y)
    {
        zoomOrigin = tilesCamera2d.Zoom;                
        tilesCamera2d.Position =  new Vector2(x,y);             
    }
}
