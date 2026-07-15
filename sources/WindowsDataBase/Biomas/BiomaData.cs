using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Biomas;

public class BiomaData:IdDataLong
{
    public long idTerreno;
    public long idSuperficie;
    public List<long> idRecursos;
    public long idCamino;
    public List<long> idDecoracion;
}
