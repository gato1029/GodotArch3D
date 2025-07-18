using Godot;
using GodotEcsArch.sources.utils;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Materials
{
    public enum MaterialType
    {
        GENERICO = 0,
        ARMAS = 1,
    }
    public class IdData 
    {
        [BsonId]
        public int id { get; set; }
        public string name { get; set; }
        [BsonIgnore]
        public AtlasTexture textureVisual { get; set; }

    }

    public class MaterialSimpleData:IdData
    {
        public int type { get; set; }
        public string pathTexture { get; set; }
        public int widhtTexture { get; set; }
        public int heightTexture { get; set; }
        public int divisionPixelX { get; set; }
        public int divisionPixelY { get; set; }
        public MaterialSimpleData()
        {
         
        }
    }
    public class MaterialData : IdData
    {                
        public int type { get; set; }
        public string pathTexture { get; set; }
        public int widhtTexture { get; set; }
        public int heightTexture { get; set; }
        public int divisionPixelX { get; set; }
        public int divisionPixelY { get; set; }

        [BsonIgnore]
        public ShaderMaterial shaderMaterial { get; set; }
        [BsonIgnore]
        public Texture textureMaterial { get; set; }
        [BsonIgnore]        
        public Mesh mesh { get; set; }

        [BsonIgnore]
        public int idMaterialPositionBatch { get; set; }
        public MaterialData()
        {
            MaterialManager.Instance.RegisterMaterial(-1, this);            
        }

        [BsonCtor]
        public MaterialData(int type, string pathTexture, int widhtTexture, int heightTexture, int divisionPixelX, int divisionPixelY,int id):base()
        {
            this.type = type;
            this.pathTexture = pathTexture;
            this.widhtTexture = widhtTexture;
            this.heightTexture = heightTexture;
            this.divisionPixelX = divisionPixelX;
            this.divisionPixelY = divisionPixelY;

            
            ShaderMaterial materialGeneric = null;
            Texture2D texture2D = null;
            float widht;
            float height;
            switch (type)
            {
                case 0:
                    MaterialManager.Instance.RegisterMaterial(id, this);
                    texture2D = (Texture2D)TextureHelper.LoadTextureLocal(FileHelper.GetPathGameDB(pathTexture));
                    break;
                case 1:
                    materialGeneric = GD.Load<ShaderMaterial>("res://resources/Material/Sprite3DMultimeshGeneric.tres");
                    texture2D = (Texture2D)TextureHelper.LoadTextureLocal(FileHelper.GetPathGameDB(pathTexture));
                    textureMaterial = texture2D;
                    widht = texture2D.GetWidth();
                    height = texture2D.GetHeight();

                    shaderMaterial = (ShaderMaterial)materialGeneric.Duplicate();
                    shaderMaterial.SetShaderParameter("main_texture", textureMaterial);
                    shaderMaterial.SetShaderParameter("atlas_width", widht);
                    shaderMaterial.SetShaderParameter("atlas_height", height);
                    mesh = MeshCreator.CreateSquareMesh(16, 16);
                    mesh.SurfaceSetMaterial(0, shaderMaterial);
                    MaterialManager.Instance.RegisterMaterial(id, this);
                    break;                                 
                default:
                    break;
            }

            if (texture2D !=null)
            {
                AtlasTexture atlasTexture = new AtlasTexture();
                atlasTexture.Atlas = texture2D;
                textureVisual = atlasTexture;
            }
            


        }

    }
}
