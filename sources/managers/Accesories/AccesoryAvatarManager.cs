using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Accesories;
internal class AccesoryAvatarManager : SingletonBase<AccesoryAvatarManager>
{
    Dictionary<AccesoryAvatarType, MultimeshMaterial> multimeshMaterialDict;
    Dictionary<AccesoryAvatarType, int> accesoryDict;
    Dictionary<AccesoryAvatarType, GpuInstance> instanceDict;
    protected override void Initialize()
    {
        multimeshMaterialDict = new Dictionary<AccesoryAvatarType, MultimeshMaterial>();
        accesoryDict = new Dictionary<AccesoryAvatarType, int>();
        instanceDict = new Dictionary<AccesoryAvatarType, GpuInstance>();
    }
    public void RegisterAccesory(AccesoryAvatarType accesoryAvatarType, int idAccesory)
    {
        if (!multimeshMaterialDict.ContainsKey(accesoryAvatarType))
        {            
            var data = AccesoryManager.Instance.GetAccesory(idAccesory);            
            MultimeshMaterial multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(data.accesoryAnimationBodyData.animationStateData.idMaterial),1);            
            multimeshMaterialDict.Add(accesoryAvatarType, multimeshMaterial);
            accesoryDict.Add(accesoryAvatarType,idAccesory);
            var instance = multimeshMaterial.CreateInstance();
            GpuInstance gpuInstance;
            gpuInstance.rid = instance.Item1;
            gpuInstance.instance = instance.Item2;
            instanceDict.Add(accesoryAvatarType, gpuInstance);
        }
      

    }

    public GpuInstance ChangueAccesory(AccesoryAvatarType accesoryAvatarType, int idAccesory)
    {
        if (!multimeshMaterialDict.ContainsKey(accesoryAvatarType))
        {
            RegisterAccesory(accesoryAvatarType, idAccesory);
        }
        else
        {
            if (accesoryDict[accesoryAvatarType] != idAccesory)
            {
                accesoryDict[accesoryAvatarType] = idAccesory;

                var data = AccesoryManager.Instance.GetAccesory(idAccesory);
                var dataMaterial = DataBaseManager.Instance.FindById<MaterialSimpleData>(data.accesoryAnimationBodyData.animationStateData.idMaterial);

                MultimeshMaterial dataMultimesh = multimeshMaterialDict[accesoryAvatarType];
                Texture2D texture2D = (Texture2D)TextureHelper.LoadTextureLocal(FileHelper.GetPathGameDB(dataMaterial.pathTexture));
                dataMultimesh.materialData.textureMaterial = texture2D;


                dataMultimesh.materialData.shaderMaterial.SetShaderParameter("main_texture", dataMultimesh.materialData.textureMaterial);
                dataMultimesh.materialData.shaderMaterial.SetShaderParameter("atlas_width", texture2D.GetWidth());
                dataMultimesh.materialData.shaderMaterial.SetShaderParameter("atlas_height", texture2D.GetHeight());

                //mesh = MeshCreator.CreateSquareMesh(16, 16, new Vector2(divisionPixelX, divisionPixelY), new Vector3(0, 0, 0));
                //mesh.SurfaceSetMaterial(0, shaderMaterial);
            }
        }
        return instanceDict[accesoryAvatarType];
    }
}
