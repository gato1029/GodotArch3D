using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Biomas;

public class BiomaData:IdDataLong
{
    public long idTerreno {  get; set; }
    public long idSuperficie { get; set; }
    public List<ResourceEntry> idRecursos { get; set; }
    public long idCamino { get; set; }
    public List<long> idDecoracion { get; set; }
    public BiomaData()
    {
        id = EpochIdGenerator.NewId();
    }
}
