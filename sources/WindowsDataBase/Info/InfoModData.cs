using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Info;

public class InfoModData:IdData
{        
    public string Version { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
}
