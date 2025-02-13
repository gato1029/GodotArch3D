using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

namespace GodotEcsArch.sources.managers.Tilemap
{
    internal class TileNumeratorManager:SingletonBase<TileNumeratorManager>
    {
        public int tileNumerator;

        public int getNumerator()
        {
            tileNumerator++;
            return tileNumerator;
        }

        protected override void Initialize()
        {
            tileNumerator = 0;
        }       

        protected override void Destroy()
        {
            throw new NotImplementedException();
        }

   
    }
}
