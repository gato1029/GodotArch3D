using Arch.Core;
using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SpriteMultimesh;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

namespace GodotEcsArch.sources.managers.Maps;


public class TileDataGame
{
    // No se debe guardar
    public EntityReference entityReference {  get; set; }
    public int idCollider { get; set; }
    public int idMaterial { get; set; }
    public int idTile { get; set; }
    public Transform3D transform3d { get; set; }
    public GeometricShape2D collisionBody { get; set; }

    //Si se debe Guardar
    public Vector2I tilePositionChunk;
    public Vector2 positionCollider;
}
public class TerrainDataGame: TileDataGame
{
    // Se debe guardar
    public int idTerrain;

}
public class TerrainMap
{   
    Dictionary<int, TerrainData> terrainDictionary;
    Vector2I chunkDimencion;
    TileMapChunkRender<TerrainDataGame> tilemapTerrain;

    int layer;
    public TerrainMap()
    {
        terrainDictionary = new Dictionary<int, TerrainData>();
        layer = 0;        
        chunkDimencion = PositionsManager.Instance.chunkDimencion;
        tilemapTerrain = new TileMapChunkRender<TerrainDataGame>( layer,ChunkManager.Instance.tiles16X16);
    }

    public void AddUpdateTile(Vector2I tilePositionGlobal, int idTerrain)
    {
        TerrainData terrainData;
        if (!terrainDictionary.ContainsKey(idTerrain))
        {
            terrainData = DataBaseManager.Instance.FindById<TerrainData>(idTerrain);
            terrainDictionary.Add(idTerrain, terrainData);  
        }
        terrainData = terrainDictionary[idTerrain];
        TerrainDataGame terrainDataGame = new TerrainDataGame();
        terrainDataGame.idTerrain = idTerrain;
        if (terrainData.isRule)
        {
            tilemapTerrain.AddUpdatedTileRule(tilePositionGlobal, terrainData.autoTileData, terrainDataGame);
        }
        else
        {
         
        
            tilemapTerrain.AddUpdatedTile(tilePositionGlobal,  terrainData.tileData, terrainDataGame);
        }
    }

  

}
