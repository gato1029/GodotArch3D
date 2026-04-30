using Godot;
using System;

namespace GodotEcsArch.sources.BlackyTextures
{
    public partial class BlackyAtlasCameraViewport : SubViewport
    {
        [Export] public float ZoomStep = 0.1f;
        [Export] public Vector2 MinZoom = new Vector2(0.3f, 0.3f);
        [Export] public Vector2 MaxZoom = new Vector2(4f, 4f);

        [Export] public float EdgeScrollSpeed = 500f;
        [Export] public int EdgeThreshold = 20;

        [Export] public bool EnableEdgeMovement = false;
        [Export] public bool EnableDrag = true;

        [Export] public Camera2D AtlasCamera;
        [Export] public BlackyAtlasSelectionCanvas AtlasCanvas;

        private bool isDragging = false;
        private Vector2 lastMousePos;

        public event Action<float> OnZoomChanged;
        public event Action<Vector2> OnCameraMoved;

        public override void _Ready()
        {
            SizeChanged += CenterCameraOnViewport;
            CallDeferred(nameof(SyncSizeFromContainer));
            CenterCameraOnViewport();
        }
        private void SyncSizeFromContainer()
        {
            if (GetParent() is SubViewportContainer container)
            {
                Size = (Vector2I)container.Size;
                container.Resized += OnContainerResized;
            }
        }
        public Vector2 ScreenToWorld(Vector2 screenPos)
        {
            // Size is a Vector2I (integer). Convert to Vector2 before floating-point division.
            Vector2 viewportCenter = new Vector2(Size.X, Size.Y) / 2f;

            Vector2 offsetFromCenter = screenPos - viewportCenter;

            Vector2 world = AtlasCamera.Position + (offsetFromCenter / AtlasCamera.Zoom);

            return world;
        }
        private void OnContainerResized()
        {
            if (GetParent() is SubViewportContainer container)
            {
                Size = (Vector2I)container.Size;
                CenterCameraOnViewport();
            }
        }
        public override void _Process(double delta)
        {
            if (EnableEdgeMovement)
                HandleEdgeMovement(delta);
        }

        public override void _Input(InputEvent @event)
        {
            HandleZoom(@event);

            if (EnableDrag)
                HandleDrag(@event);
        }

        private void HandleZoom(InputEvent @event)
        {
            if (@event is not InputEventMouseButton mouseEvent)
                return;

            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
                ApplyZoom(ZoomStep);
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
                ApplyZoom(-ZoomStep);
        }

        private void ApplyZoom(float zoomDelta)
        {
            Vector2 newZoom = (AtlasCamera.Zoom + new Vector2(zoomDelta, zoomDelta)).Clamp(MinZoom, MaxZoom);

            AtlasCamera.Zoom = newZoom;

            AtlasCanvas?.RefreshLineWidth(newZoom.X);

            OnZoomChanged?.Invoke(newZoom.X);
        }

        private void HandleDrag(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Middle && mouseEvent.Pressed)
                {
                    isDragging = true;
                    lastMousePos = GetMousePosition();
                }
                else if (mouseEvent.ButtonIndex == MouseButton.Middle && !mouseEvent.Pressed)
                {
                    isDragging = false;
                }
            }

            if (@event is InputEventMouseMotion motion && isDragging)
            {
                Vector2 mousePos = motion.Position;
                Vector2 delta = mousePos - lastMousePos;

                AtlasCamera.Position -= delta / AtlasCamera.Zoom;

                lastMousePos = mousePos;

                OnCameraMoved?.Invoke(AtlasCamera.Position);
            }
        }

        private void HandleEdgeMovement(double delta)
        {
            if (isDragging)
                return;

            Vector2 mousePos = GetMousePosition();
            Vector2 moveDir = Vector2.Zero;

            if (mousePos.X < 0 || mousePos.Y < 0 || mousePos.X > Size.X || mousePos.Y > Size.Y)
                return;

            if (mousePos.X < EdgeThreshold)
                moveDir.X -= 1;
            else if (mousePos.X > Size.X - EdgeThreshold)
                moveDir.X += 1;

            if (mousePos.Y < EdgeThreshold)
                moveDir.Y -= 1;
            else if (mousePos.Y > Size.Y - EdgeThreshold)
                moveDir.Y += 1;

            if (moveDir != Vector2.Zero)
            {
                moveDir = moveDir.Normalized();
                AtlasCamera.Position += moveDir * (EdgeScrollSpeed * (float)delta) / AtlasCamera.Zoom;

                OnCameraMoved?.Invoke(AtlasCamera.Position);
            }
        }

        public void CenterCameraOnViewport()
        {
            AtlasCamera.Position = Vector2.Zero;
            OnCameraMoved?.Invoke(AtlasCamera.Position);
        }

        public void FocusWorldPosition(Vector2 worldPos)
        {
            AtlasCamera.Position = worldPos;
            OnCameraMoved?.Invoke(AtlasCamera.Position);
        }

        public void SetAbsoluteZoom(float zoomValue)
        {
            Vector2 newZoom = new Vector2(zoomValue, zoomValue).Clamp(MinZoom, MaxZoom);

            AtlasCamera.Zoom = newZoom;

            AtlasCanvas?.RefreshLineWidth(newZoom.X);

            OnZoomChanged?.Invoke(newZoom.X);
        }

        public float CurrentZoom => AtlasCamera.Zoom.X;
        public Vector2 CameraPosition => AtlasCamera.Position;
    }
}