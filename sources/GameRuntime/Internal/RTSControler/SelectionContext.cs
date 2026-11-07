using System;
using System.Collections.Generic;
using Flecs.NET.Core;

namespace GodotEcsArch.sources.GameRuntime.Internal.RTSControler;

public static class SelectionContext
{
    private static readonly List<Entity> _currentSelection = new();
    public static IReadOnlyList<Entity> CurrentSelectedEntities => _currentSelection;

    // Evento para notificar a la UI (HUD)
    public static event Action<IReadOnlyList<Entity>> OnSelectionChanged;

    public static void SetSelection(IEnumerable<Entity> entities)
    {
        _currentSelection.Clear();
        foreach (var entity in entities)
        {
            if (entity.IsAlive())
            {
                _currentSelection.Add(entity);
            }
        }
        
        OnSelectionChanged?.Invoke(_currentSelection);
    }

    public static void Clear()
    {
        _currentSelection.Clear();
        OnSelectionChanged?.Invoke(_currentSelection);
    }
}