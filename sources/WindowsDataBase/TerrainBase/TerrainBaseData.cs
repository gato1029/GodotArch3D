using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.TerrainBase;
public class TerrainBaseData: IdDataLong
{
    // aqui se guard todo en base el idSAVE
    public long idDualTemplate { get; set; } // de la clase dualtileTemplate
    //public List<long> rampas { get; set; } = new(); // de la clase TileSpriteData, para las rampas que se pueden poner sobre este terreno
}

