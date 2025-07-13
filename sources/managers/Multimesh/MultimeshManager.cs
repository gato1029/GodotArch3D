using Godot;
using GodotEcsArch.sources.managers.Textures;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
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
        AddMaterial(MaterialType.GENERICO);
     
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
 
    public void FreeInstance(MaterialType typeShader,Rid rid, int instance, int idMaterial)
    {
        multiMeshArrayShaders[typeShader].FreeInstance(rid, instance, idMaterial);
    
    }
    public (Rid rid, int instance, int materialBatchPosition) CreateInstance(MaterialType typeShader, int idMaterial)
    {        
        return multiMeshArrayShaders[typeShader].CreateInstance(idMaterial);
    }
   
}

