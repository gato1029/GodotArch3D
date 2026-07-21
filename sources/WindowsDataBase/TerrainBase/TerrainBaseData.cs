using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.TerrainBase;
public class TerrainBaseData: IdDataLong
{   
    public long idDualTemplate { get; set; } // de la clase dualtileTemplate

    public TerrainBaseData()
    {
        id = EpochIdGenerator.NewId();
    }
}

