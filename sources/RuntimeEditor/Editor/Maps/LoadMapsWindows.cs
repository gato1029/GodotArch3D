using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.utils;
using System;

public partial class LoadMapsWindows : Window
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		LoadMaps();
        KuroItemsListMaps.OnDataSelected += KuroItemsListMaps_OnDataSelected;
        KuroButtonCargar.Pressed += KuroButtonCargar_Pressed;
	}
    SavedMapData infoMap;
    private void KuroButtonCargar_Pressed()
    {
        if (BlackyWorldContext.World!=null)
        {
            BlackyWorldContext.World.Dispose(); // limpiar y libera el mundo actual
        }
        
        BlackyWorld blackyWorld = new BlackyWorld(infoMap.Name, infoMap.MapType, infoMap.ChunkSize, infoMap.HeightCount, infoMap.seed, new Vector2I(infoMap.TileSizeX, infoMap.TileSizeY),true);
        QueueFree();
    }

    private void KuroItemsListMaps_OnDataSelected(object obj)
    {
        infoMap = (SavedMapData)obj;
    }

    private void LoadMaps()
	{
		string ruta = CommonAtributes.RutaGuardadoMapas;
        // Verificar si la ruta existe para evitar errores
        if (System.IO.Directory.Exists(ruta))
        {
            // Obtiene las rutas completas de todas las carpetas dentro de la ruta principal
            string[] carpetas = System.IO.Directory.GetDirectories(ruta);

            foreach (string carpeta in carpetas)
            {
                // Extraer solo el nombre de la carpeta (sin la ruta completa)
                string nombreMapa = System.IO.Path.GetFileName(carpeta);

                var infodata = CommonAtributes.LoadInfoMap(nombreMapa, GodotEcsArch.sources.BlackyEngine.Data.SaveFormat.Json);
                // Agregar a tu lista (asumiendo que el segundo parámetro es el dato asociado, como la ruta completa)
                KuroItemsListMaps.AddItemWithData(nombreMapa, infodata);
            }
        }
   
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
