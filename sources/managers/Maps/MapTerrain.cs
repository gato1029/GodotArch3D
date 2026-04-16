using Arch.Core;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Profiler;
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

public class TerrainDataGame:DataItem
{
    public override void SetDataGame(DataRender render)
    {
        bool haveCollider = false;
        GeometricShape2D[] colliders = null;
        switch (render.GetSpriteData().tileSpriteType)
        {
            case TileSpriteType.Static:
                haveCollider = render.GetSpriteData().spriteData.haveCollider;
                if (haveCollider)
                { colliders = render.GetSpriteData().spriteData.listCollisionBody; }
                break;
            case TileSpriteType.Animated:
                haveCollider = render.GetSpriteData().animationData.haveCollider;
                if (haveCollider)
                { colliders = render.GetSpriteData().animationData.collisionBodyArray.ToArray(); }
                break;
            default:
                break;
        }
        if (haveCollider)
        {
            if (idCollider != 0)
            {
                CollisionManager.Instance.terrainColliders.RemoveCollider(idCollider);
                idCollider = 0;
            }
            if (colliders != null)
            {
                idCollider = CollisionManager.Instance.terrainColliders.AddColliderObject(this, colliders.ToList(), render.positionCollider,0);
            }
        }
        else
        {
            if (idCollider != 0)
            {
                CollisionManager.Instance.terrainColliders.RemoveCollider(idCollider);
                idCollider = 0;
            }
        }
    }
    public override void ClearDataGame()
    {
        if (idCollider != 0)
        {
            CollisionManager.Instance.terrainColliders.RemoveCollider(idCollider);
            idCollider = 0;
        }
    } 
}



public class MapTerrain
{    

    
    private LayerChunksMaps<TerrainDataGame> mapLayerDesign;
    public string pathMapParent { get; set; }   
    public int layer { get; set; }       
    public string pathCurrentCarpet { get; set; }      
    public LayerChunksMaps<TerrainDataGame> MapLayerDesign { get => mapLayerDesign; set => mapLayerDesign = value; } 
    public MapTerrain(string pathMapParent, int Layer)
    {
   
        layer = Layer;             
        this.pathMapParent = pathMapParent;
        this.pathCurrentCarpet = pathMapParent;

        mapLayerDesign = new LayerChunksMaps<TerrainDataGame>(pathCurrentCarpet);

        int layerDesign = layer;
  

        mapLayerDesign.AddLayer(TerrainTileType.Agua.ToString(), 5);
        mapLayerDesign.AddLayer(TerrainTileType.AdornosAgua.ToString(), 6);        
        mapLayerDesign.AddLayer(TerrainTileType.TierraNivel1.ToString(), 7);
        mapLayerDesign.AddLayer(TerrainTileType.CespedNivel1.ToString(), 8);
        mapLayerDesign.AddLayer(TerrainTileType.TierraNivel2.ToString(), 9);
        mapLayerDesign.AddLayer(TerrainTileType.CespedNivel2.ToString(), 10);
        mapLayerDesign.AddLayer(TerrainTileType.AdornosTierra.ToString(), 11);                
        mapLayerDesign.AddLayer(TerrainTileType.AdornosCesped.ToString(), 12);

        
    }

    public void SaveAllMap()
    {
        var pathFull = pathCurrentCarpet + "/terreno.pak";
        SpriteChunkSerializer.Instance.SerializarLayers(mapLayerDesign, pathFull);
    }
    public void LoadMapData()
    {
        var pathFull = pathCurrentCarpet + "/terreno.pak";
        String tit = "Tiempo Carga Mapa:";
        Dictionary<ChunkId, ChunkDataSerialized> resultado;
        using (new ProfileScope(tit))
        {
             resultado = SpriteChunkSerializer.Instance.CargarMapaCompleto(pathFull);
        }
        PerformanceTimer.Instance.Print(tit);


        var predat = DataBaseManager.Instance.FindAll<TileSpriteData>();

        foreach (var item in predat)
        {
            MasterDataManager.UpdateRegisterData(item.id,item);
        }



        mapLayerDesign.SetRenderEnableAllLayers(false);
        String tit2 = "Tiempo volcado:";        
        using (new ProfileScope(tit2))
        {
            Parallel.ForEach(resultado, item =>
            {
                var id = item.Key;
                var data = item.Value.renderTiles;

                for (int i = 0; i < data.Length; i++)
                {
                    for (int j = 0; j < data[i].Length; j++)
                    {
                        DataRender dr = data[i][j];
                        if (dr != null)
                        {
                            InsertDirect(id.Layer,
                                         new Vector2I(id.X, id.Y),
                                         new Vector2I(i, j),
                                         new TileTemplate(dr.idGroup, dr.idDataTileSprite));
                        }
                    }
                }
            });

        }
        PerformanceTimer.Instance.Print(tit2);
        String tit3 = "Tiempo Recalculo:";
        using (new ProfileScope(tit3))
        {
            RecalculateColliders();
        }
        PerformanceTimer.Instance.Print(tit3);
        mapLayerDesign.SetRenderEnableAllLayers(true);

        
    }
    private void RecalculateColliders()
    {
        SpriteMapChunk<TerrainDataGame> layerCurrent = null;        
        //layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.Agua.ToString());
        //layerCurrent.RecalculateAllColliders();
          
        //layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.AdornosAgua.ToString());
        //layerCurrent.RecalculateAllColliders();

        layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.TierraNivel1.ToString());
        layerCurrent.RecalculateAllColliders();

        //layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.CespedNivel1.ToString());
        //layerCurrent.RecalculateAllColliders();

        layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.TierraNivel2.ToString());
        layerCurrent.RecalculateAllColliders();

        //layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.CespedNivel2.ToString());
        //layerCurrent.RecalculateAllColliders();

        //layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.AdornosTierra.ToString());
        //layerCurrent.RecalculateAllColliders();

        //layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.AdornosCesped.ToString());
        //layerCurrent.RecalculateAllColliders();
    }
    private void InsertDirect(int layer, Vector2I chunkPosition, Vector2I positionInChunk, TileTemplate tileTemplate)
    {
        SpriteMapChunk<TerrainDataGame> layerCurrent = null;
        switch (layer)
        {
            case 5:
                 layerCurrent= mapLayerDesign.GetLayer(TerrainTileType.Agua.ToString());
                break;
            case 6:
                layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.AdornosAgua.ToString());
                break;
            case 7:
                layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.TierraNivel1.ToString());
                break;
            case 8:
                layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.CespedNivel1.ToString());
                break;
            case 9:
                layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.TierraNivel2.ToString());
                break;
            case 10:
                layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.CespedNivel2.ToString());
                break;
            case 11:
                layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.AdornosTierra.ToString());
                break;
            case 12:
                layerCurrent = mapLayerDesign.GetLayer(TerrainTileType.AdornosCesped.ToString());
                break;
            default:
                break;
        }
        layerCurrent.AddUpdatedTileSpriteInChunk( chunkPosition,positionInChunk, tileTemplate,false);

    }
    public void ClearFilesChunks()
    {
     
        mapLayerDesign.ClearAllFiles();
    }
    public void ClearMap()
    {        
    
        mapLayerDesign.ClearAll();        
    }
    public void EnableLayer(bool enable, TerrainTileType TerrainTileType)
    {
        mapLayerDesign.GetLayer(TerrainTileType.ToString()).SetRenderEnabledGlobal(enable);        
    }
    public void EnableLayerInternal(bool enable)
    {
     
    }

    internal void RemoveTileSprite(List<Vector2I> tilesToUpdate, long idTerrain, TerrainTileEntry terrainTileEntry)
    {
        //SpriteMapChunk<TerrainDataGame> layerCurrent = null;
        //List<SpriteMapChunk<TerrainDataGame>> layerUnder = null;
        //var dataReal = MasterDataManager.GetData<TerrainData>(idTerrain); // forzar carga de TileData

        //layerUnder = GetUnderLayer(terrainTileEntry.Type);
        //layerCurrent = mapLayerDesign.GetLayer(terrainTileEntry.Type.ToString());

  
       
        //var vecindadActualizar = TileSpriteHelper.GetBorderNeighbors(tilesToUpdate);

        //layerCurrent.SetRenderEnableManual(false);
        //foreach (var tilePositionGlobal in tilesToUpdate)
        //{
        //    layerCurrent.RemoveTileSprite(tilePositionGlobal,true, terrainTileEntry.TileId, layerUnder);
        //}
        //layerCurrent.SetRenderEnableManual(true);
        //foreach (var tilePositionGlobal in vecindadActualizar)
        //{
        //    layerCurrent.Refresh(tilePositionGlobal);
        //}

        
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
         
            default:
                break;
        }
        return layersUnder;
    }

    internal void AddUpdateTileSprite(List<Vector2I> tilesToUpdate, long idTerrain, TerrainTileEntry terrainTileEntry, TileTemplate tileTemplate = null)
    {

        //SpriteMapChunk<TerrainDataGame> layerCurrent = null;
        //List< SpriteMapChunk<TerrainDataGame> > layerUnder = null;
        //var dataReal = MasterDataManager.GetData<TerrainData>(idTerrain); // forzar carga de TileData
        
        //layerCurrent = mapLayerDesign.GetLayer(terrainTileEntry.Type.ToString());
        //layerUnder = GetUnderLayer(terrainTileEntry.Type);
 

        //var vecindadActualizar = TileSpriteHelper.GetBorderNeighbors(tilesToUpdate);

        //layerCurrent.SetRenderEnableManual(false);
        //foreach (var tilePositionGlobal in tilesToUpdate)
        //{
        //    //PaintTile(layerCurrent, layerUnder, tilePositionGlobal, idTerrain, idAutoTileSprite, tileTemplate);
        //    if (tileTemplate != null)
        //    {
        //        layerCurrent.AddUpdatedTileSprite(tilePositionGlobal, tileTemplate);
        //    }
        //    else
        //    {
        //        layerCurrent.AddUpdatedAutoTileSprite(tilePositionGlobal, terrainTileEntry.TileId, layerUnder);
        //    }
        //}
        //layerCurrent.SetRenderEnableManual(true);
        //foreach (var tilePositionGlobal in tilesToUpdate)
        //{
        //    layerCurrent.Refresh(tilePositionGlobal);
        //}
        //foreach (var tilePositionGlobal in vecindadActualizar)
        //{
        //    layerCurrent.Refresh(tilePositionGlobal);
        //}
    }

    internal void AddUpdateTileSprite(
    Vector2I tilePositionGlobal,
    long idTerrain,
    TerrainTileEntry terrainTileEntry,
    TileTemplate tileTemplate = null)
    {
        //// --- Cargar capas necesarias ---
        //var layerCurrent = mapLayerDesign.GetLayer(terrainTileEntry.Type.ToString());
        //var layerUnder = GetUnderLayer(terrainTileEntry.Type);

        //// Forzar carga de datos del terreno (igual que el original)
        //var dataReal = MasterDataManager.GetData<TerrainData>(idTerrain);

        //// --- Obtener vecindad a refrescar (optimizado para 1 tile) ---
        //// Si GetBorderNeighbors requiere lista, la creamos pero sin loops
        //var vecindad = TileSpriteHelper.GetBorderNeighbors(tilePositionGlobal);

        //// --- Desactiva render temporal para evitar updates múltiples ---
        //layerCurrent.SetRenderEnableManual(false);

        //// --- Pintar/actualizar el tile ---
        //if (tileTemplate != null)
        //{
        //    layerCurrent.AddUpdatedTileSprite(tilePositionGlobal, tileTemplate);
        //}
        //else
        //{
        //    layerCurrent.AddUpdatedAutoTileSprite(tilePositionGlobal, terrainTileEntry.TileId, layerUnder);
        //}

        //// --- Reactivar render ---
        //layerCurrent.SetRenderEnableManual(true);

        //// --- Refrescar tile principal ---
        //layerCurrent.Refresh(tilePositionGlobal);

        //// --- Refrescar vecinos / bordes ---
        //foreach (var neighborPos in vecindad)
        //{
        //    layerCurrent.Refresh(neighborPos);
        //}
    }


    internal void AddUpdateTileSpriteNotRender(
   Vector2I tilePositionGlobal,
   long idTerrain,
   TerrainTileEntry terrainTileEntry,
   TileTemplate tileTemplate = null)
    {
        // --- Cargar capas necesarias ---
        //var layerCurrent = mapLayerDesign.GetLayer(terrainTileEntry.Type.ToString());
        //var layerUnder = GetUnderLayer(terrainTileEntry.Type);
        
                     
        //// --- Pintar/actualizar el tile ---
        //if (tileTemplate != null)
        //{
        //    layerCurrent.AddUpdatedTileSprite(tilePositionGlobal, tileTemplate);
        //}
        //else
        //{
        //    layerCurrent.AddUpdatedAutoTileSprite(tilePositionGlobal, terrainTileEntry.TileId, layerUnder);
        //}      
    }

    public void AddUpdateTileBulk(List<Vector2I> tilePositions,
     long idTerrain,
    TerrainTileEntry terrainTileEntry,bool bulkmode
    )
    { // --- Cargar capas necesarias ---
        //var layerCurrent = mapLayerDesign.GetLayer(terrainTileEntry.Type.ToString());
        //var layerUnder = GetUnderLayer(terrainTileEntry.Type);
        
        //foreach (var item in tilePositions)
        //{
        //    layerCurrent.AddUpdatedAutoTileSprite(item, terrainTileEntry.TileId, layerUnder, bulkmode);
        //}
        
    }


    public void AddUpdateMatrix(List<Vector2I> tileBorders, List<Vector2I> matrix,
    long idTerrain,
    TerrainTileEntry terrainTileEntry
    )
    { // --- Cargar capas necesarias ---
        //var layerCurrent = mapLayerDesign.GetLayer(terrainTileEntry.Type.ToString());
        //var layerUnder = GetUnderLayer(terrainTileEntry.Type);

        //layerCurrent.AddUpdateMatrixSprite(matrix, terrainTileEntry.TileId);
        //layerCurrent.AddUpdateBordersSprite(tileBorders, terrainTileEntry.TileId, layerUnder);        
    }


    public void AplyRulesBulk(List<Vector2I> tilePositions,
     long idTerrain,
    TerrainTileEntry terrainTileEntry
    )
    { // --- Cargar capas necesarias ---
        //var layerCurrent = mapLayerDesign.GetLayer(terrainTileEntry.Type.ToString());
        //var layerUnder = GetUnderLayer(terrainTileEntry.Type);

        //var dataAuto = MasterDataManager.GetData<AutoTileSpriteData>(terrainTileEntry.TileId);

        //// --- Obtener vecindad a refrescar (optimizado para 1 tile) ---
        //// Si GetBorderNeighbors requiere lista, la creamos pero sin loops
        //layerCurrent.ApplyRulesBulk(tilePositions, dataAuto, layerUnder);
    }
}
