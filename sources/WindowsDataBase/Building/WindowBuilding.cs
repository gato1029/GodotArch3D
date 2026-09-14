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
    Vector2I sizeReal;
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
        sizeReal = this.Size;
    }    
    
    public void SetData(BuildingData data)
    {
        objectData = data;
        LineEditName.Text = objectData.name;
        TextEditDescription.Text = objectData.Description;
        OptionButtonTypeBuilding.Selected = (int)objectData.BuildingType;
        SpinBoxMaxHealth.Value = objectData.MaxHealth;
        SpinBoxRangeAttack.Value = objectData.AttackRange;
        SpinBoxChargueAttack.Value = objectData.AttackCooldown;
        SpinBoxTimeBuild.Value = objectData.TimeToBuild;
        ControlTileSpriteItem.SetIdTile(objectData.IdTileSpriteNormal);
        ControlProyectileItem.SetData(objectData.IdProjectile);

        if (objectData.DefensePowers!=null)
        {
            ControlDefensa.SetAllData(objectData.DefensePowers.ToArray());
        }
        if (objectData.AttackPowers!=null)
        {
            ControlAtaque.SetAllData(objectData.AttackPowers.ToArray());
        }
        
        OptionButtonTypeBuilding_ItemSelected(OptionButtonTypeBuilding.Selected);
    }
    private void ButtonSave_Pressed()
    {
        objectData.name = LineEditName.Text;
        objectData.Description = TextEditDescription.Text;
        objectData.BuildingType = (BuildingType)OptionButtonTypeBuilding.Selected;
        objectData.MaxHealth = (int)SpinBoxMaxHealth.Value;
        objectData.AttackRange = (float)SpinBoxRangeAttack.Value;
        objectData.AttackCooldown = (float)SpinBoxChargueAttack.Value;
        objectData.TimeToBuild = (float)SpinBoxTimeBuild.Value;
        objectData.IdTileSpriteNormal = ControlTileSpriteItem.GetidTile();       
        objectData.DefensePowers = ControlDefensa.GetAllData();
        objectData.AttackPowers = ControlAtaque.GetAllData();
        objectData.IdProjectile = ControlProyectileItem.GetData();
        DataBaseManager.Instance.InsertUpdate(objectData);
        
        MasterDataManager.UpdateRegisterData(objectData.id, objectData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    private void OptionButtonTypeBuilding_ItemSelected(long index)
    {
        
        Vector2I sizeAtaque = new Vector2I(300,0);

        BuildingType type = (BuildingType)index;
        switch (type)
        {
            case BuildingType.Ninguno:
                this.Size = sizeReal;
                ContainerAtaque.Visible = false;
                break;
            case BuildingType.ProductorMaterial:
                this.Size = sizeReal;
                ContainerAtaque.Visible = false;
                break;
            case BuildingType.ProductorUnidades:
                this.Size = sizeReal;
                ContainerAtaque.Visible = false;
                break;
            case BuildingType.Torres:     
                this.Size = sizeReal+sizeAtaque;
                ContainerAtaque.Visible = true;
                break;
            case BuildingType.Procesador:
                this.Size = sizeReal;
                ContainerAtaque.Visible = false;
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
