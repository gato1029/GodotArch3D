using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Multimesh;
public class MultimeshManager:SingletonBase<MultimeshManager>
{
    Dictionary<int, MultimeshMaterial> multimeshMaterialDict;
    protected override void Initialize()
    {
        multimeshMaterialDict = new Dictionary<int, MultimeshMaterial>();
    }

    protected override void Destroy()
    {
        
    }
}
