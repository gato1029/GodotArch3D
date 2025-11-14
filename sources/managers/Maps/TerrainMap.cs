using Arch.Core;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.managers.Serializer.Data;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Terrain;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using GodotFlecs.sources.KuroTiles;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Flecs.NET.Core.Ecs.Units;
using static SpriteMultimesh;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

namespace GodotEcsArch.sources.managers.Maps;


public enum TerrainMapLevelDesign
{
    Basico = -1,
    Completo = 0,
    Piso = 1,
    Camino = 2,
    Agua = 3,
    Ornamentos = 4,
}

[ProtoContract]
public class TerrainDataGame:DataItem
{
    public override void SetDataGame()
    {
        bool haveCollider = false;
        GeometricShape2D[] colliders = null;
        switch (GetSpriteData().tileSpriteType)
        {
            case TileSpriteType.Static:
                haveCollider = GetSpriteData().spriteData.haveCollider;
                if (haveCollider)
                { colliders = GetSpriteData().spriteData.listCollisionBody; }
                break;
            case TileSpriteType.Animated:
                haveCollider = GetSpriteData().animationData.haveCollider;
                if (haveCollider)
                { colliders = GetSpriteData().animationData.collisionBodyArray.ToArray(); }
                break;
            default:
                break;
        }
        if (haveCollider)
        {
            if (idUnique!=0)
            {
                CollisionManager.Instance.terrainColliders.RemoveCollider(idUnique);
                idUnique = 0;
            }            
            if (colliders != null)
            {
                idUnique = CollisionManager.Instance.terrainColliders.AddColliderObject(this, colliders.ToList(), positionCollider);                
            }
        }
        else
        {
            if (idUnique != 0)
            {
                CollisionManager.Instance.terrainColliders.RemoveCollider(idUnique);
                idUnique = 0;
            }
        }
    }
    public override void ClearDataGame()
    {
        if (idUnique != 0)
        {
            CollisionManager.Instance.terrainColliders.RemoveCollider(idUnique);
            idUnique = 0;
        }
    }
    public override TileSpriteData GetSpriteData()
    {
        return MasterDataManager.GetData<TileSpriteData>(idDataTileSprite);
    }

    public override bool IsAnimation()
    {
        return MasterDataManager.GetData<TileSpriteData>(idDataTileSprite).tileSpriteType == TileSpriteType.Animated;
    }

    public override AnimationStateData GetAnimationStateData()
    {
        return null; //TerrainManager.Instance.GetData(0).animationData[0]; // cambiar luego
    }

    public override int GetTypeData()
    {
        return 0; // (int)TerrainManager.Instance.GetData(idDataTileSprite).terrainType; // cambiar luego
    }
}


[ProtoContract]
public class TerrainMapInfo
{
    public int Seed {  get; set; }
}
[ProtoContract]
public class TerrainMap
{    
    [ProtoIgnore, JsonIgnore]
    private Vector2I chunkDimencion;


    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<TerrainDataGame> mapTerrainBasic; // representaciones Basicas no renderiza
    
    [ProtoIgnore, JsonIgnore]
    private LayerChunksMaps<TerrainDataGame> mapLayerDesign;

    [ProtoIgnore, JsonIgnore]
    private string carpet = "Terrain";

    [ProtoIgnore, JsonIgnore]
    private string name = "TerrainData";

  
    public string pathMapParent { get; set; }
   
    public int layer { get; set; }    
   
    public string pathCurrentCarpet { get; set; }

    public int seed { get; set; }

    public List<int> materialsUsed { get; set; } // esto con el fin de no demorar en cargar cada material
    public SpriteMapChunk<TerrainDataGame> MapTerrainBasic { get => mapTerrainBasic; set => mapTerrainBasic = value; }
    public LayerChunksMaps<TerrainDataGame> MapLayerDesign { get => mapLayerDesign; set => mapLayerDesign = value; } 
    public TerrainMap(string pathMapParent, int Layer)
    {
        materialsUsed =new List<int>();        
        layer = Layer;        
        chunkDimencion = PositionsManager.Instance.chunkDimencion;
        this.pathMapParent = pathMapParent;
        this.pathCurrentCarpet = pathMapParent + "/" + carpet;

        mapLayerDesign = new LayerChunksMaps<TerrainDataGame>(pathCurrentCarpet);

        int layerDesign = layer;
        mapTerrainBasic = new SpriteMapChunk<TerrainDataGame>("Basico",pathCurrentCarpet, 10, ChunkManager.Instance.tiles16X16, false,false);

        mapLayerDesign.AddLayer(TerrainTileType.Agua.ToString(), 1);
        mapLayerDesign.AddLayer(TerrainTileType.AdornosAgua.ToString(), 2);        
        mapLayerDesign.AddLayer(TerrainTileType.TierraNivel1.ToString(), 3);
        mapLayerDesign.AddLayer(TerrainTileType.CespedNivel1.ToString(), 4);
        mapLayerDesign.AddLayer(TerrainTileType.TierraNivel2.ToString(), 5);
        mapLayerDesign.AddLayer(TerrainTileType.CespedNivel2.ToString(), 6);
        mapLayerDesign.AddLayer(TerrainTileType.AdornosTierra.ToString(), 7);                
        mapLayerDesign.AddLayer(TerrainTileType.AdornosCesped.ToString(), 8);


        mapLayerDesign.AddAlias(TerrainTileType.TierraNivel2.ToString(), TerrainTileType.TierraElevacionNivel2.ToString());

        //mapLayerDesign.AddLayer(TerrainTileType.SuperficieInferiorAdornos.ToString(), 2);
        //mapLayerDesign.AddLayer(TerrainTileType.Superficie.ToString(), 3);
        //mapLayerDesign.AddLayer(TerrainTileType.SuperficieAdornos.ToString(), 4);
        //mapLayerDesign.AddLayer(TerrainTileType.Elevacion.ToString(), 5);
        //mapLayerDesign.AddLayer(TerrainTileType.AccesoElevacion.ToString(), 6);
        //mapLayerDesign.AddLayer(TerrainTileType.ElevacionAdornos.ToString(), 7);


        //   mapLayerDesign.AddAlias( TerrainTileType.Elevacion.ToString(), TerrainTileType.ElevacionSuperficie.ToString());

        //mapLayerDesign.AddLayer(TerrainType.Agua.ToString(),14);
        //mapLayerDesign.AddLayer(TerrainType.PisoBase.ToString(), 15);        
        //mapLayerDesign.AddLayer(TerrainType.CaminoPiso.ToString(), 16);                
        //mapLayerDesign.AddLayer(TerrainType.Elevacion.ToString(),  19);
        //mapLayerDesign.AddLayer(TerrainType.Muro.ToString(), 19);
        //mapLayerDesign.AddLayer(TerrainType.Ornamentos.ToString(), 20);


        //mapLayerDesign.AddAlias(TerrainType.PisoBase.ToString(), TerrainType.AguaBorde.ToString());
        //mapLayerDesign.AddAlias(TerrainType.Elevacion.ToString(), TerrainType.ElevacionBase.ToString());
        //mapLayerDesign.AddAlias(TerrainType.CaminoPiso.ToString(), TerrainType.Mosaico.ToString());
        //mapLayerDesign.AddAlias(TerrainType.CaminoPiso.ToString(), TerrainType.PisoDetalle.ToString());
    }

    public void SaveAllMap()
    {
        SerializerManager.SaveToFileJson(new TerrainMapInfo { Seed = this.seed }, pathCurrentCarpet, name);
        mapTerrainBasic.SaveAll();
        mapLayerDesign.SaveAll();
    }
    public void LoadMapData()
    {
        var pathFull = pathCurrentCarpet +"/"+ name+".json";
        var dataInfo = SerializerManager.LoadFromFileJson<TerrainMapInfo>(pathFull);
        this.seed = dataInfo.Seed;
        mapTerrainBasic.SetRenderEnabledGlobal(false);
        mapLayerDesign.SetRenderEnableAllLayers(false);
        mapTerrainBasic.LoadAll();
        mapLayerDesign.LoadAll();        
        mapLayerDesign.SetRenderEnableAllLayers(true);
    }
    public void ClearFilesChunks()
    {
        MapTerrainBasic.ClearAllFiles();
        mapLayerDesign.ClearAllFiles();
    }
    public void ClearMap()
    {        
        mapTerrainBasic.ClearAllChunks();
        mapLayerDesign.ClearAll();        
    }
    public void EnableLayer(bool enable, TerrainType terrainType)
    {
        mapLayerDesign.GetLayer(terrainType.ToString()).SetRenderEnabledGlobal(enable);        
    }
    public void EnableLayerInternal(bool enable)
    {
        mapTerrainBasic.SetRenderEnabledGlobal(enable);        
    }

    internal void RemoveTileSprite(List<Vector2I> tilesToUpdate, long idTerrain, TerrainTileEntry terrainTileEntry)
    {
        SpriteMapChunk<TerrainDataGame> layerCurrent = null;
        List<SpriteMapChunk<TerrainDataGame>> layerUnder = null;
        var dataReal = MasterDataManager.GetData<TerrainData>(idTerrain); // forzar carga de TileData

        layerUnder = GetUnderLayer(terrainTileEntry.Type);
        layerCurrent = mapLayerDesign.GetLayer(terrainTileEntry.Type.ToString());

  
       
        var vecindadActualizar = TileSpriteHelper.GetBorderNeighbors(tilesToUpdate);

        layerCurrent.SetRenderEnableManual(false);
        foreach (var tilePositionGlobal in tilesToUpdate)
        {
            layerCurrent.RemoveTileSprite(tilePositionGlobal,true, terrainTileEntry.TileId, layerUnder);
        }
        layerCurrent.SetRenderEnableManual(true);
        foreach (var tilePositionGlobal in vecindadActualizar)
        {
            layerCurrent.Refresh(tilePositionGlobal);
        }

        
    }
    List <SpriteMapChunk<TerrainDataGame>> GetUnderLayer(TerrainTileType terrainTileType)
    {
        List<SpriteMapChunk<TerrainDataGame>> layersUnder = new List<SpriteMapChunk<TerrainDataGame>>();
        switch (terrainTileType)
        {
            case TerrainTileType.Agua:                
                break;
            case TerrainTileType.TierraNivel1:
                layersUnder.Add( mapLayerDesign.GetLayer(TerrainTileType.Agua.ToString()));
                break;
            case TerrainTileType.CespedNivel1:
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.TierraNivel1.ToString()));
                break;
            case TerrainTileType.TierraNivel2:
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.TierraNivel1.ToString()));
                break;
            case TerrainTileType.CespedNivel2:
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.TierraNivel2.ToString()));
                break;
            case TerrainTileType.AdornosAgua:
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.Agua.ToString()));
                break;
            case TerrainTileType.AdornosTierra:
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.TierraNivel1.ToString()));
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.TierraNivel2.ToString()));
                break;
            case TerrainTileType.AdornosCesped:
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.CespedNivel1.ToString()));
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.CespedNivel2.ToString()));
                break;
            case TerrainTileType.AguaCamino:
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.Agua.ToString()));
                break;
            case TerrainTileType.TierraCamino:
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.TierraNivel1.ToString()));
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.TierraNivel2.ToString()));
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.CespedNivel1.ToString()));
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.CespedNivel2.ToString()));
                break;
            case TerrainTileType.TierraElevacionNivel2:
                layersUnder.Add(mapLayerDesign.GetLayer(TerrainTileType.TierraNivel1.ToString()));
                break;
            default:
                break;
        }
        return layersUnder;
    }
    void PaintTile(SpriteMapChunk<TerrainDataGame> layerCurrent, List<SpriteMapChunk<TerrainDataGame>> layerUnder, Vector2I tilePosition, long idTerrain, long idAutoTileSprite, TileTemplate tileTemplate = null)
    {
        var dataAuto = MasterDataManager.GetData<AutoTileSpriteData>(idAutoTileSprite);
        
        if (tileTemplate != null)
        {
            layerCurrent.AddUpdatedTileSprite(tilePosition, tileTemplate);
        }
        else
        {
            layerCurrent.AddUpdatedAutoTileSprite(tilePosition, idAutoTileSprite);
        }
    }
    internal void AddUpdateTileSprite(List<Vector2I> tilesToUpdate, long idTerrain, TerrainTileEntry terrainTileEntry, TileTemplate tileTemplate = null)
    {

        SpriteMapChunk<TerrainDataGame> layerCurrent = null;
        List< SpriteMapChunk<TerrainDataGame> > layerUnder = null;
        var dataReal = MasterDataManager.GetData<TerrainData>(idTerrain); // forzar carga de TileData
        
        layerCurrent = mapLayerDesign.GetLayer(terrainTileEntry.Type.ToString());
        layerUnder = GetUnderLayer(terrainTileEntry.Type);
 

        var vecindadActualizar = TileSpriteHelper.GetBorderNeighbors(tilesToUpdate);

        layerCurrent.SetRenderEnableManual(false);
        foreach (var tilePositionGlobal in tilesToUpdate)
        {
            //PaintTile(layerCurrent, layerUnder, tilePositionGlobal, idTerrain, idAutoTileSprite, tileTemplate);
            if (tileTemplate != null)
            {
                layerCurrent.AddUpdatedTileSprite(tilePositionGlobal, tileTemplate);
            }
            else
            {
                layerCurrent.AddUpdatedAutoTileSprite(tilePositionGlobal, terrainTileEntry.TileId, layerUnder);
            }
        }
        layerCurrent.SetRenderEnableManual(true);
        foreach (var tilePositionGlobal in tilesToUpdate)
        {
            layerCurrent.Refresh(tilePositionGlobal);
        }
        foreach (var tilePositionGlobal in vecindadActualizar)
        {
            layerCurrent.Refresh(tilePositionGlobal);
        }
    }


    private void AddMaterialInternal(int idMaterial)
    {
        if (!materialsUsed.Contains(idMaterial) && idMaterial != 0)
        {
            materialsUsed.Add(idMaterial);
        }        
    }


}
