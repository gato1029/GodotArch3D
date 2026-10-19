using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.GameRuntime.Internal;

public struct WorldSlot
{
    public string WorldName; // se generara de forma aleatoria, pero se puede cambiar

    public long ProfileId;
    public int InternalSeed;
}

public class GameSaveData 
{
    // aqui empieza el juego es decir esto es el inicio de la partida
    public string SaveName;
    public int MasterSeed;
    public List<WorldSlot> VisitedWorlds;
    public WorldSlot CurrentWorld;
}
