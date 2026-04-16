using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.KuroTiles;

public enum NeighborPosition
{
    Arriba = 0,
    Derecha = 1,
    Abajo = 2,
    Izquierda = 3,
    ArribaDerecha = 4,
    AbajoDerecha = 5,
    AbajoIzquierda = 6,
    ArribaIzquierda = 7,
    DebajoCentro = 8,
}

public enum NeighborType
{    
    VacioAlgo = 0, // No interesa el contenido lleno o vacio de cualquier grupo
    LlenoMismoGrupo = 1, // Lleno con cualquier tile del mismo grupo            
    NoLlenoMismoGrupo = 2, // No hay tile  del mismo grupo
    AlgoCualquierGrupo = 3, // Cualquier tile de cualquier grupo
    NoLlenoCualquierGrupo = 4, // No hay tile  de cualquier grupo 
    LLenoConTileEspecifico = 5, // Lleno con un tile específico
}
public enum UnderNeighborType
{
    NoHacerNada= 0,
    Ocultar = 1,
    Eliminar = 2,
}
public class TileTemplate
{
    public int idGroup { get; set; }
    public long idTileSprite { get; set; }

    public TileTemplate(int idGroup, long idTileSprite)
    {
        this.idGroup = idGroup;
        this.idTileSprite = idTileSprite;
    }

    public TileTemplate()
    {
    }
}

public class TileRuleTemplate
{
    public TileTemplate TileCentral { get; set; }
    public bool IsRandomTiles { get; set; }
    public List<TileTemplate> RandomTiles { get; set; } = new();

    // Array de 8 posiciones (una por dirección)
    public NeighborConditionTemplate[] neighborConditionTemplate { get; private set; } = new NeighborConditionTemplate[8];

    public NeighborConditionTemplate neighborConditionTemplateCenter {  get; set; } = new NeighborConditionTemplate();
    

    public int Bitmask { get; private set; }

    public NeighborConditionTemplate GetNeigbordCondition(NeighborPosition neighborPosition)
    {
        return neighborConditionTemplate[(int)neighborPosition];
    }
    public TileRuleTemplate()
    {
        // Inicializar cada posición con condición vacía por defecto
        for (int i = 0; i < neighborConditionTemplate.Length; i++)

            neighborConditionTemplate[i] = new NeighborConditionTemplate
            {
                position = (NeighborPosition)i,
                type = NeighborType.VacioAlgo
            };
    }

    public void CalculateBitmask()
    {
        int mask = 0;
        for (int i = 0; i < neighborConditionTemplate.Length; i++)
        {
            var cond = neighborConditionTemplate[i];           
                mask |= 1 << i;
        }
        Bitmask = mask;
    }

    public bool MatchesEnvironment(TileEnvironment environment)
    {
        if (neighborConditionTemplateCenter.Match(environment.GetUnderCenter().TileId, environment.GetUnderCenter().GroupId))
        {
            foreach (var cond in neighborConditionTemplate)
            {
                var neighbor = environment.Get(cond.position);
                //if (neighbor == null)
                //    continue; // si el entorno no tiene dato, se ignora (opcional)

                if (!cond.Match(neighbor.Value.TileId, neighbor.Value.GroupId))
                    return false;
            }
            return true;
        }
        return false;
    }
}
public class NeighborConditionTemplate
{
    public NeighborPosition position { get; set; }  // Dirección del vecino
    public NeighborType type { get; set; }          // Tipo de condición
    public long idTile { get; set; }               // Tile específico, si aplica
    public int groupId { get; set; }               // Grupo del tile central
    public UnderNeighborType UnderNeighborType { get; set; } = UnderNeighborType.NoHacerNada;
    public bool Match(long neighborTileId=0, int neighborGroupId=0)
    {
        switch (type)
        {
            case NeighborType.VacioAlgo:
                // No importa si hay o no hay tile
                return true;

            case NeighborType.LlenoMismoGrupo:
                // Debe haber tile y ser del mismo grupo
                return neighborTileId != 0 && neighborGroupId == groupId;

            case NeighborType.NoLlenoMismoGrupo:
                // No hay tile del mismo grupo (vacío o de otro grupo)
                return neighborTileId == 0 || neighborGroupId != groupId;

            case NeighborType.AlgoCualquierGrupo:
                // Debe existir tile (de cualquier grupo)
                return neighborTileId != 0;

            case NeighborType.NoLlenoCualquierGrupo:
                // No hay tile en absoluto
                return neighborTileId == 0;

            case NeighborType.LLenoConTileEspecifico:
                // Debe existir y coincidir con tile específico
                return neighborTileId == idTile;

            default:
                return false;
        }
    }
}
public class AutoTileTemplate
{
    public int id { get; set; }
    public string name { get; set; }
    public List<TileRuleTemplate> tileRuleTemplates { get; set; }
}

