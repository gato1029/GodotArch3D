using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System.Collections.Generic;
using System.Linq;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.GameRuntime.Internal.TimerGame;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Chunks;

namespace GodotEcsArch.sources.GameRuntime.Internal;

// aquí se controla toda la partida

public class GameSession
{
    private readonly string _name; // nombre de la partida
    private int _masterSeed;
    private readonly List<WorldSlot> VisitedWorlds;
    public BlackyWorld CurrentWorld { get; private set; }
    private readonly BlackyTimerGame _timer = new BlackyTimerGame();
    
    public GameSession(string name, int masterSeed = 0,bool isNewGame=true)
    {
        this._name = name;
        VisitedWorlds = new List<WorldSlot>();
        this._masterSeed = masterSeed;
        // 1 segundo real = 1 minutos de juego
        _timer.MinutesPerSecond = 1.0;
        _timer.TimeScale = 1.0;
        
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
        _masterSeed = 759245143;
        Vector2I sizeMap = new Vector2I(2048,2048);//  GenerateRandomSizeMap();
        CurrentWorld = new BlackyWorld(_name, perfilMundo.tipoMundo, 32, alturaMaxima, _masterSeed,sizeMap,perfilMundo.id);
                
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

    public void Update(double delta)
    {
        _timer.Update(delta);
        RenderCommandQueue.ExecuteFrame();
        ChunkManager.Instance.UpdatePlayerPosition(PositionsManager.Instance.positionCamera);

        var worlds = BlackyWorldRegistry.Instance.GetAllWorlds();

        foreach (var item in worlds)
        {
            item.Update((float)delta);
        }
    }
}