using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.TerrenoBase;
public class TerrainBase: IdDataLong
{
    public ushort idDualTemplate { get; set; }
        
}

public class SurfaceBase : IdDataLong
{
    public ushort idDuaTemplate { get; set; }
    //public int costo {  get; set; }

}
