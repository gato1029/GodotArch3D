using Godot;
using GodotEcsArch.sources.managers.Textures;
using GodotEcsArch.sources.utils;
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

    private  Dictionary<MaterialType,List<MaterialData>> materialsByType = new Dictionary<MaterialType, List<MaterialData>>(); // esto solo es temporal para aglomerar por tipos
    private  Dictionary<string, MaterialModData> _textureLookup = new();
    private Dictionary<int, MaterialModData> materialsMods = new Dictionary<int, MaterialModData>(); // aqui se carga solo un material por mod
    private Dictionary<int, MaterialModData> subMaterialsModsGlobal = new Dictionary<int, MaterialModData>(); // aqui se carga todas las subtexturas y pertenencia

    //public void ForceChargue()
    //{
    //    GD.Print("🔄 Refreshing AtlasModsManager...");
    //    Initialize();
    //}
    protected override void Initialize()
    {
      
    }
    public void LoadCurrentMod()
    {
        
        string folderPath = FileHelper.GetPathGameDB("");
        string folderPathDb = FileHelper.GetPathGameDB("db/MiBaseDeDatos2.db");
        string modName = new DirectoryInfo(folderPath).Name;

        var modInfo = new ModInfo(modName, folderPathDb, folderPath);

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
    private void ChargueEditorMods()
    {
        // esta funcion se llama desde el editor, cada vez que se hace un cambio en los mods, para recargar todo y ver los cambios reflejados
        mods.Clear();
        materialsByType.Clear();
        _textureLookup.Clear();
        materialsMods.Clear();
        TableMods.Instance.Clear(); // limpio la tabla de mods para volver a cargar solo el mod actual, asi evito cargar mods que no estoy editando y tener que revisar cambios innecesarios
        textureArrayManager = new TextureArrayManager();
        renderManager = new RenderManagerOptimized(textureArrayManager);
        LoadCurrentMod();
        AtlasModsManager.Instance.ClearAll(); // limpio todos los manager de mods para volver a cargar solo el mod actual, asi evito cargar mods que no estoy editando y tener que revisar cambios innecesarios
        AtlasModsManager.Instance.FirstLoad(); // luego de que cargo la tabla de mods, genero todos los manager por mods
        CreateAllMaterials();
        CreateAtlasMaterial();
        CreateTexturesRendering();
        BuildRendering();
    }
    private void ChargueAllMods()
    {
        mods.Clear();
        materialsByType.Clear();
        _textureLookup.Clear();
        materialsMods.Clear();

        textureArrayManager = new TextureArrayManager();
        renderManager = new RenderManagerOptimized(textureArrayManager);
        LoadAllMods();
        AtlasModsManager.Instance.FirstLoad(); // luego de que cargo la tabla de mods, genero todos los manager por mods
        CreateAllMaterials();

        bool needRebuildAtlas = ReviewDiferences();

        if (needRebuildAtlas)
        {
            GD.Print("🔄 Rebuilding atlas...");
            CreateAtlasMaterial();
        }
        else
        {
            GD.Print("Atlas Sin modifcacion");
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
                    return true;

                if (mat.timeStamp != materialModData.timeStamp)
                {
                    return true; // 🔥 hubo cambios
                }
            }
        }

        return false; // no hubo cambios
    }
    internal IEnumerable<MaterialModData> GetAllMaterialsTextures()
    {
        return materialsMods.Values;
    }
    public MaterialModData GetMaterialTextureBySubId(int subTextureId)
    {
        return subMaterialsModsGlobal[subTextureId];
    }
    public MaterialModData GetMaterialTextureByAtlasId(int atlasId)
    {
        return materialsMods[atlasId];
    }
    public MaterialModData GetMaterialTexture(string textureIdMod)
    {
        if (!_textureLookup.TryGetValue(textureIdMod, out var idAtlasTexture))
        {
            GD.PrintErr($"❌ Texture no encontrada: {textureIdMod}");
            return null;
        }

        return idAtlasTexture;
    }
    public  (Rid rid, int instance,int layerTexture) CreateInstanceRender(string textureIdMod)
    {

        if (!_textureLookup.TryGetValue(textureIdMod, out var idAtlasTexture))
        {
            GD.PrintErr($"❌ Texture no encontrada: {textureIdMod}");
            return (default, -1, -1);
        }

        var (multimeshRid, instanceId, layer) = renderManager.CreateInstance(idAtlasTexture.idTextureAtlas);
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
            _textureLookup[item.idNameMod] = item;
            subMaterialsModsGlobal.Add(item.idSubTexture, item);
            if (!processed.Add(item.idTextureAtlas))
                continue; // ya existe → lo saltamos
            materialsMods.Add(item.idTextureAtlas, item);
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

            var data = AtlasModsManager.GetAll<MaterialData>(info.Name);
            LoadMaterials(info,data);
        }
    }

    private  void LoadMaterials(ModInfo info, IEnumerable<MaterialData> data)
    {
        foreach (var item in data)
        {
            
            if (item.type != MaterialType.ACCESORIOS_ANIMADOS)
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
    public void LoadAllMods()
    {
        var basePath = ProjectSettings.GlobalizePath(EditorWorldsPath);
        var files = GetDatabaseFiles(basePath, "db");

        foreach (var file in files)
        {
            // carpeta donde está el db
            string dbFolder = Path.GetDirectoryName(file);

            // carpeta superior = carpeta real del mod
            DirectoryInfo parentFolder = Directory.GetParent(dbFolder);

            if (parentFolder == null)
            {
                GD.PrintErr($"No se pudo resolver carpeta padre para: {file}");
                continue;
            }

            string folderPath = parentFolder.FullName;
            string modName = parentFolder.Name;

            if (modName == "Orquestador")
                continue;

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

    internal void FirstLoad(bool AllMods=true)
    {
        if (AllMods)
        {
            ChargueAllMods();
        }
        else
        {
            ChargueEditorMods(); 
        }
        
    }


}
