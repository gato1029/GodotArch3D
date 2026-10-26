using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System.Collections.Generic;
using System.Linq;

namespace GodotEcsArch.sources.GameRuntime.Internal;

public class GameRuntime
{
    public string Name; // nombre de la partida
    public int MasterSeed;
    public List<WorldSlot> VisitedWorlds;    
    BlackyWorld CurrentWorld; // el mundo actual en el que se encuentra el jugador

    public GameRuntime(string name, int masterSeed = 0,bool isNewGame=true)
    {
        this.Name = name;
        this.MasterSeed = masterSeed;
        if (isNewGame)
        {
            GenerateNewWorld();
        }
        else
        {
            // Cargar el mundo actual desde los datos guardados por el nombre
        }

    }

    private void GenerateNewWorld()
    {
        if (BlackyWorldContext.World != null)
        {
            BlackyWorldContext.World.Dispose(); // limpiar y libera el mundo actual
        }
        var perfilMundo = GetProfileWorld();
        int alturaMaxima = 5;
        MasterSeed = 759245143;
        Vector2I sizeMap = new Vector2I(4096,4096);//  GenerateRandomSizeMap();
        CurrentWorld = new BlackyWorld(Name, perfilMundo.tipoMundo, 32, alturaMaxima, MasterSeed,sizeMap,perfilMundo.id);
                
    }

    private Vector2I GenerateRandomSizeMap()
    {
        int minSize = 256;
        int maxSize = 4096; //4096
        int step = 32;

        int width = GD.RandRange(minSize / step, maxSize / step) * step;
        int height = GD.RandRange(minSize / step, maxSize / step) * step;

        return new Vector2I(width, height);
    }
    /// <summary>
    /// Devuelve un perfil de mundo aleatorio de la paleta de perfiles de mundo disponibles.    
    /// </summary>
    /// <returns></returns>
    private TemplateWorldProfileData GetProfileWorld()
    {
        var lista = BlackyPalletesPersistence.templateWorldProfilePallete.GetAllData();
        var perfilesValidos = lista
            .Where(p => p != null && p.tipoMundo == BlackyWorldTypeDetail.MUNDO)
            .ToList();

        if (perfilesValidos.Count == 0)
        {
            GD.PrintErr("No hay perfiles de mundo válidos disponibles.");
            return null;
        }

        int index = GD.RandRange(0, perfilesValidos.Count - 1);
        return perfilesValidos[index];
    }

    public void AddVisitedWorld(long profileId, int internalSeed)
    {
        VisitedWorlds.Add(new WorldSlot { WorldName = $"World_{internalSeed}", ProfileId = profileId, InternalSeed = internalSeed });
    }

}