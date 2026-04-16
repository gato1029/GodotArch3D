using Godot;
using GodotEcsArch.sources.managers.Textures;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


namespace GodotEcsArch.sources.managers.Mods;

/// <summary>
/// Aqui cargaremos todos las texturas de los mods, y crearemos los atlas. Tambien se encargara de crear las instancias de render para cada textura.
/// </summary>
/// 
public class AtlasTexturesModsManager: SingletonBase<AtlasTexturesModsManager> 
{
    private  string EditorWorldsPath = "D:\\GitKraken\\ModsGame\\Mods";
    private  Dictionary<string, ModInfo> mods = new();
    private  TextureArrayManager textureArrayManager;
    private  RenderManagerOptimized renderManager;

    private  Dictionary<MaterialType,List<MaterialData>> materialsByType = new Dictionary<MaterialType, List<MaterialData>>();
    private  Dictionary<string, int> _textureLookup = new();   

    //public void ForceChargue()
    //{
    //    GD.Print("🔄 Refreshing AtlasModsManager...");
    //    Initialize();
    //}
    protected override void Initialize()
    {
        mods.Clear();
        materialsByType.Clear();
        _textureLookup.Clear();

        textureArrayManager = new TextureArrayManager();
        renderManager = new RenderManagerOptimized(textureArrayManager);
        LoadAllMods();
        CreateAllMaterials();

        bool needRebuildAtlas = ReviewDiferences();

        if (needRebuildAtlas)
        {
            GD.Print("🔄 Rebuilding atlas...");
            CreateAtlasMaterial();
        }
        CreateTexturesRendering();
        BuildRendering();
    }
 
    private  bool ReviewDiferences()
    {
        foreach (var item in materialsByType)
        {
            foreach (var mat in item.Value)
            {
                var materialModData = MaterialModDbService.Instance.Obtener(mat.idNameMod);

                if (materialModData == null)
                    continue;

                if (mat.timeStamp != materialModData.timeStamp)
                {
                    return true; // 🔥 hubo cambios
                }
            }
        }

        return false; // no hubo cambios
    }

    public  (Rid rid, int instance,int layerTexture) CreateInstanceRender(string textureIdMod)
    {

        if (!_textureLookup.TryGetValue(textureIdMod, out var idAtlasTexture))
        {
            GD.PrintErr($"❌ Texture no encontrada: {textureIdMod}");
            return (default, -1, -1);
        }

        var (multimeshRid, instanceId, layer) = renderManager.CreateInstance(idAtlasTexture);
        if (instanceId == -1)
        {
            GD.PrintErr($"No se pudo crear instancia para textureId: {textureIdMod}");
            
        }
        return (multimeshRid, instanceId,layer);
    }
    public  void FreeInstance(Rid Multimesh, int instanceId)
    {
        renderManager.FreeInstance(Multimesh, instanceId);
    }
    private  void BuildRendering()
    {
        if (_textureLookup.Count == 0)
        {
            GD.PrintErr("❌ No hay texturas para construir");
            return;
        }
        textureArrayManager.BuildAll();
        renderManager.Build();
    }
    private  void CreateTexturesRendering()
    {

        var data = MaterialModDbService.Instance.ObtenerTodos();

        HashSet<int> processed = new HashSet<int>();

        foreach (var item in data)
        {
            _textureLookup[item.idNameMod] = item.idTextureAtlas;

            if (!processed.Add(item.idTextureAtlas))
                continue; // ya existe → lo saltamos
            var pathComplete = FileHelper.GetPathGameDB(item.pathTextureAtlas);
            textureArrayManager.SetTexture(item.idTextureAtlas, pathComplete);
        }

       
    }

    private  void CreateAtlasMaterial()
    {
        foreach (var item in materialsByType)
        {
            AtlasTextureGenerator.Instance.GenerateMasterAtlas(item.Value, item.Key);
        }
    }

    public  void CreateAllMaterials()
    {
        foreach (var item in mods)
        {
            var info = item.Value;
            DataBaseManager.Instance.LoadCustomDataBase(info.DbPath);
            var data= DataBaseManager.Instance.FindAll<MaterialData>();
            LoadMaterials(info,data);
        }
    }

    private  void LoadMaterials(ModInfo info, List<MaterialData> data)
    {
        foreach (var item in data)
        {
            
            if (item.type != (int)MaterialType.ACCESORIOS_ANIMADOS)
            {
                if (!materialsByType.ContainsKey((MaterialType)item.type))
                {// crear la lista
                    materialsByType.Add((MaterialType)item.type, new List<MaterialData>());
                }
                item.idNameMod = info.Name + ":" + item.id;
                materialsByType[(MaterialType)item.type].Add(item);
            }
            else
            {
                // si es un accesorio animado, lo cargamos directamente en el render manager
                // esto me queda pendiente de como hacerlo // REVISAR LUEGO OJO
            }
        }
    }
    public  void LoadAllMods()
    {
        var basePath = ProjectSettings.GlobalizePath(EditorWorldsPath);
        var files = GetDatabaseFiles(basePath, "db");

        foreach (var file in files)
        {
            string folderPath = Path.GetDirectoryName(file);
            string modName = new DirectoryInfo(folderPath).Name;

            var modInfo = new ModInfo(modName, file, folderPath);

            if (!mods.ContainsKey(modName))
            {
                mods.Add(modName, modInfo);
                TableMods.Instance.Registrar(modInfo);
                GD.Print($"Mod cargado: {modName}");
            }
            else
            {
                GD.PrintErr($"Mod duplicado: {modName}");
            }
        }
        TableMods.Instance.FinalizarCarga();
    }
    public  List<string> GetDatabaseFiles(string folderPath, params string[] extensions)
    {
        var result = new List<string>();

        if (!Directory.Exists(folderPath))
        {
            GD.PrintErr($"Carpeta no existe: {folderPath}");
            return result;
        }

        foreach (var ext in extensions)
        {
            var files = Directory.GetFiles(folderPath, $"*.{ext}", SearchOption.AllDirectories);
            result.AddRange(files);
        }

        return result;
    }

    internal void FirstLoad()
    {
        
    }
}
