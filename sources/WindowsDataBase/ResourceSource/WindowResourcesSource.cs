using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;

public partial class WindowResourcesSource : Window, IFacadeWindow<ResourceSourceData>
{
    ResourceSourceData objectData;

    public event IFacadeWindow<ResourceSourceData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        objectData = new ResourceSourceData();

        ButtonSave.Pressed += ButtonSave_Pressed;
      
        foreach (ResourceSourceType tipo in Enum.GetValues(typeof(ResourceSourceType)))
        {
            OptionButtonType.AddItem(tipo.ToString());
        }
    }

    private void ButtonSave_Pressed()
    {
        objectData.name = LineEditName.Text;
        objectData.description = TextEditDescription.Text;
        objectData.amount = (int)SpinBoxAmount.Value;
        objectData.resourceSourceType = (ResourceSourceType)OptionButtonType.Selected;
        objectData.listIdTileSpriteData = ControlTileSprite.GetIdTiles();
        objectData.health = (int)SpinBoxHealthPoints.Value;
        objectData.isExploitable = CheckBoxIsExploitable.ButtonPressed;
        DataBaseManager.Instance.InsertUpdate<ResourceSourceData>(objectData);
        MasterDataManager.UpdateRegisterData(objectData.id, objectData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();

    }




    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    public void SetData(ResourceSourceData data)
    {
        objectData = data;
        LineEditName.Text = data.name;
        TextEditDescription.Text = data.description;
        SpinBoxAmount.Value = data.amount;
        OptionButtonType.Selected = (int)data.resourceSourceType;
        SpinBoxHealthPoints.Value = data.health;
        CheckBoxIsExploitable.ButtonPressed = data.isExploitable;
        ControlTileSprite.SetIdTiles(data.listIdTileSpriteData);

    }
}
