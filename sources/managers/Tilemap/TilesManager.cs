using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;
namespace GodotEcsArch.sources.managers.Tilemap;
internal class TilesManager : SingletonBase<TilesManager>
{
    public Dictionary<int, TileData> tilesDictionary;
    protected override void Initialize()
    {
        tilesDictionary = new Dictionary<int, TileData>();
    }

    public void RegisterTileData(TileData tileData)
    {
        if (!tilesDictionary.ContainsKey(tileData.id))
        {
            tilesDictionary.Add(tileData.id, tileData);
        }
    }
    public TileData RegisterTileData(int idTileData)
    {
        if (idTileData ==0)
        {
            return null;
        }
        TileData tileData = null;
        if (!tilesDictionary.ContainsKey(idTileData))
        {
            
            tileData = DataBaseManager.Instance.FindById<TileData>(idTileData);
            var mat = MaterialManager.Instance.GetMaterial(tileData.idMaterial);        
            if (tileData.type == "TileDynamicData")
            {
                tileData = DataBaseManager.Instance.FindById<TileDynamicData>(idTileData);
       
            }
            if (tileData.type == "TileAnimateData")
            {
                tileData = DataBaseManager.Instance.FindById<TileAnimateData>(idTileData);
                //tileData.offsetInternal = tileData.offsetInternal+new Vector2(0.25f, 0.25f);
            }
            
         
            tilesDictionary.Add(tileData.id, tileData);
        }
       
        tileData = tilesDictionary[idTileData];
        return tileData;
    }

    public TileData GetTileData(int idTile)
    {
        if (!tilesDictionary.ContainsKey(idTile))
        {
            return RegisterTileData(idTile);
        }
        return tilesDictionary[idTile];
    }
    public TileAnimateData GetTileDataAnimation(int idTile)
    {
        if (!tilesDictionary.ContainsKey(idTile))
        {
            return (TileAnimateData)RegisterTileData(idTile);
        }
        return (TileAnimateData)tilesDictionary[idTile];
    }
    public Entity CreateTileStatic(int idTile, float scale, Vector3 worldPosition)
    {
        Entity entity = EcsManager.Instance.World.Create();
        TileDynamicData data = (TileDynamicData)GetTileData(idTile);

        Transform3D transform3D = new Transform3D(Basis.Identity, worldPosition);
        transform3D = transform3D.ScaledLocal(new Vector3(scale, scale, 1));

        var instanceComplex = MultimeshManager.Instance.CreateInstance(data.idMaterial);

        RenderingServer.MultimeshInstanceSetTransform(instanceComplex.rid, instanceComplex.instance, transform3D);
        RenderingServer.MultimeshInstanceSetCustomData(instanceComplex.rid, instanceComplex.instance, new Color(data.x, data.y, data.widhtFormat, data.heightFormat));
        RenderingServer.MultimeshInstanceSetColor(instanceComplex.rid, instanceComplex.instance, new Color(0, 0, 0, instanceComplex.materialBatchPosition));

        return entity;
    }
    public Entity CreateTileDinamic(int idTile, float scale, Vector2 worldPosition,int layer, Vector2 originOffset , int zOrdering = 0)
    {

        Entity entity = EcsManager.Instance.World.Create();
        TileDynamicData data = (TileDynamicData)GetTileData(idTile);

        Transform3D transform3D = new Transform3D(Basis.Identity, new Vector3(worldPosition.X,worldPosition.Y,worldPosition.Y));
        transform3D = transform3D.ScaledLocal(new Vector3(scale, scale, 1));

        var instanceComplex = MultimeshManager.Instance.CreateInstance(data.idMaterial);

        SpriteRenderGPUComponent spriteRenderGPU = new SpriteRenderGPUComponent();
        spriteRenderGPU.idMaterial = data.idMaterial;
        spriteRenderGPU.rid = instanceComplex.rid;
        spriteRenderGPU.instance = instanceComplex.instance;
        spriteRenderGPU.arrayPositiontexture = instanceComplex.materialBatchPosition;
        spriteRenderGPU.uvMap = new Color(data.xFormat, data.yFormat, data.widhtFormat, data.heightFormat);
        spriteRenderGPU.transform = transform3D;
        spriteRenderGPU.layerRender = layer;
        spriteRenderGPU.zOrdering = zOrdering;
        spriteRenderGPU.originOffset = originOffset;

        PositionComponent position = new PositionComponent();
        position.position = worldPosition;

        TilePositionComponent tilePositionComponent = new TilePositionComponent();
        tilePositionComponent.x = 0;
        tilePositionComponent.y = 0;
        entity.Add(spriteRenderGPU);
        entity.Add(position);
        entity.Add(tilePositionComponent);

        RenderingServer.MultimeshInstanceSetTransform(instanceComplex.rid, instanceComplex.instance, transform3D);
        RenderingServer.MultimeshInstanceSetCustomData(instanceComplex.rid, instanceComplex.instance, spriteRenderGPU.uvMap);
        RenderingServer.MultimeshInstanceSetColor(instanceComplex.rid, instanceComplex.instance, new Color(0, 0, 0, instanceComplex.materialBatchPosition));
        return entity;
    }

    public void FreeTileEntity(Entity entity)
    {
        SpriteRenderGPUComponent spriteRenderGPU = entity.Get<SpriteRenderGPUComponent>();
        RenderingServer.MultimeshInstanceSetCustomData(spriteRenderGPU.rid, spriteRenderGPU.instance, new Godot.Color(-1, 0, 0, 0));
        MultimeshManager.Instance.FreeInstance(spriteRenderGPU.rid, spriteRenderGPU.instance, spriteRenderGPU.idMaterial);
        EcsManager.Instance.World.Destroy(entity);
    }
    

    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
}
