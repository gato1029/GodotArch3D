using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Materials
{
    internal class MaterialData
    {
        [BsonId]
        public int idMaterial { get; set; }
        public bool isDinamic { get; set; }
        public string pathTexture { get; set; }
        public int widhtTexture { get; set; }
        public int heightTexture { get; set; }
        public int divisionPixelX { get; set; }
        public int divisionPixelY { get; set; }
    }
}
