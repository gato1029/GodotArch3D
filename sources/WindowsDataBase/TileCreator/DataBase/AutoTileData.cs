using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System.Data;
using System.Linq;

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
            if (arrayTiles.Length>0)
            {
                textureVisual = arrayTiles[0].tileDataCentral.textureVisual;
            }
            
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

        public TileRuleData FindBestMatchingRule(byte mask, TileData[] neighborTiles)
        {
            foreach (var rule in arrayTiles)
            {
                bool usesSpecificIds = rule.neighborConditions.Any(c => c.SpecificTileId != 0);

                if (usesSpecificIds)
                {
                    if (rule.Matches(mask, neighborTiles))
                        return rule;
                }
                else
                {
                    if (rule.Matches(mask))
                        return rule;
                }
            }

            return null;
        }
        public TileRuleData FindBestMatchingRule(byte mask, int[] neighborTilesIds)
        {
            foreach (var rule in arrayTiles)
            {
                bool usesSpecificIds = rule.neighborConditions.Any(c => c.SpecificTileId != 0);

                if (usesSpecificIds)
                {
                    if (rule.Matches(mask, neighborTilesIds))
                        return rule;
                }
                else
                {
                    if (rule.Matches(mask))
                        return rule;
                }
            }

            return null;
        }
    }
}
