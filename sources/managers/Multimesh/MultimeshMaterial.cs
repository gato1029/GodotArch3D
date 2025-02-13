using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Multimesh;

public class MultimeshMaterial
{
    public MaterialData materialData;
    public Dictionary<Rid, MultimeshData> multimeshDataDict;

    public MultimeshMaterial(MaterialData MaterialData)
    {
        this.materialData = MaterialData;
        multimeshDataDict = new Dictionary<Rid, MultimeshData>();
               
        
    }
    public void FreeInstance(Rid rid, int instance)
    {
        multimeshDataDict[rid].FreeInstance(instance);
    }
    public (Rid, int) CreateInstance()
    {
        MultimeshData multimeshData =null;
        foreach (var item in multimeshDataDict)
        {
            multimeshData = item.Value;
            if (multimeshData.freePositions.Count>0)
            {
                multimeshData = item.Value;
                break;
            }
        }

        if (multimeshData==null)
        {
            multimeshData = new MultimeshData(materialData.mesh);
            multimeshDataDict.Add(multimeshData.rid,multimeshData);
        }
        else
        {
            if (!multimeshData.AvailbleSpace())
            {
                multimeshData = new MultimeshData(materialData.mesh);
                multimeshDataDict.Add(multimeshData.rid, multimeshData);
            }
            
        }

        return (multimeshData.rid, multimeshData.CreateInstance());
    }
}
