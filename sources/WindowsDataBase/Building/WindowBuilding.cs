using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using System;
using System.Linq;

public partial class WindowBuilding : Window, IFacadeWindow<BuildingData>
{
    public event IFacadeWindow<BuildingData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;
    BuildingData objectData;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        objectData = new BuildingData();
        foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
        {
            OptionButtonTypeBuilding.AddItem(type.ToString());
        }
        
        OptionButtonTypeBuilding.ItemSelected += OptionButtonTypeBuilding_ItemSelected;
        ButtonSave.Pressed += ButtonSave_Pressed;
        ControlSpriteBasico.OnNotifyChangued += ControlSpriteBasico_OnNotifyChangued;
        ControlBuildingGridBasic.OnNotifyChangued += ControlBuildingGridBasic_OnNotifyChangued;
    }

    private void ControlBuildingGridBasic_OnNotifyChangued(ControlBuildingGrid objectControl)
    {
        if (objectControl.SelectedBuildings.Count>0)
        {
            objectData.buildingPosition = objectControl.GetBuildingPosition();
        }
        
    }

    private void ControlSpriteBasico_OnNotifyChangued(ControlSprite objectControl)
    {
        ControlBuildingGridBasic.SetTexture(objectControl.GetSprite().Texture);
        ControlBuildingGridBasic.SetScaleTexture(objectControl.ObjectData.scale);
    }

    public void SetData(BuildingData data)
    {
        objectData = data;
        LineEditName.Text = objectData.name;
        TextEditDescription.Text = objectData.description;
        OptionButtonTypeBuilding.Selected = (int)objectData.buildingType;
        SpinBoxMaxHealth.Value = objectData.maxHealth;
        SpinBoxRangeAttack.Value = objectData.attackRange;
        SpinBoxChargueAttack.Value = objectData.attackCooldown;
        SpinBoxTimeBuild.Value = objectData.timeToBuild;
        ControlSpriteMiniatura.SetData(objectData.miniatura);
        ControlSpriteBasico.SetData(objectData.spriteData);
        ContainerAnimationBasico.SetData(objectData.animationData.ToArray());
        ControlBuildingGridBasic.SetTexture(ControlSpriteBasico.GetSprite().Texture);
        ControlBuildingGridBasic.SetBuildingPosition(objectData.buildingPosition);
    }
    private void ButtonSave_Pressed()
    {
        objectData.name = LineEditName.Text;
        objectData.description = TextEditDescription.Text;
        objectData.buildingType = (BuildingType)OptionButtonTypeBuilding.Selected;
        objectData.maxHealth = (int)SpinBoxMaxHealth.Value;
        objectData.attackRange = (int)SpinBoxRangeAttack.Value;
        objectData.attackCooldown = (int)SpinBoxChargueAttack.Value;
        objectData.timeToBuild = (int)SpinBoxTimeBuild.Value;
        objectData.miniatura = ControlSpriteMiniatura.ObjectData;
        objectData.spriteData = ControlSpriteBasico.ObjectData;
        objectData.animationData = ContainerAnimationBasico.GetData().ToList();

        if (objectData.miniatura.idMaterial == 0)
        {
            objectData.miniatura = null;
        }
        if (objectData.spriteData.idMaterial == 0)
        {
            objectData.spriteData = null;
        }
        if (objectData.animationData.Count > 0)
        {
            objectData.animationData = null;
        }             
        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    private void OptionButtonTypeBuilding_ItemSelected(long index)
    {
        BuildingType type = (BuildingType)index;
        switch (type)
        {
            case BuildingType.Ninguno:
                break;
            case BuildingType.ProductorMaterial:
                break;
            case BuildingType.ProductorUnidades:
                break;
            case BuildingType.TorreDefensa:
                break;
            case BuildingType.Procesador:
                break;
            default:
                break;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

  
}
