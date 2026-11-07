using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.GameRuntime.Internal.RTSControler;
using GodotEcsArch.sources.managers;



public enum WorldControlMode
{
    DefaultOrSelecting,
    BuildingPlacement
}

public partial class WorldControlManager : Node2D
{
    private BlackyWorld _world;
    private WorldControlMode _currentMode = WorldControlMode.DefaultOrSelecting;

    private RTSSelectionManager _selectionManager;
    private BuildingPlacementManager _buildingPlacementManager;

    // Variables de control visual y de arrastre (Box Selection)
    private bool _isDragging = false;
    private Vector2 _dragStartScreen = Vector2.Zero;
    private Vector2 _dragCurrentScreen = Vector2.Zero;
    private Vector2 _dragStartWorld = Vector2.Zero;
    private Vector2 _dragCurrentWorld = Vector2.Zero;
    private const float DragThreshold = 6.0f;

    public void Initialize(BlackyWorld world)
    {
        _world = world;
        _selectionManager = new RTSSelectionManager(world);
        _buildingPlacementManager = new BuildingPlacementManager(world);
        GD.Print("WorldControlManager Initialized");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_world == null) return;

        // Cancelar acción actual o modo de construcción con la tecla ESC
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
        {
            SwitchMode(WorldControlMode.DefaultOrSelecting);
            return;
        }

        // Delegar según el estado actual del juego
        switch (_currentMode)
        {
            case WorldControlMode.DefaultOrSelecting:
                HandleDefaultInput(@event);
                break;
            case WorldControlMode.BuildingPlacement:
                _buildingPlacementManager.HandleInput(@event, this);
                break;
        }
    }

    private void HandleDefaultInput(InputEvent @event)
    {
        // 1. Clic Derecho -> Órdenes (Mover, Atacar, FlowField)
        if (@event is InputEventMouseButton mbRight && mbRight.ButtonIndex == MouseButton.Right && mbRight.Pressed)
        {
            Vector2 targetWorldPos = PositionsManager.Instance.positionMouseCamera;
            _selectionManager.HandleRightClick(targetWorldPos);
            return;
        }

        // 2. Clic Izquierdo (Presionado / Soltado) -> Selección simple o inicio de Box Selection
        if (@event is InputEventMouseButton mbLeft && mbLeft.ButtonIndex == MouseButton.Left)
        {
            if (mbLeft.Pressed)
            {
                _isDragging = false;
                _dragStartScreen = GetGlobalMousePosition();
                _dragCurrentScreen = _dragStartScreen;
                _dragStartWorld = PositionsManager.Instance.positionMouseCamera;
                _dragCurrentWorld = _dragStartWorld;
            }
            else
            {
                // Al soltar el clic izquierdo
                Vector2 mouseUpScreen = GetGlobalMousePosition();
                float distanceMoved = _dragStartScreen.DistanceTo(mouseUpScreen);

                if (_isDragging || distanceMoved > DragThreshold)
                {
                    // Fue un arrastre: Seleccionar unidades en el rectángulo del mundo
                    Rect2 worldRect = CreateWorldRect(_dragStartWorld, _dragCurrentWorld);
                    _selectionManager.SelectUnitsInRect(worldRect);
                }
                else
                {
                    // Fue un clic simple individual
                    _selectionManager.SelectUnitAtPoint(PositionsManager.Instance.positionMouseCamera);
                }

                _isDragging = false;
                QueueRedraw(); // Limpia el cuadro visual de la pantalla
            }
            return;
        }

        // 3. Movimiento del Ratón mientras se mantiene presionado el clic izquierdo
        if (@event is InputEventMouseMotion && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (_dragStartScreen.DistanceTo(GetGlobalMousePosition()) > DragThreshold)
            {
                _isDragging = true;
                _dragCurrentScreen = GetGlobalMousePosition();
                _dragCurrentWorld = PositionsManager.Instance.positionMouseCamera;

                QueueRedraw(); // Fuerza a redibujar el cuadro verde en pantalla
            }
        }
    }

    public override void _Draw()
    {
        if (!_isDragging) return;

        // Dibujamos el rectángulo visual en coordenadas de pantalla
        Rect2 screenRect = CreateScreenRect(_dragStartScreen, _dragCurrentScreen);

        Color fillColor = new Color(0, 1, 0, 0.15f);   // Verde translúcido
        Color borderColor = new Color(0, 1, 0, 0.8f);  // Borde verde sólido

        DrawRect(screenRect, fillColor, true);
        DrawRect(screenRect, borderColor, false, 2.0f);
    }

    private Rect2 CreateScreenRect(Vector2 pos1, Vector2 pos2)
    {
        Vector2 min = new Vector2(Mathf.Min(pos1.X, pos2.X), Mathf.Min(pos1.Y, pos2.Y));
        Vector2 size = new Vector2(Mathf.Abs(pos1.X - pos2.X), Mathf.Abs(pos1.Y - pos2.Y));
        return new Rect2(min, size);
    }

    private Rect2 CreateWorldRect(Vector2 pos1, Vector2 pos2)
    {
        Vector2 min = new Vector2(Mathf.Min(pos1.X, pos2.X), Mathf.Min(pos1.Y, pos2.Y));
        Vector2 size = new Vector2(Mathf.Abs(pos1.X - pos2.X), Mathf.Abs(pos1.Y - pos2.Y));
        return new Rect2(min, size);
    }

    public void SwitchMode(WorldControlMode newMode, string buildingId = null)
    {
        _currentMode = newMode;
        if (newMode == WorldControlMode.BuildingPlacement && buildingId != null)
        {
            _buildingPlacementManager.StartPlacement(buildingId);
        }
        else
        {
            _buildingPlacementManager.CancelPlacement();
        }
    }
}