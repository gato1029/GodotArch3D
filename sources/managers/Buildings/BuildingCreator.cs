using Arch.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Buildings;
internal class BuildingCreator:SingletonBase<BuildingCreator>
{
    public Entity CreateBuilding(int idBuilding)
    {
        Entity entity = EcsManager.Instance.World.Create();

        return entity;
    }
}
