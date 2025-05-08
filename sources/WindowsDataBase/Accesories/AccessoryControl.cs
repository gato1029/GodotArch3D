using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;
using System.Linq;

public partial class AccessoryControl : Window
{
        

    AccessoryData objectData;

    ContainerAnimationBody controlAnimationBody;

    ContainerAnimationTiles controlAnimationTiles;

    ContainerRequirements controlRequirements;


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        

        ButtonDelete.Pressed += ButtonDelete_Pressed;
        ButtonSave.Pressed += ButtonSave_Pressed;

        foreach (AccesoryClassType type in Enum.GetValues(typeof(AccesoryClassType)))
        {
            OptionButtonClassAccesory.AddItem(type.ToString());
        }
        foreach (AccesoryType type in Enum.GetValues(typeof(AccesoryType)))
        {
            OptionButtonTypeAccesory.AddItem(type.ToString());
        }
        
        foreach (AccesoryBodyPartType type in Enum.GetValues(typeof(AccesoryBodyPartType)))
        {
            OptionButtonBodyAccesory.AddItem(type.ToString());
        }

       CheckBoxAnimationBody.Pressed += CheckBoxAnimationBody_Pressed;
       CheckBoxAnimationTiles.Pressed += CheckBoxAnimationTiles_Pressed;
        CheckBoxRequeriment.Pressed += CheckBoxRequeriment_Pressed;

        objectData = new AccessoryData();
    }
    internal void SetData(AccessoryData p_ObjectData)
    {
        objectData = p_ObjectData;
        var components = p_ObjectData.colorBase.Trim('(', ')')
                            .Split(',')
                            .Select(s => float.Parse(s.Trim()))
                            .ToArray();

        // Color base
        ColorPickerButtonColorBase.Color = new Color(components[0], components[1], components[2], components[3]);

        // Texto
        LineEditName.Text = p_ObjectData.name ?? "";
        TextEditDescription.Text = p_ObjectData.description ?? "";

        // Miniatura
        ControlMiniatura.SetData(p_ObjectData.miniatureData);
        
        // Enums
        OptionButtonClassAccesory.Select((int)p_ObjectData.accesoryClassType);
        OptionButtonBodyAccesory.Select((int)p_ObjectData.accesoryBodyPartType);
        OptionButtonTypeAccesory.Select((int)p_ObjectData.accesoryType);

        // Checkboxes
        CheckBoxAnimationTiles.ButtonPressed = p_ObjectData.hasAnimationTile;
        CheckBoxAnimationBody.ButtonPressed = p_ObjectData.hasBodyAnimation;
        CheckBoxRequeriment.ButtonPressed = p_ObjectData.hasRequirements;

        // Animaciones
        if (p_ObjectData.hasAnimationTile && p_ObjectData.animationTilesData != null)
        {
            controlAnimationTiles = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/ContainerAnimationTiles.tscn").Instantiate<ContainerAnimationTiles>();
            controlAnimationTiles.Name = "Control Animacion Tiles";
            TabContainerControl.AddChild(controlAnimationTiles);
            controlAnimationTiles.SetData( p_ObjectData.animationTilesData);
        }

        if (p_ObjectData.hasBodyAnimation && p_ObjectData.idBodyAnimationBaseData > 0)
        {
            controlAnimationBody = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/ContainerAnimationBody.tscn").Instantiate<ContainerAnimationBody>();
            controlAnimationBody.Name = "Control Animacion Body";
            TabContainerControl.AddChild(controlAnimationBody);

            controlAnimationBody.LoadById(p_ObjectData.idBodyAnimationBaseData);
        }

        // Requerimientos
        if (p_ObjectData.hasRequirements && p_ObjectData.requirementsData != null)
        {
            controlRequirements = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/ContainerRequirements.tscn").Instantiate<ContainerRequirements>();
            controlRequirements.Name = "Requisitos";
            TabContainerControl.AddChild(controlRequirements);
            controlRequirements.SetRequirementsData(p_ObjectData.requirementsData);
        }

        // Defensa
        ControlDefensa.SetAllData(p_ObjectData.defenseDataArray ?? null);

        // Ataque
        ControlAtaque.SetAllData(p_ObjectData.damageDataArray ?? null);

        // Stats
        ControlStats.SetAllData(p_ObjectData.statsDataArray ?? null);

        // Bonificaciones
        ControlBonusBase.SetAllData(p_ObjectData.bonusDataArray ?? null);
    }

    public void SaveAll()
    {

        
        objectData.colorBase = ColorPickerButtonColorBase.Color.ToString();
        objectData.name = LineEditName.Text;
        objectData.description = TextEditDescription.Text;

        objectData.miniatureData = ControlMiniatura.ObjectData;
        objectData.accesoryClassType = (AccesoryClassType)OptionButtonClassAccesory.GetSelectedId();
        objectData.accesoryBodyPartType = (AccesoryBodyPartType)OptionButtonBodyAccesory.GetSelectedId();
        objectData.accesoryType = (AccesoryType)OptionButtonTypeAccesory.GetSelectedId();
   
        objectData.hasAnimationTile = CheckBoxAnimationTiles.ButtonPressed;
        objectData.hasBodyAnimation = CheckBoxAnimationBody.ButtonPressed;
        objectData.hasRequirements = CheckBoxRequeriment.ButtonPressed;

        if (objectData.hasAnimationTile)
        {
            objectData.animationTilesData = controlAnimationTiles.ObjectData;
        }
        else 
        {
            objectData.animationTilesData = null;
        }
        if (objectData.hasBodyAnimation)
        {
            objectData.idBodyAnimationBaseData = controlAnimationBody.ObjectData.id;
        }
        else
        {
            objectData.idBodyAnimationBaseData = 0;
        }

        if (objectData.hasRequirements)
        {
            objectData.requirementsData = controlRequirements.GetRequirementsData();
        }
        else
        {
            objectData.requirementsData = null;
        }

        if (ControlDefensa.GetAllStats().Count>0)
        {
            objectData.defenseDataArray = ControlDefensa.GetAllStats().ToArray();
        }
        else
        {
            objectData.defenseDataArray = null;
        }

        if (ControlAtaque.GetAllStats().Count > 0)
        {
            objectData.damageDataArray = ControlAtaque.GetAllStats().ToArray();
        }
        else
        {
            objectData.damageDataArray = null;
        }

        if (ControlStats.GetAllStats().Count > 0)
        {
            objectData.statsDataArray = ControlStats.GetAllStats().ToArray();
        }
        else
        {
            objectData.statsDataArray = null;
        }

        if (ControlBonusBase.GetAllStats().Count > 0)
        {
            objectData.bonusDataArray = ControlBonusBase.GetAllStats().ToArray();
        }
        else
        {
            objectData.bonusDataArray = null;
        }
              
    }

    private void ButtonDelete_Pressed()
    {
        DataBaseManager.Instance.RemoveById<AccessoryData>(objectData.id);
        OnNotifyChangued?.Invoke(this);
        QueueFree();
    }

    private void ButtonSave_Pressed()
    {
        SaveAll();
        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChangued?.Invoke(this);
        QueueFree();
    }

    private void CheckBoxAnimationBody_Pressed()
    {
        if (CheckBoxAnimationBody.ButtonPressed)
        {

            controlAnimationBody = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/ContainerAnimationBody.tscn").Instantiate<ContainerAnimationBody>();
            controlAnimationBody.Name = "Control Animacion Body";
            TabContainerControl.AddChild(controlAnimationBody);
        }
        else
        {
            TabContainerControl.RemoveChild(controlAnimationBody);
        }
    }

    private void CheckBoxAnimationTiles_Pressed()
    {
        if (CheckBoxAnimationTiles.ButtonPressed)
        {

            controlAnimationTiles = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/ContainerAnimationTiles.tscn").Instantiate<ContainerAnimationTiles>();
            controlAnimationTiles.Name = "Control Animacion Tiles";
            TabContainerControl.AddChild(controlAnimationTiles);
        }
        else
        {
            TabContainerControl.RemoveChild(controlAnimationTiles);
        }
    }

    private void CheckBoxRequeriment_Pressed()
    {
        if (CheckBoxRequeriment.ButtonPressed)
        {
            controlRequirements = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/ContainerRequirements.tscn").Instantiate<ContainerRequirements>();
            controlRequirements.Name = "Requisitos";
            TabContainerControl.AddChild(controlRequirements);
        }
        else
        {
            TabContainerControl.RemoveChild(controlRequirements);
        }
    }

  
}
