using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
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
        public bool haveCollider { get; set; }
        public GeometricShape2D collisionBody { get; set; }
        public int idMaterial { get; set; }   
        public string type { get; set; }    
        public float scale { get; set; }
        public float offsetX { get; set; }
        public float offsetY { get; set; }

        public bool mirrorX { get; set; }
        public bool mirrorY { get; set; }

        public string colorString { get; set; } // Color en formato hexadecimal, por ejemplo: "#FF0000" para rojo
        [BsonIgnore]
        public Vector2 offsetInternal{ get;  set; }
        [BsonIgnore]
        public Color color { get; set; }
    }
    public class TileSimpleData : TileData
    {
        public int idInternalPosition { get; set; }

        public TileSimpleData()
        {           
            type = nameof(TileSimpleData);         
        }
        [BsonCtor]
        public TileSimpleData(int idMaterial, int idInternalPosition, float offsetX, float offsetY) :base()
        {
            offsetInternal = new Godot.Vector2(MeshCreator.PixelsToUnits(offsetX), MeshCreator.PixelsToUnits(offsetY));
            textureVisual = MaterialManager.Instance.GetAtlasTexture(idMaterial, idInternalPosition);
          
        }        
    }
    public class TileDynamicData : TileData
    {
        FrameData[] aleatoryTile { get; set; }
        bool haveAleatory { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float xFormat { get; set; }
        public float yFormat { get; set; }
        public float widht { get; set; }
        public float height { get; set; }
        public float widhtFormat { get; set; }
        public float heightFormat { get; set; }
        public TileDynamicData()
        {
            type = nameof(TileDynamicData);
        
        }
        [BsonCtor]
        public TileDynamicData(int idMaterial, float xFormat, float yFormat, float widhtFormat, float heightFormat, float offsetX, float offsetY,string colorString) :base()
        {
            offsetInternal = new Godot.Vector2(MeshCreator.PixelsToUnits(offsetX), MeshCreator.PixelsToUnits(offsetY));
            textureVisual = MaterialManager.Instance.GetAtlasTexture(idMaterial, xFormat, yFormat, widhtFormat, heightFormat);
            if (colorString != null)
            { 
            var components = colorString.Trim('(', ')')
                       .Split(',')
                       .Select(s => float.Parse(s.Trim()))
                       .ToArray();            
            color = new Color(components[0], components[1], components[2], components[3]);
            }
        }
    }
    public class TileAnimateData : TileData
    {       
        public FrameData[] framesArray { get; set; }
        public float frameDuration { get; set; } // Duración de cada frame
        public TileAnimateData()
        {
            type = nameof(TileAnimateData);
        }

        [BsonCtor]
        public TileAnimateData(int idMaterial, FrameData[] framesArray, float offsetX, float offsetY, string colorString) : base()
        {
            offsetInternal = new Godot.Vector2(MeshCreator.PixelsToUnits(offsetX), MeshCreator.PixelsToUnits(offsetY));
            if (framesArray != null && framesArray.Length>0)
            {
                textureVisual = MaterialManager.Instance.GetAtlasTexture(idMaterial, framesArray[0].x, framesArray[0].y, framesArray[0].widhtFormat, framesArray[0].heightFormat);
            }
            if (colorString != null)
            {
                var components = colorString.Trim('(', ')')
                       .Split(',')
                       .Select(s => float.Parse(s.Trim()))
                       .ToArray();
                color = new Color(components[0], components[1], components[2], components[3]);
            }
        }
    }

  
 
}
