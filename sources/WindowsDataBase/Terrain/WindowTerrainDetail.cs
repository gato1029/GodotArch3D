
using Godot;
using GodotEcsArch.sources.managers.Terrain;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Data;
using System.Linq;

public partial class WindowTerrainDetail : Window, IFacadeWindow<TerrainData>
{

    TerrainData objectData;

    public event IFacadeWindow<TerrainData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        objectData = new TerrainData();

        ButtonSave.Pressed += ButtonSave_Pressed;
        ButtonSaveActive.Pressed += ButtonSaveActive_Pressed;
        ButtonDuplicate.Pressed += ButtonDuplicate_Pressed;
        CheckBoxHasAnimation.Pressed += CheckBoxHasAnimation_Pressed;
        CheckBoxHasRule.Pressed += CheckBoxHasRule_Pressed;
        foreach (TerrainType tipo in Enum.GetValues(typeof(TerrainType)))
        {
            OptionButtonType.AddItem(tipo.ToString());
        }
        foreach (TerrainCategoryType tipo in Enum.GetValues(typeof(TerrainCategoryType)))
        {
            OptionButtonCategory.AddItem(tipo.ToString());
        }
    }

    private void ButtonDuplicate_Pressed()
    {
        objectData.id = 0;
        Save();
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    ControlRules controlRules;
    private void CheckBoxHasRule_Pressed()
    {
        if (CheckBoxHasRule.ButtonPressed)
        {

            controlRules = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/Controls/ControlRules.tscn").Instantiate<ControlRules>();
            controlRules.Name = "Regla Dinamica";
            TabContainerBase.AddChild(controlRules);
            controlRules.OnNotifyChangued += ControlRules_OnNotifyChangued;
        }
        else
        {
            TabContainerBase.RemoveChild(controlRules);
            controlRules = null;
        }
    }

    private void ControlRules_OnNotifyChangued(ControlRules objectControl)
    {
        Save();
    }

    ContainerAnimation ContainerAnimationBasico = null;
    ScrollContainer scrollContainer;
    private void CheckBoxHasAnimation_Pressed()
    {
        if (CheckBoxHasAnimation.ButtonPressed)
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


    private void Save()
    {

        objectData.spriteData = ControlSprite.ObjectData;
        objectData.isAnimated = CheckBoxHasAnimation.ButtonPressed;
        objectData.isRule = CheckBoxHasRule.ButtonPressed;
        objectData.category = OptionButtonCategory.GetItemText(OptionButtonCategory.Selected);
        objectData.terrainType = (TerrainType)OptionButtonType.Selected;

        if (!objectData.isRule)
        {
            string code = objectData.spriteData.x.ToString() + objectData.spriteData.y.ToString() + objectData.spriteData.widht.ToString() + objectData.spriteData.height.ToString();
            objectData.name = OptionButtonCategory.GetItemText(OptionButtonCategory.Selected) + LineEditName.Text + "_" + StableHash.FromString(code).ToString();
        }
        else
        {
            objectData.name = LineEditName.Text;
        }
        
        if (objectData.isAnimated)
        {
            objectData.animationData = ContainerAnimationBasico.GetData();
        }
        else
        {
            objectData.animationData = null;
        }
        if (objectData.isRule)
        {
            objectData.rules = controlRules.GetData().ToArray();
        }
        else
        {
            objectData.rules = null;
        }
        TextureHelper.RecalulateUVFormat(objectData.spriteData);      
        DataBaseManager.Instance.InsertUpdate(objectData);
        TerrainManager.Instance.RegisterUpdateData(objectData.id,objectData);
        
        
    }
    private void ButtonSaveActive_Pressed()
    {
        objectData = new TerrainData();
        Save();
        OnNotifyChanguedSimple?.Invoke();
    }
    private void ButtonSave_Pressed()
    {
        Save();
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    private void RegulationRules(RuleData[] rules)
    {
        if (rules != null)
        {
            foreach (var item in rules)
            {
                if (item.idDataCentral!=0)
                {
                    item.dataCentral = TerrainManager.Instance.GetData(item.idDataCentral).spriteData;
                }
                else
                {
                    item.dataCentral = null;
                }

                for (int i = 0; i < item.neighborConditions.Length; i++)
                {
                    NeighborCondition itemNeigh = item.neighborConditions[i];
                    if (itemNeigh.SpecificTileId != 0)
                    {
                        var sprite = TerrainManager.Instance.GetData(itemNeigh.SpecificTileId).spriteData;
                        item.dataNeighbor[i] = sprite;
                    }
                }
            }
        }
    }
    public void SetData(TerrainData data)
    {
        objectData = data;
        LineEditName.Text = data.name;  
        CheckBoxHasAnimation.ButtonPressed = data.isAnimated;

        CheckBoxHasRule.ButtonPressed  = objectData.isRule;
        int index = Enumerable.Range(0, OptionButtonCategory.ItemCount)
                      .FirstOrDefault(i => OptionButtonCategory.GetItemText(i) == objectData.category);

        OptionButtonCategory.Selected = index;
        OptionButtonType.Selected = (int)objectData.terrainType;

        if (objectData.isRule)
        {
            CheckBoxHasRule_Pressed();
            RegulationRules(objectData.rules);
            controlRules.SetData(objectData.rules);
        }
        if (data.isAnimated)
        {
            CheckBoxHasAnimation_Pressed();
        }
        if (objectData.animationData != null)
        {
            ContainerAnimationBasico.SetData(objectData.animationData.ToArray());
        }
        if (data.spriteData != null)
        {
            ControlSprite.SetData(data.spriteData);
      
        }

    }
}
