using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.TilesTexture;

// solo guardaremos aqui si el tile tiene un collider y si el tile es animado,
// si no es un tile visual y no tiene collider o no es animado, no se guardara
// como plantilla, puesto que seria insulso crear todos los tiles si la mayoria son solo visuales
// y con el index resolvemos todo,
public class TileTextureData: IdDataLong
{
    public string idMod {  get; set; } // de aqui podemos buscar la textura y su offset en el atlas
    public int idMaterial { get; set; }
    public int index {  get; set; }
    public bool isAnimated { get; set; }
    
    public int[] indexAnimation { get; set; }
    public bool hasCollider { get; set; }

    // informacion en unidades de mundo
    public float fps {  get; set; }
    public FastCollider fastCollider {  get; set; }

    // informacion en pixeles
    public float fpsTemplate { get; set; }
    public FastCollider fastColliderTemplate { get; set; }

    public TileTextureData()
    {
        ReGerenateId();
    }


}
