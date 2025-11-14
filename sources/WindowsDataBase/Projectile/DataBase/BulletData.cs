using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Projectile.DataBase;
public class BulletData:IdData
{
    public SpriteData spriteData { get; set; }

    [BsonCtor]
    public BulletData(SpriteData spriteData)
    {
        if (spriteData != null)
        {
            textureVisual = MaterialManager.Instance.GetAtlasTextureInternal(spriteData.idMaterial, spriteData.x, spriteData.y, spriteData.widht, spriteData.height);
        }
    }

    public BulletData()
    {
    }
}
