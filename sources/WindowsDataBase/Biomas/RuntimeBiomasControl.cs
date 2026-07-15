using Godot;
using GodotEcsArch.sources.Helpers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Generic;

public partial class RuntimeBiomasControl : PanelContainer
{
    BiomaData data;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        KuroItems.ReloadObjects<BiomaData>();
        KuroItems.OnObjectPressed += KuroItems_OnObjectPressed;
        ButtonGuardar.Pressed += ButtonGuardar_Pressed;
        ButtonEliminar.Pressed += ButtonEliminar_Pressed;
        ButtonNuevo.Pressed += ButtonNuevo_Pressed;
        ButtonCaminos.Pressed += ButtonCaminos_Pressed;
        ButtonTerreno.Pressed += ButtonTerreno_Pressed;
        ButtonSuperficie.Pressed += ButtonSuperficie_Pressed;
        ClearAll();
    }

    private void ButtonSuperficie_Pressed()
    {
        WindowDataEditor w = RuntimeServices.NodeRegistry.Create<WindowDataEditor>();
        w.OnItemSelected += W_OnItemSelectedSuperficie;
        AddChild(w);
        w.Popup();
        w.LoadData<SuperficieData>();
    }

    private void W_OnItemSelectedSuperficie(object obj)
    {
        var item = (SuperficieData)obj;
        data.idSuperficie = item.id;
        ButtonSuperficie.Text = item.name;
    }

    private void ButtonTerreno_Pressed()
    {
        WindowDataEditor w = RuntimeServices.NodeRegistry.Create<WindowDataEditor>();
        w.OnItemSelected += W_OnItemSelectedTerreno;        
        AddChild(w);
        w.Popup();
        w.LoadData<TerrainBaseData>();
    }

    private void W_OnItemSelectedTerreno(object obj)
    {
        var item = ((TerrainBaseData)obj);
        data.idTerreno = item.id;
        ButtonTerreno.Text = item.name; 
    }

    private void ButtonCaminos_Pressed()
    {
        WindowDataEditor w = RuntimeServices.NodeRegistry.Create<WindowDataEditor>();
        w.OnItemSelected += W_OnItemSelectedCaminos;                
        AddChild(w);
        w.Popup();
        w.LoadData<CaminosData>();
    }

    private void W_OnItemSelectedCaminos(object obj)
    {
        var item = ((CaminosData)obj);
        data.idCamino = item.id;
        ButtonCaminos.Text = item.name;
    }

    private void ClearAll()
    {
        data = new BiomaData();
        LineEditName.Text = string.Empty;
        ButtonSuperficie.Text = "Seleccione Uno";
        ButtonCaminos.Text = "Seleccione Uno";
        ButtonTerreno.Text = "Seleccione Uno";
        ListaRecursos.ClearChildrens();
    }

    private void ButtonNuevo_Pressed()
    {
        ClearAll();

    }


    private void ButtonEliminar_Pressed()
    {
        KuroItems.RemoveObject(data);
        DataBaseManager.Instance.RemoveDirectById<BiomaData>(data.id);

    }

    private void ButtonGuardar_Pressed()
    {
        data.name = LineEditName.Text;
        data.idRecursos = ListaRecursos.GetData();        
        DataBaseManager.Instance.InsertUpdate(data);
        KuroItems.RefreshObject(data);
    }
    private void KuroItems_OnObjectPressed(object obj)
    {
        data = (BiomaData)obj;
        ButtonCaminos.Text = AtlasModsManager.Get<CaminosData>(data.nameMod, data.idCamino).name;

        LineEditName.Text = data.name;       
    }
}
