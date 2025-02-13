using Godot;
using GodotEcsArch.sources.imgui;
using GodotEcsArch.sources.managers;
using ImGuiNET;
using System;

public partial class Camera3dGodot : Camera3D
{
    [Export] public float CameraSpeed = 10.0f;
    [Export] public float MouseEdgeMargin = 50.0f; // Margin in pixels from the edge of the screen to start moving the camera
    [Export] public float ZoomSpeed = 1.0f; // Speed of zooming
    [Export] public float MinZoomSize = 20.0f; // Minimum size of the camera's Orthogonal or Perspective size
    [Export] public float MaxZoomSize = 40.0f; // Maximum size of the camera's Orthogonal or Perspective size

    [Export] private float borderSize = 10f;
    private float currentZoomSize = 20.0f; // Initial zoom size of the camera

    private Vector2 _screenSize;
    private Viewport _viewport;
    private Camera3D camera;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        camera = this;
        _viewport = GetViewport();
        string nombre = _viewport.Name;
        _screenSize = GetViewport().GetVisibleRect().Size;
        _viewport.Connect("size_changed", new Callable(this, "ViewPortChanged"));
       
        RenderManager.Instance.currentDisplay = RectRender();
    }

 

    private void ViewPortChanged()
    {
        _screenSize = _viewport.GetVisibleRect().Size;
        RenderManager.Instance.currentDisplay = RectRender();
    }
    private bool isInScreen()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector2 windowSize = _screenSize;
        if (mousePos.X < 0 || mousePos.Y < 0 || mousePos.X > windowSize.X || mousePos.Y > windowSize.Y)
        {
            return false;            
        }
        return true;
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        
        if (isInScreen())
        {
            MoveCamera((float)delta);

            var cameraPosition = camera.ProjectRayOrigin(GetViewport().GetMousePosition());
            PositionsManager.Instance.positionMouseCamera = new Vector2(cameraPosition.X, cameraPosition.Y);
            PositionsManager.Instance.positionMouseTileGlobal = new Vector2(MathF.Floor(cameraPosition.X), MathF.Floor(cameraPosition.Y));
            PositionsManager.Instance.positionMouseChunk = new Vector2(
                MathF.Floor(PositionsManager.Instance.positionMouseTileGlobal.X / PositionsManager.Instance.chunkDimencion.X), 
                MathF.Floor(PositionsManager.Instance.positionMouseTileGlobal.Y / PositionsManager.Instance.chunkDimencion.Y));

            var calc = PositionsManager.Instance.positionMouseChunk * PositionsManager.Instance.chunkDimencion;
            PositionsManager.Instance.positionMouseTileChunk = PositionsManager.Instance.positionMouseTileGlobal - calc;
            PositionsManager.Instance.positionCamera = new Vector2(this.Position.X, this.Position.Y);
            PositionsManager.Instance.positionMouseCameraPixel = ConvertMouseToPixel(PositionsManager.Instance.positionMouseCamera);
        }
      
    }
    public Vector2 ConvertMouseToPixel(Vector2 mousePositionUnits)
    {
        float pixelsPerUnit = 32.0f; // 1 unidad = 32 píxeles
        return new Vector2(Mathf.Round(mousePositionUnits.X * pixelsPerUnit),
                        Mathf.Round(mousePositionUnits.Y * pixelsPerUnit));
    }
    private void MoveCamera(float delta)
    {
        Vector2 mousePosition = GetViewport().GetMousePosition();
        Vector2 viewportSize = GetViewport().GetWindow().Size;

        Vector3 direction = Vector3.Zero;
      
        {
            // Check if the mouse is near the edges of the screen and set the direction accordingly
            if (mousePosition.X < MouseEdgeMargin)
            {
                direction.X -= 1;

            }
            if (mousePosition.X > _screenSize.X - MouseEdgeMargin)
            {
                direction.X += 1;

            }

            if (mousePosition.Y < MouseEdgeMargin)
            {
                direction.Y += 1;

            }
            if (mousePosition.Y > _screenSize.Y - MouseEdgeMargin)
            {
                direction.Y -= 1;

            }

            // Normalize the direction to ensure consistent speed
            if (direction != Vector3.Zero)
            {
                direction = direction.Normalized();

            }

            // Move the camera

            Vector3 move = camera.Position + direction * CameraSpeed * (float)delta;
            camera.Position = move;
            RenderManager.Instance.currentDisplay = RectRender();
        }
    }

    public Rect2 RectRender()
    {
        Rect2 visibleRect = GetViewport().GetVisibleRect();

        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;


        float halfHeight = camera.Size;
        float halfWidth = halfHeight * (viewportSize.X / viewportSize.Y);


        Vector2 topLeft = new Vector2(camera.GlobalTransform.Origin.X, camera.GlobalTransform.Origin.Y) + new Vector2(-halfWidth, -halfHeight);
        Vector2 bottomRight = new Vector2(camera.GlobalTransform.Origin.X, camera.GlobalTransform.Origin.Y) + new Vector2(halfWidth, halfHeight);


        Rect2 worldRect = new Rect2(topLeft, bottomRight - topLeft);

        

        return worldRect;
    }
    private bool IsMouseInsideGameScreen(Vector2 mousePosition, Vector2 viewportSize)
    {
        return mousePosition.X >= 0 && mousePosition.X <= viewportSize.X &&
               mousePosition.Y >= 0 && mousePosition.Y <= viewportSize.Y;
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            float zoomInput = 0;
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
            {
                zoomInput = 1;

            }
            if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
            {
                zoomInput = -1;

            }



            currentZoomSize = Mathf.Clamp(currentZoomSize - zoomInput * ZoomSpeed, MinZoomSize, MaxZoomSize);
            SetCameraZoom(currentZoomSize);
        }


    }
    private void SetCameraZoom(float zoomSize)
    {
        camera.Size = zoomSize;
        RenderManager.Instance.currentDisplay = RectRender();
    }
}
