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
        TextEditDescription.Text = objectData.description;
        OptionButtonTypeBuilding.Selected = (int)objectData.buildingType;
        SpinBoxMaxHealth.Value = objectData.maxHealth;
        SpinBoxRangeAttack.Value = objectData.attackRange;
        SpinBoxChargueAttack.Value = objectData.attackCooldown;
        SpinBoxTimeBuild.Value = objectData.timeToBuild;
        ControlTileSpriteItem.SetIdTile(objectData.idTileSpriteNormal);
        ControlProyectileItem.SetData(objectData.idProyectile);

        if (objectData.defensePowers!=null)
        {
            ControlDefensa.SetAllData(objectData.defensePowers.ToArray());
        }
        if (objectData.attackPowers!=null)
        {
            ControlAtaque.SetAllData(objectData.attackPowers.ToArray());
        }
        
        OptionButtonTypeBuilding_ItemSelected(OptionButtonTypeBuilding.Selected);
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
        objectData.idTileSpriteNormal = ControlTileSpriteItem.GetidTile();       
        objectData.defensePowers = ControlDefensa.GetAllData();
        objectData.attackPowers = ControlAtaque.GetAllData();
        objectData.idProyectile = ControlProyectileItem.GetData();
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
            case BuildingType.TorreDefensa:     
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
