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
    public class IdData
    {
        [BsonId]
        public int id { get; set; }
        public string name { get; set; }
        [BsonIgnore]
        public AtlasTexture textureVisual { get; set; }

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

        public MaterialData()
        {
        }

        [BsonCtor]
        public MaterialData(int type, string pathTexture, int widhtTexture, int heightTexture, int divisionPixelX, int divisionPixelY)
        {
            switch (type)
            {
                case 0:
                    ShaderMaterial materialGeneric = GD.Load<ShaderMaterial>("res://resources/Material/Sprite3DMultimesh.tres");
                    Texture2D texture2D = (Texture2D)TextureHelper.LoadTextureLocal(FileHelper.GetPathGameDB(pathTexture));
                    float widht = texture2D.GetWidth() / divisionPixelX;
                    float height = texture2D.GetHeight() / divisionPixelY;

                    ShaderMaterial shaderMaterial = (ShaderMaterial)materialGeneric.Duplicate();
                    shaderMaterial.SetShaderParameter("mtexture", texture2D);
                    shaderMaterial.SetShaderParameter("ancho", widht);
                    shaderMaterial.SetShaderParameter("alto", height);

                    textureMaterial = texture2D;
                break;

                default:
                    break;
            }
            
        }
   
    }
}
