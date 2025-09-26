using Godot;
using GodotEcsArch.sources.utils;
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
        CheckBoxIsAnimated.Pressed += CheckBoxIsAnimated_Pressed;
    }
    ContainerAnimation ContainerAnimationBasico = null;
    ScrollContainer scrollContainer;

    

    private void CheckBoxIsAnimated_Pressed()
    {
        if (CheckBoxIsAnimated.ButtonPressed)
        {
            scrollContainer = new ScrollContainer();
            scrollContainer.Name = "Animacion";
            scrollContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            ContainerAnimationBasico = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/ContainerAnimation.tscn").Instantiate<ContainerAnimation>();
            ContainerAnimationBasico.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            ContainerAnimationBasico.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            scrollContainer.AddChild(ContainerAnimationBasico);
            TabContainerBase.AddChild(scrollContainer);

        }
        else
        {
            TabContainerBase.RemoveChild(scrollContainer);
            scrollContainer = null;
            ContainerAnimationBasico = null;
        }
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
        // ControlSpriteMiniatura.SetData(objectData.miniatura);
        ControlSpriteBasico.SetData(objectData.spriteData);
        CheckBoxIsAnimated.ButtonPressed = objectData.isAnimated;

        if (objectData.isAnimated) 
        { 
            ContainerAnimationBasico.SetData(objectData.animationData.ToArray());
        }
        ControlBuildingGridBasic.SetTexture(ControlSpriteBasico.GetSprite().Texture);
        ControlBuildingGridBasic.SetBuildingPosition(objectData.buildingPosition);
        
        ControlTower();
        if (objectData.buildingType == BuildingType.TorreDefensa)
        {
            ControlBulletSprite.SetData(data.spriteBullet);            
        }
    }
    private void ButtonSave_Pressed()
    {
        objectData.name = LineEditName.Text;
        objectData.description = TextEditDescription.Text;
        objectData.buildingType = (BuildingType)OptionButtonTypeBuilding.Selected;
        objectData.maxHealth = (int)SpinBoxMaxHealth.Value;
        objectData.attackRange = (float)SpinBoxRangeAttack.Value;
        objectData.attackCooldown = (float)SpinBoxChargueAttack.Value;
        objectData.timeToBuild = (float)SpinBoxTimeBuild.Value;
      //  objectData.miniatura = ControlSpriteMiniatura.ObjectData;
        objectData.spriteData = ControlSpriteBasico.ObjectData;
      
        objectData.isAnimated = CheckBoxIsAnimated.ButtonPressed;

        //if (objectData.miniatura.idMaterial == 0)
        //{
        //    objectData.miniatura = null;
        //}
        if (objectData.spriteData.idMaterial == 0)
        {
            objectData.spriteData = null;
        }
        if (objectData.isAnimated && objectData.animationData.Count > 0)
        {
            objectData.animationData = ContainerAnimationBasico.GetData().ToList();
        }
        if (objectData.buildingType == BuildingType.TorreDefensa)
        {
            objectData.spriteBullet = ControlBulletSprite.ObjectData;
            TextureHelper.RecalulateUVFormat(objectData.spriteBullet);
        }
        TextureHelper.RecalulateUVFormat(objectData.spriteData);
        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    private void OptionButtonTypeBuilding_ItemSelected(long index)
    {
        ClearControls();
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
                ControlTower();
                break;
            case BuildingType.Procesador:
                break;
            default:
                break;
        }

    }
    ControlSprite ControlBulletSprite = null;
    TabBar tabBullet =null;

    private void ClearControls()
    {
        if (tabBullet!=null)
        {
            TabContainerBase.RemoveChild(tabBullet);
            tabBullet = null;
            ControlBulletSprite = null;
        }        
    }
    private void ControlTower()
    {
        tabBullet = new TabBar();
        tabBullet.Name = "Proyectil";
        tabBullet.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        ControlBulletSprite = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/ControlSprite.tscn").Instantiate<ControlSprite>();
        ControlBulletSprite.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        ControlBulletSprite.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        tabBullet.AddChild(ControlBulletSprite);
        TabContainerBase.AddChild(tabBullet);

 
       
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

  
}
