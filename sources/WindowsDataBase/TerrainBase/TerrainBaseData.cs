using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.TerrainBase;
public class TerrainBaseData: IdDataLong
{
    public ushort idDualTemplate { get; set; }        
    public int materialTemplate { get; set; }
}

public class SurfaceBaseData : IdDataLong
{
    public ushort idDuaTemplate { get; set; }

    // luego agregar costo materiales etc
    //public int costo {  get; set; }

}
