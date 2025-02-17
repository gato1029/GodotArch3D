using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System.Data;

namespace GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase
{
    public class AutoTileData : IdData
    {
        public TileRuleData[] arrayTiles { get;  }
  
        [BsonCtor]
        public AutoTileData( TileRuleData[] arrayTiles)
        {        

            this.arrayTiles = arrayTiles;
            foreach (var item in this.arrayTiles)
            {
                item.tileDataCentral = TilesManager.Instance.RegisterTileData(item.idTileDataCentral);
                
                for (int i = 0; i < 8; i++)
                {
                    if (item.idsTileDataMask[i] != 0)
                    {
                        item.tileDataMask[i] = TilesManager.Instance.RegisterTileData(item.idsTileDataMask[i]);
                    }
                }
            }
            textureVisual = arrayTiles[0].tileDataCentral.textureVisual;
        }
        public AutoTileData()
        {
           
        }
        
        public AutoTileData(TileRuleData[] arrayTiles, bool simple)
        {
            this.arrayTiles = arrayTiles;         
        }

        public TileRuleData FindBestMatchingRule( byte mask)
        {
            foreach (var rule in arrayTiles)
            {
                if (rule.Matches(mask))
                {
                    return rule;
                }
            }

            return  null; // Si no encuentra coincidencia, usa la primera
        }
    }
}
