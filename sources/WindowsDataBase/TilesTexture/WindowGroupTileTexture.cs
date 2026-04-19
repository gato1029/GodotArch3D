using Godot;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.IO;
using static System.Formats.Asn1.AsnWriter;

public partial class WindowGroupTileTexture : Window
{
    MaterialData material;
    WindowSearchTileMaterial windowLocal = null;


    // ventana compartida de tiles
    private WindowSearchTileMaterial sharedWindow;
    private TileTextureRuleControl currentRequester;
    private TileTextureControl currentRequesterAlter;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI();
        KuroTextureButtonAdd.Pressed += KuroTextureButtonAdd_Pressed;
        KuroTextureButtonSearch.Pressed += KuroTextureButtonSearch_Pressed;
        Contenedor.ChildOrderChanged += () =>
        {
            CallDeferred(nameof(UpdatePositions));
        };

  
    }

    
    public void OpenMaterialWindow(TileTextureControl requester, Vector2I position)
    {
        currentRequesterAlter = requester;
        currentRequester = null;
        //sharedWindow.Popup();
        //sharedWindow.Position = position;
        

        if (requester.HasMaterial())
        {
            sharedWindow.SetSelection(requester.GetMaterialId(), requester.GetTileIndex());
        }
    }
    public void OpenMaterialWindow(TileTextureRuleControl requester, Vector2I position)
    {
        currentRequester = requester;
        currentRequesterAlter = null;
        //sharedWindow.Position = position;
        //sharedWindow.Popup();

        if (requester.HasMaterial())
        {
            sharedWindow.SetSelection(requester.GetMaterialId(), requester.GetTileIndex());
        }
    }
    private void OnMaterialSelected(int index, MaterialData materialData)
    {
        if (currentRequester != null) 
        {
            currentRequester.SetMaterialData(materialData.id, index);
        }                     
        if (currentRequesterAlter !=null)
        {
            currentRequesterAlter.SetMaterialData(materialData.id, index);
        }
    }

    private void KuroTextureButtonSearch_Pressed()
    {
        FacadeWindowDataSearch<MaterialData> windowQuery = new FacadeWindowDataSearch<MaterialData>("res://sources/WindowsDataBase/Materials/windowNewMaterial.tscn", this, WindowType.SELECTED);
        windowQuery.OnNotifySelected += WindowQuery_OnNotifySelected;        
        //windowLocal = GD.Load<PackedScene>("res://sources/KuroTiles/WindowSearchTileMaterial.tscn").Instantiate<WindowSearchTileMaterial>();
        //// Escuchar cuando se cierre
        //AddChild(windowLocal);
        //windowLocal.TreeExited += () => windowLocal = null;
        //windowLocal.OnNotifySelectionIndex += WindowLocal_OnNotifySelectionIndex;
        //windowLocal.Popup();
        
    }

    private void WindowQuery_OnNotifySelected(MaterialData objectSelected)
    {

        sharedWindow = GD.Load<PackedScene>("res://sources/KuroTiles/WindowSearchTileMaterial.tscn").Instantiate<WindowSearchTileMaterial>();

        AddChild(sharedWindow); // o GetTree().Root si quieres flotante global
        sharedWindow.SetAlwaysOpen();
        sharedWindow.Popup();
        //sharedWindow.Hide();

        sharedWindow.OnNotifySelectionIndex += OnMaterialSelected;

        material = objectSelected;
        LineEditMaterial.Text = material.name;
        sharedWindow.SetSelection(material.id, 0);
        //UpdateMaterial();
    }

    private void WindowLocal_OnNotifySelectionIndex(int index, MaterialData materialData)
    {
        throw new NotImplementedException();
    }

    private void KuroTextureButtonAdd_Pressed()
    {
        var scene = GD.Load<PackedScene>("res://sources/WindowsDataBase/TilesTexture/RuleTextureControl.tscn");         
        var widget = scene.Instantiate<RuleTextureControl>();        
        Contenedor.AddChild(widget);
        widget.SetGroupParent(this);
        
    }
    private void UpdateMaterial()
    {
        int index = 0;

        foreach (var child in Contenedor.GetChildren())
        {
            if (child is RuleTextureControl item)
            {
                item.SetMaterial(material.id);                
            }
        }
    }
    private void UpdatePositions()
    {
        int index = 0;

        foreach (var child in Contenedor.GetChildren())
        {
            if (child is RuleTextureControl item)
            {
                item.SetPosition(index);
                index++;
            }
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
