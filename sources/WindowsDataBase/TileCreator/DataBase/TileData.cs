using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase
{
    public class TileData:IdData
    {        
        public int idMaterial { get; set; }   
        public string type { get; set; }    
    }
    public class TileSimpleData : TileData
    {
        public int idInternalPosition { get; set; }

        public TileSimpleData()
        {           
            type = nameof(TileSimpleData);         
        }
        [BsonCtor]
        public TileSimpleData(int idMaterial, int idInternalPosition):base()
        {
           textureVisual = MaterialManager.Instance.GetAtlasTexture(idMaterial, idInternalPosition);
        }        
    }
    public class TileDynamicData : TileData
    {
        public float x { get; set; }
        public float y { get; set; }
        public float widht { get; set; }
        public float height { get; set; }
        public TileDynamicData()
        {
            type = nameof(TileDynamicData);
        }
    }
    public class TileAnimateData : TileData
    {
        public int[] idFrames { get; set; } // Cantidad de frames en la animación
        public float frameDuration { get; set; } // Duración de cada frame
        public TileAnimateData()
        {
            type = nameof(TileAnimateData);
        }

        [BsonCtor]
        public TileAnimateData(int idMaterial, int idInternalPosition) : base()
        {
            textureVisual = MaterialManager.Instance.GetAtlasTexture(idMaterial, idInternalPosition);
        }
    }

  
 
}
