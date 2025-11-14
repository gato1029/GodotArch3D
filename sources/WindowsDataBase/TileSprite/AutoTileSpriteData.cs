using GodotEcsArch.sources.KuroTiles;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.KuroTiles;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.TileSprite;
public class AutoTileSpriteData:IdDataLong
{    
    public List<TileRuleTemplate> tileRuleTemplates { get; set; }
    public AutoTileSpriteData()
    {
        id = EpochIdGenerator.NewId();
    }
    public void ReGerenateId()
    {
        id = EpochIdGenerator.NewId();
    }

    [BsonCtor]
    public AutoTileSpriteData(List<TileRuleTemplate> tileRuleTemplates)
    {
        if (tileRuleTemplates!=null && tileRuleTemplates.Count>0)
        {
            if (tileRuleTemplates[0]!=null)
            {
                var data = GroupManager.Instance.GetData(tileRuleTemplates[0].TileCentral.idGroup);
                textureVisual = data.textureVisual;
            }
            
        }
        
    }
}
