using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;
namespace GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
public class TerrainData :IdData
{
    public int idTile { get; set; }
    public bool isRule {  get; set; }
    public int idRule { get; set; }

    [BsonIgnore]
    public TileData tileData { get; set; }

    [BsonIgnore]
    public AutoTileData autoTileData { get; set; }

    public TerrainData()
    {
    }

    [BsonCtor]
    public TerrainData(int idTile, bool isRule, int idRule)
    {
        if (isRule)
        {
            autoTileData = DataBaseManager.Instance.FindById<AutoTileData>(idRule);      
        }
        else
        {
            tileData = TilesManager.Instance.RegisterTileData(idTile);
        }
    }
}
