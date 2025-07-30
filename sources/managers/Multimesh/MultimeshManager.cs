using Godot;
using GodotEcsArch.sources.managers.Textures;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.HttpRequest;

namespace GodotEcsArch.sources.managers.Multimesh;
public class MultimeshManager:SingletonBase<MultimeshManager>
{
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
    public (Rid rid, int instance, int materialBatchPosition) CreateInstance( int idMaterial)
    {
        MaterialType typeShader = (MaterialType)MaterialManager.Instance.GetMaterial(idMaterial).type;
        return multiMeshArrayShaders[typeShader].CreateInstance(idMaterial);
    }

 
}

