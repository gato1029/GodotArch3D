using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Group;
public class GroupData:IdData
{
    public int idMaterial { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public int width { get; set; }
    public int height{ get; set; }

    public GroupData()
    {
    }
    [BsonCtor]
    public GroupData(int idMaterial, int x, int y, int width, int height)
    {       
        textureVisual = MaterialManager.Instance.GetAtlasTextureInternal(idMaterial, x,y, width,height);        
    }
}
