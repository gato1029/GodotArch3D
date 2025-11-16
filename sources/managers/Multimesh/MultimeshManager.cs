using Godot;
using GodotEcsArch.sources.managers.Textures;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.HttpRequest;

namespace GodotEcsArch.sources.managers.Multimesh;

public struct PendingRemoveInstance
{
    public PendingRemoveInstance(Rid rid, int instance, int materialId)
    {
        Rid = rid;
        this.instance = instance;
        this.materialId = materialId;
    }

    public Rid Rid { get; set; }
    public int instance {  get; set; }
    public int materialId { get; set; }
}
public class MultimeshManager:SingletonBase<MultimeshManager>
{
    private readonly ConcurrentQueue<PendingRemoveInstance> pendingRemoveInstances
          = new ConcurrentQueue<PendingRemoveInstance>();

    private Dictionary<MaterialType, MultimeshArrayMaterial> multiMeshArrayShaders;
    protected override void Initialize()
    {
        multiMeshArrayShaders = new Dictionary<MaterialType, MultimeshArrayMaterial>();      
        foreach (MaterialType value in Enum.GetValues(typeof(MaterialType)))
        {
            if (value != MaterialType.ACCESORIOS_ANIMADOS)
            {
                AddMaterial(value);
            }
        }
    }
    public void Init()
    {
        foreach (var item in multiMeshArrayShaders)
        {
            item.Value.ShowInfo();
        }
    }
    public void AddPendingRemove(PendingRemoveInstance pendingRemove)
    {
        pendingRemoveInstances.Enqueue(pendingRemove);
    }

    public void ProcessPendingRemove()
    {
        while (pendingRemoveInstances.TryDequeue(out var item))
        {
            FreeInstance(item.Rid, item.instance, item.materialId);
        }
    }

    private void AddMaterial(MaterialType materialType)
    {
        var tempo = new MultimeshArrayMaterial(materialType);
        multiMeshArrayShaders.Add(materialType, tempo);
    }
    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
 
    public void FreeInstance(Rid rid, int instance, int idMaterial)
    {
        MaterialType typeShader = (MaterialType)MaterialManager.Instance.GetMaterial(idMaterial).type;
        multiMeshArrayShaders[typeShader].FreeInstance(rid, instance, idMaterial);
    
    }
    public (Rid rid, int instance, int material, int layerTexture) CreateInstance( int idMaterial)
    {
        MaterialType typeShader = (MaterialType)MaterialManager.Instance.GetMaterial(idMaterial).type;
       // GD.Print("Tipo Material Usado:"+typeShader.ToString()+" ->" +idMaterial);
        return multiMeshArrayShaders[typeShader].CreateInstance(idMaterial);
    }

 
}

