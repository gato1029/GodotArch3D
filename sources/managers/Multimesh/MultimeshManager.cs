using Godot;
using GodotEcsArch.sources.managers.Textures;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Multimesh;
public class MultimeshManager:SingletonBase<MultimeshManager>
{
    
    
    public Dictionary<Rid, MultimeshData> multimeshDataDict;
    int sizeMax = 10000; // instanciasMaximas

    protected override void Initialize()
    {          
        multimeshDataDict = new Dictionary<Rid, MultimeshData>();

        var listData = DataBaseManager.Instance.FindAll<MaterialData>();
        foreach (var item in listData)
        {
            TextureArrayBuilder.Instance.SetTextureAt(item.id,FileHelper.GetPathGameDB(item.pathTexture));
        }
        TextureArrayBuilder.Instance.BuildAndApply();
    }

    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
 
    public void FreeInstance(Rid rid, int instance)
    {
        multimeshDataDict[rid].FreeInstance(instance);
    }
    public (Rid, int) CreateInstance()
    {
        MultimeshData multimeshData = null;
        foreach (var item in multimeshDataDict)
        {
            multimeshData = item.Value;
            if (multimeshData.freePositions.Count > 0)
            {
                multimeshData = item.Value;
                break;
            }
        }

        if (multimeshData == null)
        {
            multimeshData = new MultimeshData(TextureArrayBuilder.Instance.mesh, sizeMax);
            multimeshDataDict.Add(multimeshData.rid, multimeshData);
        }
        else
        {
            if (!multimeshData.AvailbleSpace())
            {
                multimeshData = new MultimeshData(TextureArrayBuilder.Instance.mesh, sizeMax);
                multimeshDataDict.Add(multimeshData.rid, multimeshData);
            }

        }

        return (multimeshData.rid, multimeshData.CreateInstance());
    }

    
}
