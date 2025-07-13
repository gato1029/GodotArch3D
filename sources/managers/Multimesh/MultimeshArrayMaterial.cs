using Godot;
using GodotEcsArch.sources.managers.Textures;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Multimesh;
public class MultimeshArrayMaterial
{
    public MaterialType typeMaterial;
    private Dictionary<int, int> materialToGroup;
    
    public Dictionary<int, Dictionary<Rid, MultimeshData>> multimeshDataDict;
    private Dictionary<int, TextureArrayBuilder> groupArrayBuilder;

    int INSTANCE_CAPACITY = 10000; // instanciasMaximas
    private const int MAX_LAYERS = 256;    
    public MultimeshArrayMaterial(MaterialType typeMaterial)
    {
        
        this.typeMaterial = typeMaterial;
        multimeshDataDict = new Dictionary<int, Dictionary<Rid, MultimeshData>>();
        materialToGroup = new Dictionary<int, int>();

        BsonExpression bsonExpression = BsonExpression.Create("type = @0", (int)typeMaterial);
        var listData = DataBaseManager.Instance.FindAllFilter<MaterialData>(bsonExpression);

        
        groupArrayBuilder = new Dictionary<int, TextureArrayBuilder>();

        var groupedTextures = new List<List<MaterialData>>();
        // Dividir materiales en bloques de 256 capas
        for (int i = 0; i < listData.Count; i += MAX_LAYERS)
        {
            var group = listData.Skip(i).Take(MAX_LAYERS).ToList();
            groupedTextures.Add(group);
        }
        // Crear multimesh y texture array por grupo
        for (int i = 0; i < groupedTextures.Count; i++)
        {
            List<MaterialData> textureGroup = groupedTextures[i];
            var texBuilder = new TextureArrayBuilder();
            for (int ii = 0; ii < textureGroup.Count; ii++)
            {
                MaterialData item = textureGroup[ii];
                texBuilder.SetTextureAt(ii, FileHelper.GetPathGameDB(item.pathTexture));
                MaterialManager.Instance.SetMaterialPositionBatch(item.id, ii);
            }
            texBuilder.BuildAndApply();
            groupArrayBuilder.Add(i, texBuilder);
            // Crear primer multimesh para el grupo
            Dictionary<Rid, MultimeshData> multimeshDataDictInternal = new Dictionary<Rid, MultimeshData>();
            multimeshDataDict.Add(i, multimeshDataDictInternal);

            foreach (MaterialData item in textureGroup)
            {
                AddToGroup(i, item);
            }
        }


    }
    int FindGroupId(int materialId)
    {
        if (materialToGroup.TryGetValue(materialId, out int groupId))
            return groupId;
        return 0;
    }
    void AddToGroup(int groupId, MaterialData material)
    {
        if (!materialToGroup.ContainsKey(material.id))
        {
            materialToGroup[material.id] = groupId;
        }
    }
    public void FreeInstance(Rid rid, int instance,int idMaterial)
    {
        int grupo = FindGroupId(idMaterial);
        multimeshDataDict[grupo][rid].FreeInstance(instance);
    }
    public (Rid rid, int instance,  int materialBatchPosition) CreateInstance(int idMaterial)
    {
        int grupo = FindGroupId(idMaterial);
        MultimeshData multimeshData = null;
        foreach (var item in multimeshDataDict[grupo])
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
            multimeshData = new MultimeshData(groupArrayBuilder[grupo].mesh, INSTANCE_CAPACITY);
            multimeshDataDict[grupo].Add(multimeshData.rid, multimeshData);
        }
        else
        {
            if (!multimeshData.AvailbleSpace())
            {
                multimeshData = new MultimeshData(groupArrayBuilder[grupo].mesh, INSTANCE_CAPACITY);
                multimeshDataDict[grupo].Add(multimeshData.rid, multimeshData);
            }

        }

        return (multimeshData.rid, multimeshData.CreateInstance(), MaterialManager.Instance.GetMaterial(idMaterial).idMaterialPositionBatch);
    }  
}
