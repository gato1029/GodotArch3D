using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System.Linq;

public partial class RuntimeEditorWindowMaterial : Window
{
    public delegate void EventOnSelection(MaterialData materialData);
    public event EventOnSelection OnSelection;

    [Export] public MaterialType materialType = MaterialType.TERRENO;

    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        AtlasTexturesModsManager.Instance.FirstLoad();
        ListaTipos.OnDataSelected += ListaTipos_OnDataSelected;
        ItemsList.OnDataSelected += ItemsList_OnDataSelected;
        CargarMods();

    }

    public void SetTipoTexturas(MaterialType materialType)
    {
        this.materialType = materialType;
    }
    private void ItemsList_OnDataSelected(object obj)
    {
        var data = (MaterialData)obj;
        OnSelection?.Invoke(data);
        QueueFree();
    }

    private void ListaTipos_OnDataSelected(object obj)
    {
        CargarMaterialTexturas((ModInfo)obj);
    }
    private void CargarMaterialTexturas(ModInfo modInfo)
    {
        var items = AtlasModsManager.GetAll<MaterialData>(modInfo.Name);
        foreach (var item in items)
        {
            if (item.type == materialType)
            {
                ItemsList.AddItemWithData(item.name, item, item.textureVisual);
            }            
        }
        
    }

    

    private void CargarMods()
    {
        var listMods = TableMods.Instance.ObtenerTodos().ToList();
        foreach (System.Collections.Generic.KeyValuePair<byte, ModInfo> item in listMods)
        {
            ListaTipos.AddItemWithData(item.Value.Name, item.Value);
        }
        CargarMaterialTexturas(listMods[0].Value);
    }
}
