using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.managers;

namespace GodotEcsArch.sources.GameRuntime.Internal.RTSControler;

public class BuildingPlacementManager
{
    private readonly BlackyWorld _world;
    private bool _isPlacing = false;
    private string _activeBuildingTemplateId;
    private Node2D _ghostNode; // Nodo visual temporal (fantasma)

    public BuildingPlacementManager(BlackyWorld world)
    {
        _world = world;
    }

    public void StartPlacement(string buildingTemplateId)
    {
        _activeBuildingTemplateId = buildingTemplateId;
        _isPlacing = true;
        // Instanciar o activar el nodo fantasma visual en la escena de Godot
        GD.Print($"Iniciando modo construcción para: {buildingTemplateId}");
    }

    public void CancelPlacement()
    {
        _isPlacing = false;
        _activeBuildingTemplateId = null;
        // Ocultar o destruir nodo fantasma
    }

    public void HandleInput(InputEvent @event, WorldControlManager controller)
    {
        if (!_isPlacing) return;

        if (@event is InputEventMouseButton mb && mb.Pressed)
        {
            if (mb.ButtonIndex == MouseButton.Left)
            {
                // Validar celdas con el grid estático y confirmar colocación
                TryConfirmPlacement();
                controller.SwitchMode(WorldControlMode.DefaultOrSelecting);
            }
            else if (mb.ButtonIndex == MouseButton.Right)
            {
                CancelPlacement();
                controller.SwitchMode(WorldControlMode.DefaultOrSelecting);
            }
        }
    }

    private void TryConfirmPlacement()
    {
        Vector2 mouseWorldPos = PositionsManager.Instance.positionMouseCamera;
        // Consultar al StaticSpatialChunkGrid si el área está libre, descontar recursos y crear entidad en Flecs
        GD.Print($"Edificio {_activeBuildingTemplateId} colocado en {mouseWorldPos}");
    }
}