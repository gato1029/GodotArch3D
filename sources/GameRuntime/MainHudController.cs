using System.Collections.Generic;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.GameRuntime.Internal.RTSControler;
using GodotFlecs.sources.Flecs.Components;

public partial class MainHudController : Control
{
	private VBoxContainer _buildingPanel;
	private VBoxContainer _unitPanel;

	public override void _Ready()
	{
		InitializeUI(); // Insertado por el generador de UI
		// Nos suscribimos al evento del SelectionContext
		SelectionContext.OnSelectionChanged += HandleSelectionChanged;
	}

	public override void _ExitTree()
	{
		// Limpieza de eventos al salir
		SelectionContext.OnSelectionChanged -= HandleSelectionChanged;
	}

	private void HandleSelectionChanged(IReadOnlyList<Entity> selectedEntities)
	{
		// Ocultar todos los paneles por defecto
		// _buildingPanel.Hide();
		// _unitPanel.Hide();

		if (selectedEntities.Count == 0) return;

		if (selectedEntities.Count == 1)
		{
			Entity target = selectedEntities[0];

			if (target.Has<BuildingDefinitionComponent>())
			{
				// Mostrar panel del edificio y sus opciones de producción o construcción
				// _buildingPanel.Show();
			}
			else if (target.Has<UnitDefinitionComponent>())
			{
				// Mostrar estadísticas de la unidad seleccionada
				// _unitPanel.Show();
			}
		}
		else
		{
			// Selección múltiple: mostrar cuadrícula de unidades
		}
	}

	// Método que llaman los botones de la UI de Godot para iniciar una construcción
	public void OnBuildButtonClicked(string buildingTemplateId)
	{
		// Buscamos el WorldControlManager en la escena y cambiamos al modo de construcción
		var worldController = GetTree().Root.GetNode<WorldControlManager>("WorldController");
		worldController.SwitchMode(WorldControlMode.BuildingPlacement, buildingTemplateId);
	}
}