using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Maps;

public enum EditorMode
{
    APAGADO,
    TERRENO,
    RECURSOS,
    UNIDADES
}
public class MapManagerEditor : SingletonBase<MapManagerEditor>
{ 
    // 🔒 Evento solo invocable desde dentro de esta clase
    public event Action<MapLevelData> OnMapLevelDataChanged;
    private MapLevelData currentMapLevelData;
    public EditorMode editorMode { get; set; }
    public bool enableEditor { get; set; }

    public MapLevelData CurrentMapLevelData
    {
        get => currentMapLevelData;
        set
        {
            if (currentMapLevelData != value)
            {
                if (currentMapLevelData!=null)
                {
                    currentMapLevelData.ClearAll();
                }                
                currentMapLevelData = value;
                OnMapLevelDataChanged?.Invoke(currentMapLevelData);
            }
        }
    }
    protected override void Initialize()
    {
 
    }

    protected override void Destroy()
    {
 
    }
}
