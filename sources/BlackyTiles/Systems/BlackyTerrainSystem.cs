using Arch.Core;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles.Tiles;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs;

namespace GodotEcsArch.sources.BlackyTiles.Systems;
public class BlackyTerrainSystem
    : BlackyChunkSystem<BlackyTerrainChunkData>,
      BlackyAutoTileDataProvider
{
    private readonly BlackyChunkOccupancyMap occupancyMap;
    private readonly BlackyTileRenderSystem renderSystem;
    private readonly BlackyAutoTileRuleResolver ruleResolver;

    private const int RegionResolveThreshold = 32;

    private readonly Dictionary<Vector2I, int> _topHeightBeforeBuffer = new();
    private readonly HashSet<Vector2I> _topHeightAffectedBuffer = new();

    public event Action<Vector2I, int, int> OnTopHeightChanged;

    public BlackyTerrainSystem(
        BlackyChunkRenderData renderData,
        BlackyChunkOccupancyMap occupancyMap,
        BlackyTileRenderSystem tileRender)
        : base(renderData)
    {
        this.occupancyMap = occupancyMap;
        this.renderSystem = tileRender;

        tileRender.OnRefreshDirtyTile += TileRender_OnRefreshDirtyTile;
        tileRender.OnRefreshDirtyChunk += TileRender_OnRefreshDirtyChunk;

        ruleResolver = new BlackyAutoTileRuleResolver(this);
    }
    private void BeginTopHeightTracking(List<Vector2I> worldPositions)
    {
        _topHeightBeforeBuffer.Clear();
        _topHeightAffectedBuffer.Clear();

        foreach (var pos in worldPositions)
        {
            if (_topHeightAffectedBuffer.Add(pos))
            {
                _topHeightBeforeBuffer[pos] = GetTopHeight(pos.X, pos.Y);
            }
        }
    }

    private void EndTopHeightTracking()
    {
        foreach (var pos in _topHeightAffectedBuffer)
        {
            int oldTopHeight = _topHeightBeforeBuffer[pos];
            int newTopHeight = GetTopHeight(pos.X, pos.Y);

            if (oldTopHeight != newTopHeight)
            {
                OnTopHeightChanged?.Invoke(pos, oldTopHeight, newTopHeight);
            }
        }

        _topHeightBeforeBuffer.Clear();
        _topHeightAffectedBuffer.Clear();
    }


    #region ===== DATA PROVIDER =====

    public bool TryGetTileData(
      int altura,
      int layer,
      int worldX,
      int worldY,
      out long idTile,
      out int idGroup)
    {
        idTile = 0;
        idGroup = 0;

        var tile = GetTileFast(altura,(BlackyTerrainLayer)layer, worldX, worldY);

        if (tile.IsEmpty)
            return false;

        var template = MasterDataManager.GetBySaveIds<TileSpriteData>(tile.TileSetId, tile.TileGroup);

        idTile = template.id;
        idGroup = tile.RuleGrouping;

        
        return true;
    }
    #endregion

    #region ===== PUBLIC API =====

    public bool SetTerrain(int worldX, int worldY, ushort idTerrainBiome, long idAutoTile, int altura, int layer, bool resolveAutoTile = true, bool overrideTile = true, bool renderTiles = true)
    {
        return SetTerrainList(
            new List<Vector2I> { new Vector2I(worldX, worldY) }, idTerrainBiome,
             idAutoTile,  altura,  layer, resolveAutoTile, overrideTile, renderTiles);
    }
    private readonly List<Vector2I> _placedBuffer = new();

    public bool SetTerrainList(List<Vector2I> worldPositions,ushort idTerrainBiome, long idAutoTile, int altura, int layer, bool resolveAutoTile = true, bool overrideTile = true, bool renderTiles = true)
    {
        
        AutoTileSpriteData rule =MasterDataManager.GetData<AutoTileSpriteData>(idAutoTile);
    
        if (rule == null)
        {
            return false;
        }
        bool placedAny = false;

        _placedBuffer.Clear();

        BeginTopHeightTracking(worldPositions);

        foreach (var pos in worldPositions)
        {
            if (overrideTile)
            {
                InternalPlaceLogicalOnly(altura,
                    (BlackyTerrainLayer)layer,
                    pos.X,
                    pos.Y,
                    rule,
                    idTerrainBiome);

                _placedBuffer.Add(pos);
                placedAny = true;
            }
        }

        
        // 🔥 Solo resolvemos las posiciones que realmente cambiaron
        if (resolveAutoTile && placedAny)
        {
            ResolveAutoTile(altura, layer, _placedBuffer, rule);
        }
        else
        {
            ResolveTilesDirect(altura, layer, _placedBuffer, rule);
        }
        if (renderTiles)
        {
            ForceUpdateTiles(_placedBuffer, altura, (BlackyTerrainLayer)layer);
        }
        EndTopHeightTracking();
        return placedAny;        
    }
  

    private void ResolveTilesDirect(int altura,
    int layer,
    List<Vector2I> worldPositions,
    AutoTileSpriteData rule)
    {
        foreach (var pos in worldPositions)
        {
            
          ApplyTileTemplate(altura, layer, pos.X, pos.Y, rule.tileRuleTemplates.Last().TileCentral);
        }
            
    }

    public void ForceUpdateTiles(List<Vector2I> worldPositions, int altura, BlackyTerrainLayer entry)
    {
        var affected = BuildAffectedSet(worldPositions);

        foreach (var pos in affected)
        {
            ForceUpdateTile(pos.X, pos.Y, altura, entry);
        }
    }
    public void ForceUpdateTile(int worldX, int worldY, int altura, BlackyTerrainLayer entry)
    {
        var (chunk, localX, localY) = RenderData.Resolve(worldX, worldY);
        if (chunk == null)
            return;
        
        renderSystem.ForceUpdateTile(chunk, altura, (int)entry, localX, localY);
    }



    public void RemoveTerrain(int worldX, int worldY, int altura, int layer, long idAutoTile,bool resolveAutotile=true, bool resolveRender =true)
    {
        RemoveTerrainList(
            new List<Vector2I> { new Vector2I(worldX, worldY) },
             altura, layer,idAutoTile,resolveAutotile, resolveRender);
    }

    public bool RemoveTerrainList(
        List<Vector2I> worldPositions,
        int altura, int layer, long idAutoTile,bool resolveAutoTile=true, bool resolveRender =true)
    {             

        var rule = MasterDataManager
            .GetData<AutoTileSpriteData>(idAutoTile);

        _placedBuffer.Clear();
        BeginTopHeightTracking(worldPositions);
        foreach (var pos in worldPositions)
        {
            InternalRemoveLogicalOnly(altura, (BlackyTerrainLayer)layer, pos.X, pos.Y,resolveRender);
            _placedBuffer.Add(pos);
        }
        if (resolveAutoTile)
        {
            ResolveAutoTile(altura, layer, worldPositions, rule);
        }
        if (resolveRender)
        {
            ForceUpdateTiles(_placedBuffer, altura, (BlackyTerrainLayer)layer);
        }
        BeginTopHeightTracking(worldPositions);

        return true;
    }

    public int GetTopHeight(Vector2I worldPos)
    {
        return GetTopHeight(worldPos.X, worldPos.Y);
    }
    public int GetTopHeight(int worldX, int worldY)
    {
        if (!TryResolve(worldX, worldY,
            out var chunk, out var lx, out var ly))
            return -1;
        if (!TryGetChunkData(chunk.Coord, out var chunkData))
            return -1;
        ref var column = ref chunkData.GetColumn(lx, ly);
        
        return column.GetTopHeight();
        
    }
    public BlackyTerrainLayerTile GetTerrain(
        int height,
        BlackyTerrainLayer layer,
        int worldX,
        int worldY)
    {
        if (!TryResolve(worldX, worldY,
            out var chunk, out var lx, out var ly))
            return default;

        if (!TryGetChunkData(chunk.Coord, out var chunkData))
            return default;

        ref var column = ref chunkData.GetColumn(lx, ly);

        ref var level = ref column.GetLevel(height);

        return level.GetLayer(layer);
    }

    public bool CanPlaceTerrain(int layer, int worldX, int worldY)
    {
        return !occupancyMap.IsOccupied(layer, worldX, worldY);
    }

    #endregion

    #region ===== CORE LOGIC =====

    private void InternalPlaceLogicalOnly(
    int height,
    BlackyTerrainLayer layer,
    int worldX,
    int worldY,
    AutoTileSpriteData rule, ushort idBiome)
    {
        var (chunk, localX, localY) =
            RenderData.ResolveOrCreate(worldX, worldY);

        var chunkData = GetOrCreateChunkData(
            chunk.Coord,
            () => new BlackyTerrainChunkData(RenderData.ChunkSize,idBiome));

        var baseTemplate = rule.tileRuleTemplates[0].TileCentral;

        var tileSpriteData =
            MasterDataManager.GetData<TileSpriteData>(
                baseTemplate.idTileSprite);

        var groupingData = 
            MasterDataManager.GetData<GroupingData>(
                tileSpriteData.idGrouping);

        var tile = new BlackyTerrainLayerTile
        {
            IdBiome = idBiome,
            RuleType = (ushort)rule.idSave,
            RuleGrouping =(ushort) baseTemplate.idGroup,
            TileGroup = (ushort)groupingData.idSave,
            TileSetId = (ushort)tileSpriteData.idSave
        };

        ref var column = ref chunkData.GetColumn(localX, localY);

        column.SetLayer(height, layer, tile);

        chunk.MarkDirty();
    }

    private void InternalRemoveLogicalOnly(
     int height,
     BlackyTerrainLayer layer,
     int worldX,
     int worldY, bool resolveRender)
    {
        if (!TryResolve(worldX, worldY,
            out var chunk, out var localX, out var localY))
            return;

        if (!TryGetChunkData(chunk.Coord, out var chunkData))
            return;
        
        ref var column = ref chunkData.GetColumn(localX, localY);

        column.GetLevel(height).ClearLayer(layer);
       
            var rd = RenderData.Resolve(worldX, worldY);
            rd.chunk.GetLayer(height, (int)layer).ClearRender(localX, localY);
        if (resolveRender)
        {
            renderSystem.ForceRemoveTile(
               new Vector2I(chunk.Coord.X, chunk.Coord.Y), height,
               (int)layer, localX, localY);
        }
        

//        chunk.MarkDirty();
    }

    #endregion

    #region ===== AUTO TILE RESOLUTION =====

    private void ResolveAutoTile(int altura,
        int layer,
        List<Vector2I> worldPositions,
        AutoTileSpriteData rule)
    {
        if (worldPositions.Count >= RegionResolveThreshold)
        {
            ruleResolver.ResolveRegion(altura,layer, worldPositions, rule);
            return;
        }
        if (rule.tileRuleTemplates.Count==1)
        {
          
            TileTemplate templateToApply;

            var bestRule = rule.tileRuleTemplates[0];
            if (bestRule.IsRandomTiles)
            {
                int index = (int)(GD.Randi() % bestRule.RandomTiles.Count);
                templateToApply = bestRule.RandomTiles[index];
            }
            else
            {
                templateToApply = bestRule.TileCentral;
            }


            foreach (var item in worldPositions)
            {
                ApplyTileTemplate(altura,
                     layer,
                     item.X,
                     item.Y,
                     templateToApply);
            }
         
        }
        else
        {
            var affected = BuildAffectedSet(worldPositions);
            foreach (var pos in affected)
                ruleResolver.ResolveSingle(altura, layer, pos, rule);
        }
        
    }

    private HashSet<Vector2I> BuildAffectedSet(
        List<Vector2I> worldPositions)
    {
        HashSet<Vector2I> affected = new();

        foreach (var pos in worldPositions)
        {
            affected.Add(pos);

            for (int i = 0; i < 8; i++)
            {
                var dir = (NeighborPosition)i;
                affected.Add(GetNeighborPositionTileSprite(pos, dir));
            }
        }

        return affected;
    }

    #endregion

    #region ===== TEMPLATE APPLY =====

    public void ApplyTileTemplate(int altura,
        int layer,
        int worldX,
        int worldY,
        TileTemplate template, bool clearData = false)
    {


        if (clearData)
        {
            if (!TryResolve(worldX, worldY, out var chunkData, out var localX, out var localY))
                return;

            if (!TryGetChunkData(chunkData.Coord, out var chunkDataLogical))
                return;

            chunkDataLogical.GetColumn(localX,localY).GetLevel(altura).ClearLayer(layer);

            var rd = RenderData.Resolve(worldX, worldY);
            rd.chunk.GetLayer(altura, (int)layer).ClearRender(localX, localY);

            //occupancyMap.Clear(layer, worldX, worldY);
            return;
        }
        var renderChunk = RenderData.Resolve(worldX, worldY);
       
        
        
        if (renderChunk.chunk == null)
        {
            return;
        }
            
        //ApplyTileTemplateInternal(chunk, altura, layer, lx, ly, template);
        if (renderChunk.chunk.GetLayer(altura, layer).isEmpty(renderChunk.localX, renderChunk.localY))
        {
            ApplyTileTemplateInternal(renderChunk.chunk, altura, layer, renderChunk.localX ,renderChunk.localY, template,worldX,worldY);
        }
        else
        {
            if (TryGetTileData(altura, layer, worldX, worldY, out long currentTileId, out _))
            {
                if (currentTileId != template.idTileSprite)
                {
                    ApplyTileTemplateInternal(renderChunk.chunk, altura, layer, renderChunk.localX, renderChunk.localY, template, worldX,worldY); // Solo aplicamos el template si el tile actual es diferente al del template, evitando cambios innecesarios
                }

            }
        }

    }

    private void ApplyTileTemplateInternal(
        BlackyChunk chunk,
        int altura,
        int layer,
        int localX,
        int localY,
        TileTemplate tileTemplate,int worldX, int worldY)
    {
        var tileTemplateData =
            MasterDataManager.GetData<TileSpriteData>(
                tileTemplate.idTileSprite);

        var groupTemplateData =
            MasterDataManager.GetData<GroupingData>(
                tileTemplateData.idGrouping);

        float scale = 1f;
        Godot.Vector2 offset = Godot.Vector2.Zero;
        GeometricShape2D[] collisions = null;

        switch (tileTemplateData.tileSpriteType)
        {
            case TileSpriteType.Static:
                offset = tileTemplateData.spriteData.offsetInternal;
                scale = tileTemplateData.spriteData.scale;
                if (tileTemplateData.spriteData.listCollisionBody!=null)
                {
                    collisions = new GeometricShape2D[tileTemplateData.spriteData.listCollisionBody.Count()];
                    for (int i = 0; i < tileTemplateData.spriteData.listCollisionBody.Count(); i++)
                    {
                        collisions[i] = tileTemplateData.spriteData.listCollisionBody[i].Multiplicity(scale);
                    }
                }
                break;

            case TileSpriteType.Animated:
                offset = tileTemplateData.animationData.offsetInternal;
                scale = tileTemplateData.animationData.scale;
                if (tileTemplateData.animationData.collisionBodyArray!=null)
                {
                    collisions = new GeometricShape2D[tileTemplateData.animationData.collisionBodyArray.Count];
                    for (int i = 0; i < tileTemplateData.animationData.collisionBodyArray.Count; i++)
                    {
                        collisions[i] = tileTemplateData.animationData.collisionBodyArray[i].Multiplicity(scale);
                    }
                }
               
                break;
        }
        ushort colliderId = 0;
        TryGetChunkData(chunk.Coord, out var chunkData);
        if (collisions != null)
        {
            if (collisions.Length > 0)
            {
                colliderId = TerrainCollisionLibrary.AddComplexTemplate(collisions);
                //CollisionShapeDraw.Instance.DrawCollisionShapes(collisions.ToList(), TilesHelper.WorldPositionTile( new Godot.Vector2I(worldX,worldY)));
            }
     

        }
        ref var tile = ref chunkData.GetColumn(localX, localY).GetLevel(altura).GetLayer((BlackyTerrainLayer)layer);               
        
        tile.RuleGrouping = (ushort)tileTemplate.idGroup;
        tile.TileGroup = (ushort)groupTemplateData.idSave;
        tile.TileSetId = (ushort)tileTemplateData.idSave;
        tile.CollisionId = colliderId;

        var dbTile = MasterDataManager.GetBySaveIds<TileSpriteData>(tile.TileSetId, tile.TileGroup);

        chunk.GetLayer(altura,layer)
            .SetRender(localX, localY,
                tile.RuleGrouping,
                dbTile.id,
                offset.X * scale,
                offset.Y * scale);
        
        
    }

    #endregion

    #region ===== HELPERS =====

    private bool TryResolve(
        int worldX,
        int worldY,
        out BlackyChunk chunk,
        out int localX,
        out int localY)
    {
        (chunk, localX, localY) =
            RenderData.Resolve(worldX, worldY);

        return chunk != null;
    }

    public BlackyTerrainLayerTile GetTileTop(int worldX, int worldY, int height)
    {
        if (height==-1)
        {
            return default;
        }
        if (!TryResolve(worldX, worldY,
            out var chunk, out var lx, out var ly))
            return default;

        if (!TryGetChunkData(chunk.Coord, out var data))
            return default;

        ref var column = ref data.GetColumn(lx, ly);

        ref var level = ref column.GetLevel(height);

        return level.GetTopOccupiedTile();

    }
    private BlackyTerrainLayerTile GetTileFast(
       int height,
       BlackyTerrainLayer layer,
       int worldX,
       int worldY)
    {
        if (!TryResolve(worldX, worldY,
            out var chunk, out var lx, out var ly))
            return default;

        if (!TryGetChunkData(chunk.Coord, out var data))
            return default;

        ref var column = ref data.GetColumn(lx, ly);

        ref var level = ref column.GetLevel(height);

        return level.GetLayer(layer);
    }

    public Vector2I GetNeighborPositionTileSprite(
        Vector2I tilePos,
        NeighborPosition direction)
    {
        return direction switch
        {
            NeighborPosition.Arriba =>
                new Vector2I(tilePos.X, tilePos.Y + 1),

            NeighborPosition.Derecha =>
                new Vector2I(tilePos.X + 1, tilePos.Y),

            NeighborPosition.Abajo =>
                new Vector2I(tilePos.X, tilePos.Y - 1),

            NeighborPosition.Izquierda =>
                new Vector2I(tilePos.X - 1, tilePos.Y),

            NeighborPosition.ArribaDerecha =>
                new Vector2I(tilePos.X + 1, tilePos.Y + 1),

            NeighborPosition.AbajoDerecha =>
                new Vector2I(tilePos.X + 1, tilePos.Y - 1),

            NeighborPosition.AbajoIzquierda =>
                new Vector2I(tilePos.X - 1, tilePos.Y - 1),

            NeighborPosition.ArribaIzquierda =>
                new Vector2I(tilePos.X - 1, tilePos.Y + 1),

            _ => tilePos
        };
    }

    private void TileRender_OnRefreshDirtyTile(
        int layer,
        Vector2I tileposition)
    {
        // Hook opcional
    }

    private readonly List<int> _layerBuffer = new();
    private readonly List<Vector2I> _localBuffer = new();
    private readonly List<Vector2I> _worldBuffer = new();

    private void TileRender_OnRefreshDirtyChunk(BlackyChunk obj)
    {
        //if (!TryGetChunkData(obj.Coord, out var chunkData))
        //    return;

        //if (chunkData.IsChunkEmpty())
        //    return;

        //ushort idBiome = chunkData.GetBiomeId();
        //var tdata = MasterDataManager.GetBySaveIds<TerrainData>(idBiome);

        //int chunkSize = chunkData.Size;

        //// Offset del chunk en mundo
        //int baseWorldX = obj.Coord.X * chunkSize;
        //int baseWorldY = obj.Coord.Y * chunkSize;

        //chunkData.GetOccupiedLayers(_layerBuffer);

        //for (int i = 0; i < _layerBuffer.Count; i++)
        //{
        //    int layer = _layerBuffer[i];

        //    // 1️⃣ Obtener posiciones locales
        //    chunkData.GetOccupiedPositions(layer, _localBuffer);

        //    if (_localBuffer.Count == 0)
        //        continue;

        //    // 2️⃣ Convertir a posiciones de mundo
        //    _worldBuffer.Clear();

        //    for (int p = 0; p < _localBuffer.Count; p++)
        //    {
        //        var local = _localBuffer[p];

        //        _worldBuffer.Add(new Vector2I(
        //            baseWorldX + local.X,
        //            baseWorldY + local.Y));
        //    }

        //    // 3️⃣ Resolver autotile usando world positions
        //    if (tdata.typesTerrain.TryGetValue((TerrainTileType)layer, out var entryTerrain))
        //    {
        //        var rule = MasterDataManager.GetData<AutoTileSpriteData>(entryTerrain.TileId);
        //        ResolveAutoTile(layer, _worldBuffer, rule);
        //    }
        //}
    }

    #endregion


    private void GetChunkWorldPositions(Vector2I chunkCoord, List<Vector2I> buffer)
    {
        buffer.Clear();

        int chunkSize = RenderData.ChunkSize;

        int baseWorldX = chunkCoord.X * chunkSize;
        int baseWorldY = chunkCoord.Y * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                buffer.Add(new Vector2I(
                    baseWorldX + x,
                    baseWorldY + y));
            }
        }
    }

}