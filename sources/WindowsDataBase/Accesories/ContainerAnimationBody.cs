using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class ContainerAnimationBody : PanelContainer
{
    AnimationBaseData objectData;
    WindowViewDb windowViewDb;

    public AnimationBaseData ObjectData { get => objectData; set => objectData = value; }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI();
        ButtonBuscar.Pressed += ButtonBuscar_Pressed;
    }

    internal void LoadById(int idBodyAnimationBaseData)
    {
        WindowViewDb_OnRequestSelectedItem(idBodyAnimationBaseData);
    }

    private void ButtonBuscar_Pressed()
    {   
        windowViewDb = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowViewDB.tscn").Instantiate<WindowViewDb>();        
        AddChild(windowViewDb);
        windowViewDb.Show();
        windowViewDb.EnableFilter(false);
        windowViewDb.LoadItems<AnimationBaseData>();
        windowViewDb.OnRequestSelectedItem += WindowViewDb_OnRequestSelectedItem;
      
    }

  

    private void WindowViewDb_OnRequestSelectedItem(int id)
    {
        objectData = DataBaseManager.Instance.FindById<AnimationBaseData>(id);
        var data = objectData.animationDataArray[0];
        

        
        

        Godot.Image image = TextureHelper.LoadImageLocal(FileHelper.GetPathGameDB(objectData.pathTexture));
        Texture2D texture2D = (Texture2D)TextureHelper.LoadTextureLocal(FileHelper.GetPathGameDB(objectData.pathTexture));

        MaterialData materialData = new MaterialData();
        materialData.id = -1;
        materialData.name = "temporal";
        materialData.pathTexture = objectData.pathTexture;
        materialData.heightTexture = image.GetHeight();
        materialData.widhtTexture = image.GetWidth();
        materialData.divisionPixelX = objectData.divisionPixelX;
        materialData.divisionPixelY = objectData.divisionPixelY;
        materialData.textureMaterial = texture2D;

        AnimacionScene.SetData(data, -1);
    }
}
