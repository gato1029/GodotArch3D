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

public class InfoArrayMultimesh
{
    private int grupo;
    private HashSet<int> materiales = new HashSet<int>();
    Dictionary<Rid, MultimeshData> multimeshInfo = new Dictionary<Rid, MultimeshData>();
    TextureArrayBuilder textureArrayBuilder;
    // Propiedad para grupo
    public int Grupo
    {
        get => grupo;
        set => grupo = value;
    }

    // Propiedad para materiales (lectura)
    public IReadOnlyCollection<int> Materiales => materiales;

    // Propiedad para multimeshInfo (lectura)
    public IReadOnlyDictionary<Rid, MultimeshData> MultimeshInfo => multimeshInfo;
    public TextureArrayBuilder TextureArrayBuilder
    {
        get => textureArrayBuilder;
        set => textureArrayBuilder = value;
    }

    // Métodos para materiales
    public void AddMaterial(int materialId)
    {
        materiales.Add(materialId);
    }

    public bool RemoveMaterial(int materialId)
    {
        return materiales.Remove(materialId);
    }

    public bool ContainsMaterial(int materialId)
    {
        return materiales.Contains(materialId);
    }

    // Métodos para multimeshInfo
    public void AddMultimesh(Rid rid, MultimeshData data)
    {
        multimeshInfo[rid] = data;
    }

    public bool RemoveMultimesh(Rid rid)
    {
        return multimeshInfo.Remove(rid);
    }

    public bool TryGetMultimesh(Rid rid, out MultimeshData data)
    {
        return multimeshInfo.TryGetValue(rid, out data);
    }
    public MultimeshData CreateMultimeshData()
    {
        int INSTANCE_CAPACITY = 500;
        var mesh = (Mesh) textureArrayBuilder.mesh.Duplicate();
        var multimeshData = new MultimeshData(mesh, INSTANCE_CAPACITY);
        multimeshInfo.Add(multimeshData.multimeshRid, multimeshData);
        return multimeshData;
    }
    internal void RemoveInstance(Rid rid, int instance)
    {
        if (multimeshInfo.TryGetValue(rid, out var multiData))
        {
            multiData.FreeInstance(instance);
        }
        else
        {
            GD.PrintErr("Rid de eliminacion no encontrado:" + rid);
        }
    }
    internal (Rid, int) CreateInstance()
    {
        MultimeshData multimeshData = null;
        int instance = -1;
        Rid rid = default;
        foreach (var item in multimeshInfo)
        {
            multimeshData = item.Value;
            if (multimeshData.AvailbleSpace())
            {
                multimeshData = item.Value;
                instance =  multimeshData.CreateInstance();
                rid = multimeshData.multimeshRid;
                //GD.Print("IC: Rid:" + rid + " Instance:" + instance);
                return (rid, instance);                
            }
        }
        multimeshData = CreateMultimeshData();
        instance = multimeshData.CreateInstance();
        rid = multimeshData.multimeshRid;
        //GD.Print("IC: Rid:" + rid + " Instance:" + instance);
        return (rid, instance);

    }
    // Muestra toda la información de este InfoArrayMultimesh
    public void Show()
    {
        GD.Print($"--- InfoArrayMultimesh Grupo: {grupo} ---");
        GD.Print($"Materiales (count): {materiales.Count}");
        if (materiales.Count > 0)
        {
            GD.Print("Materiales: " + string.Join(", ", materiales));
        }

        GD.Print($"Multimesh entries: {multimeshInfo.Count}");
        foreach (var kv in multimeshInfo)
        {
            var rid = kv.Key;
            var data = kv.Value;
            int freeCount = data.freePositions != null ? data.freePositions.Count : 0;
            int currentPos = data.currentPosition;
            int maxInst = data.maxInstances;
            GD.Print($"  RID: {rid} | currentPosition: {currentPos} | maxInstances: {maxInst} | freePositions: {freeCount}");
        }

        if (textureArrayBuilder == null)
        {
            GD.Print("TextureArrayBuilder: null");
        }
        else
        {
            GD.Print("TextureArrayBuilder: present");
            GD.Print($"  ShaderUniformName: {textureArrayBuilder.ShaderUniformName}");
            GD.Print($"  TargetSize: {textureArrayBuilder.TargetSize.X}x{textureArrayBuilder.TargetSize.Y}");
            GD.Print($"  Mesh asignado: {(textureArrayBuilder.mesh != null ? "si" : "no")}");
            GD.Print($"  TargetMaterial asignado: {(textureArrayBuilder.TargetMaterial != null ? "si" : "no")}");
        }

        GD.Print($"--- End InfoArrayMultimesh Grupo: {grupo} ---");
    }

 
}
public class MultimeshArrayMaterial
{
    public MaterialType typeMaterial;
    

    private Dictionary<int, Dictionary<Rid, MultimeshData>> multimeshDataDict;
    private Dictionary<int, TextureArrayBuilder> groupArrayBuilder;

    int INSTANCE_CAPACITY = 1000; // instanciasMaximas
    private const int MAX_LAYERS = 256; // capas maximas por array

    // refactorizando
    private Dictionary<int, int> materialToGroup = new Dictionary<int, int>();
    private Dictionary<int, InfoArrayMultimesh> grupoInfo = new Dictionary<int, InfoArrayMultimesh>(); 
    public void Procces(MaterialType typeMaterial)
    {
        BsonExpression bsonExpression = BsonExpression.Create("materialType = @0", typeMaterial.ToString());
        var listData = DataBaseManager.Instance.FindAllFilter<TextureMasterData>(bsonExpression);

        var groupedTextures = new List<List<TextureMasterData>>();
        // Dividir materiales en bloques de 256 capas
        for (int i = 0; i < listData.Count; i += MAX_LAYERS)
        {
            var group = listData.Skip(i).Take(MAX_LAYERS).ToList();
            groupedTextures.Add(group);
        }

        for (int i = 0; i < groupedTextures.Count; i++)
        {
            int nroGrupo = i;

            InfoArrayMultimesh infoArrayMultimesh = new InfoArrayMultimesh();

            var textureGroup = groupedTextures[i];
            var texBuilder = new TextureArrayBuilder();
            for (int ii = 0; ii < textureGroup.Count; ii++)
            {
                TextureMasterData item = textureGroup[ii];
                texBuilder.SetTextureAt(ii, FileHelper.GetPathGameDB(item.pathTexture));
                foreach (var idMat in item.listMaterials)
                {
                    MaterialManager.Instance.SetMaterialPositionBatch(idMat, ii);
                    infoArrayMultimesh.AddMaterial(idMat);
                    materialToGroup.Add(idMat,nroGrupo);
                }
            }
            texBuilder.BuildAndApply();
            infoArrayMultimesh.TextureArrayBuilder = texBuilder;
            infoArrayMultimesh.CreateMultimeshData();
            grupoInfo.Add(nroGrupo, infoArrayMultimesh);
        }
    }
    public MultimeshArrayMaterial(MaterialType typeMaterial)
    {
        this.typeMaterial = typeMaterial;
        Procces(typeMaterial);
        
       
    }

    public void ShowInfo()
    {
        foreach (var item in grupoInfo)
        {
            item.Value.Show();
        }             
    }

    int FindGroupId(int materialId)
    {
        if (materialToGroup.TryGetValue(materialId, out int groupId))
            return groupId;
        return -1;
    }

    public void FreeInstance(Rid rid, int instance,int idMaterial)
    {
        int grupo = FindGroupId(idMaterial);
        if (grupoInfo.TryGetValue(grupo, out var infoData))
        {
            infoData.RemoveInstance(rid,instance);
        }
            //multimeshDataDict[grupo][rid].FreeInstance(instance);
    }

    public (Rid rid, int instance, int material, int layerTexture) CreateInstance(int idMaterial)
    {
        int grupo = FindGroupId(idMaterial);
        if (grupo==-1)
        {
            GD.PrintErr("Grupo no disponible");
            throw new InvalidOperationException($"Grupo no disponible para idMaterial {idMaterial}");
        }

        if (grupoInfo.TryGetValue(grupo,out var infoData))
        {
          var data = infoData.CreateInstance();
          int layerTexture = MaterialManager.Instance.GetMaterial(idMaterial).idMaterialPositionBatch;
          return (data.Item1, data.Item2, idMaterial,layerTexture);
        }
        return default;
    }

    public int GetMaterialBatch(int idMaterial)
    {
        return MaterialManager.Instance.GetMaterial(idMaterial).idMaterialPositionBatch;
    }
}
