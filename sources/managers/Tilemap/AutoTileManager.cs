using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Tilemap;
internal class AutoTileManager:SingletonBase<AutoTileManager>
{
    public Dictionary<int, AutoTileData> dictionaryData;
    protected override void Initialize()
    {
        dictionaryData = new Dictionary<int, AutoTileData>();
    }

    public void RegisterTileData(AutoTileData tileData)
    {
        if (!dictionaryData.ContainsKey(tileData.id))
        {
            dictionaryData.Add(tileData.id, tileData);
        }
    }
    public AutoTileData RegisterTileData(int idAutoTileData)
    {
        if (idAutoTileData==0)
        {
            return null;
        }
        AutoTileData tileData = null;
        if (!dictionaryData.ContainsKey(idAutoTileData))
        {
            tileData = DataBaseManager.Instance.FindById<AutoTileData>(idAutoTileData);         
            dictionaryData.Add(tileData.id, tileData);
        }
        else
        {
            tileData = DataBaseManager.Instance.FindById<AutoTileData>(idAutoTileData);
            dictionaryData[tileData.id]= tileData;
        }

        tileData = dictionaryData[idAutoTileData];
        return tileData;
    }

    public AutoTileData GetTileData(int idTile)
    {
        if (!dictionaryData.ContainsKey(idTile))
        {
            return RegisterTileData(idTile);
        }
        return dictionaryData[idTile];
    }
}
