using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Generic;

public partial class RuntimeEditorWindowMaterial : Window
{
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI

        ComboBoxTipos.ItemSelected += ComboBoxTipos_ItemSelected;
        
        CargarMods();

    }

    private void ComboBoxTipos_ItemSelected(long index)
    {
        // ModInfo data = ComboBoxTipos.GetItemMetadata((int)index).As<ModInfo>();
        //GD.Print($"Seleccionaste: {data.Value.Name}" );
    }

    private void CargarMods()
    {
      var listMods = TableMods.Instance.ObtenerTodos();
        foreach (System.Collections.Generic.KeyValuePair<byte, ModInfo> item in listMods)
        {
            ComboBoxTipos.AddItem(item.Value.Name); // Texto visible
            int index = ComboBoxTipos.GetItemCount() - 1;

            // Guardamos el objeto completo como metadata
            ComboBoxTipos.SetItemMetadata(index, Variant.From(item.Value));
        }
    }
}
