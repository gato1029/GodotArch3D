using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
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
        TileData tileData = null;
        if (!tilesDictionary.ContainsKey(idTileData))
        {
             tileData = DataBaseManager.Instance.FindById<TileData>(idTileData);
            if (tileData.type == "TileSimpleData")
            {
                tileData = DataBaseManager.Instance.FindById<TileSimpleData>(idTileData);
            }
            if (tileData.type == "TileAnimateData")
            {
                tileData = DataBaseManager.Instance.FindById<TileAnimateData>(idTileData);
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
            return null;
        }
        return tilesDictionary[idTile];
    }
    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
}
