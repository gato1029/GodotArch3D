using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Projectile.DataBase;
using System;
using System.Linq;

public partial class WindowProjectile : Window, IFacadeWindow<BulletData>
{   
    BulletData objectData;
    public event IFacadeWindow<BulletData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        objectData = new BulletData();       
        ButtonSave.Pressed += ButtonSave_Pressed;
    }
  
    public void SetData(BulletData data)
    {
        objectData = data;
        LineEditName.Text = objectData.name;

        ControlTileSpriteItem.SetIdTile(objectData.idTileSprite);
    }
    private void ButtonSave_Pressed()
    {
        objectData.name = LineEditName.Text;
        objectData.idTileSprite = ControlTileSpriteItem.GetidTile();        
        DataBaseManager.Instance.InsertUpdate(objectData);
        MasterDataManager.UpdateRegisterData(objectData.id, objectData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }
   
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
