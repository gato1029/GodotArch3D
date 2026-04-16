using GodotEcsArch.sources.utils;
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
    public long idTileSprite {  get; set; } 
    [BsonCtor]
    public BulletData(long idTileSprite)
    {
        textureVisual = MasterDataManager.GetData<TileSpriteData>(idTileSprite).textureVisual;
    }

    public override void RefreshTextureVisual()
    {
        textureVisual = MasterDataManager.GetData<TileSpriteData>(idTileSprite).textureVisual;
    }
    public BulletData()
    {
    }
}
