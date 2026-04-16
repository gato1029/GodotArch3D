using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Accesories;
internal class AccesoryAvatarManager : SingletonBase<AccesoryAvatarManager>
{
    Dictionary<AccesoryAvatarType, MultimeshMaterial> multimeshMaterialDict;
    Dictionary<AccesoryAvatarType, int> accesoryDict;
    Dictionary<AccesoryAvatarType, GpuInstance> instanceDict;
    protected override void Initialize()
    {
        multimeshMaterialDict = new Dictionary<AccesoryAvatarType, MultimeshMaterial>();
        accesoryDict = new Dictionary<AccesoryAvatarType, int>();
        instanceDict = new Dictionary<AccesoryAvatarType, GpuInstance>();
    }
    public (SpriteAnimationDirection, long) RegisterAccesory(AccesoryAvatarType accesoryAvatarType, int idAccesory)
    {
        if (!multimeshMaterialDict.ContainsKey(accesoryAvatarType))
        {
            var data = MasterDataManager.GetData<AccessoryData>(idAccesory); // AccesoryManager.Instance.GetAccesory(idAccesory);
            var tileSpriteData = MasterDataManager.GetData<TileSpriteData>(data.idTileSpriteData);
            var sp = tileSpriteData.spriteMultipleAnimationDirection.animationsTypes[AnimationType.ARMA_ATACANDO];
            var spriteData = sp.animations[AnimationDirection.LEFT];
            MultimeshMaterial multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(spriteData.idMaterial),1);            
            multimeshMaterialDict.Add(accesoryAvatarType, multimeshMaterial);
            accesoryDict.Add(accesoryAvatarType,idAccesory);
            var instance = multimeshMaterial.CreateInstance();
            GpuInstance gpuInstance;
            gpuInstance.rid = instance.Item1;
            gpuInstance.instance = instance.Item2;
            instanceDict.Add(accesoryAvatarType, gpuInstance);
            return (sp,tileSpriteData.id);
        }
        return (null,0);

    }

    public (GpuInstance gpu, long idTileSprite, WeaponComponent weaponComponent) ChangueAccesory(AccesoryAvatarType accesoryAvatarType, int idAccesory, float scale = 1)
    {
        SpriteAnimationDirection sp = null;
        long idTile = 0;
        if (!multimeshMaterialDict.ContainsKey(accesoryAvatarType))
        {
           var rp= RegisterAccesory(accesoryAvatarType, idAccesory);
            sp = rp.Item1;
            idTile = rp.Item2;
        }
        else
        {
            if (accesoryDict[accesoryAvatarType] != idAccesory)
            {
                accesoryDict[accesoryAvatarType] = idAccesory;

                var data = MasterDataManager.GetData<AccessoryData>(idAccesory);
                var tileSpriteData = MasterDataManager.GetData<TileSpriteData>(data.idTileSpriteData);
                sp = tileSpriteData.spriteMultipleAnimationDirection.animationsTypes[AnimationType.ARMA_ATACANDO];
                var spriteData = sp.animations[AnimationDirection.LEFT];
                var dataMaterial = DataBaseManager.Instance.FindById<MaterialSimpleData>(spriteData.idMaterial);
                idTile = tileSpriteData.id;
                MultimeshMaterial dataMultimesh = multimeshMaterialDict[accesoryAvatarType];
                Texture2D texture2D = (Texture2D)TextureHelper.LoadTextureLocal(FileHelper.GetPathGameDB(dataMaterial.pathTexture));
                dataMultimesh.materialData.textureMaterial = texture2D;


                dataMultimesh.materialData.shaderMaterial.SetShaderParameter("main_texture", dataMultimesh.materialData.textureMaterial);
                dataMultimesh.materialData.shaderMaterial.SetShaderParameter("atlas_width", texture2D.GetWidth());
                dataMultimesh.materialData.shaderMaterial.SetShaderParameter("atlas_height", texture2D.GetHeight());  
            }
        }
        WeaponComponent weaponComponent = new WeaponComponent();
        weaponComponent.idWeapon = idAccesory;
        weaponComponent.isRanged = false;
        weaponComponent.collidersDirection = new Dictionary<AnimationDirection, Collision.GeometricShape2D>();
        foreach (var item in sp.animations)
        {
           var collider =  item.Value.collisionBodyArray[0].Multiplicity(scale);
           weaponComponent.collidersDirection.Add(item.Key, collider);
        }

        //
        return (instanceDict[accesoryAvatarType],idTile,weaponComponent);
    }
}
