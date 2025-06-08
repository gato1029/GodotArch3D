using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using System;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class WindowCharacterCreator : Window,IFacadeWindow<CharacterModelBaseData>
{
	CharacterModelBaseData objectData;

    public event IFacadeWindow<CharacterModelBaseData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        objectData = new CharacterModelBaseData();
        ButtonSave.Pressed += ButtonSave_Pressed;
        ButtonSearch.Pressed += ButtonSearch_Pressed;

        foreach (CharacterBehaviorType type in Enum.GetValues(typeof(CharacterBehaviorType)))
        {
            OptionButtonBehavior.AddItem(type.ToString());
        }
        foreach (CharacterType type in Enum.GetValues(typeof(CharacterType)))
        {
            OptionButtonType.AddItem(type.ToString());
        }
        foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
        {
            OptionButtonUnitType.AddItem(type.ToString());
        }
        foreach (UnitDirectionType type in Enum.GetValues(typeof(UnitDirectionType)))
        {
            OptionButtonUnitDirectionType.AddItem(type.ToString());
        }
        foreach (UnitMoveType type in Enum.GetValues(typeof(UnitMoveType)))
        {
            OptionButtonUnitMoveType.AddItem(type.ToString());
        }

        OptionButtonBehavior.ItemSelected += OptionButtonBehavior_ItemSelected;
        OptionButtonBehavior_ItemSelected(0);
    }

    private void OptionButtonBehavior_ItemSelected(long index)
    {
        if (OptionButtonBehavior.GetSelectedId() == (int)CharacterBehaviorType.GENERICO)
        {
            VBoxContainerUnits.Visible = true;
        }
        else
        {
            VBoxContainerUnits.Visible = false;
        }
        
    }

    private void ButtonSearch_Pressed()
    {
        FacadeWindowDataSearch<AnimationCharacterBaseData> window = new FacadeWindowDataSearch<AnimationCharacterBaseData>("res://sources/WindowsDataBase/Character/WindowAnimationCharacterRefact.tscn",this,WindowType.SELECTED);
        window.OnNotifySelected += Window_OnNotifySelected;
    }

    private void Window_OnNotifySelected(AnimationCharacterBaseData objectSelected)
    {
        objectData.idAnimationCharacterBaseData = objectSelected.id;

        FrameData iFrame = objectSelected.animationDataArray[0].animationData[0].frameDataArray[0];
        var dataTexture = MaterialManager.Instance.GetAtlasTexture(objectSelected.animationDataArray[0].idMaterial, iFrame.x, iFrame.y, iFrame.widht, iFrame.height);
        Sprite2DView.Texture = dataTexture;

        
    }

    private void ButtonSave_Pressed()
    {
        objectData.name = LineEditName.Text;
        objectData.scale = (float)SpinBoxScale.Value;
        objectData.colorBase = ColorBase.Color.ToString();
        objectData.description = TextEditDescription.Text;
        objectData.bonusDataArray = PanelBonificaciones.GetAllStats().ToArray();
        objectData.statsDataArray = PanelEstadisticas.GetAllStats().ToArray();
        objectData.damageDataArray = PanelAtaque.GetAllStats().ToArray();
        objectData.defenseDataArray = PanelDefensa.GetAllStats().ToArray();

        objectData.characterBehaviorType = (CharacterBehaviorType)OptionButtonBehavior.GetSelectedId();
        objectData.characterType = (CharacterType)OptionButtonType.GetSelectedId();
        objectData.unitType = (UnitType)OptionButtonUnitType.GetSelectedId();
        objectData.unitDirectionType = (UnitDirectionType)OptionButtonUnitDirectionType.GetSelectedId();
        objectData.unitMoveType = (UnitMoveType)OptionButtonUnitMoveType.GetSelectedId();

        objectData.unitMoveData = new UnitMoveData { radiusMove = (float)SpinBoxRadiusMove.Value, radiusSearch = (float)SpinBoxRadiusSearch.Value };
        
        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChangued?.Invoke(this);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void SetData(CharacterModelBaseData data)
    {
        objectData = data;

        var components = objectData.colorBase.Trim('(', ')')
                            .Split(',')
                            .Select(s => float.Parse(s.Trim()))
                            .ToArray();
        // Color base
        ColorBase.Color = new Color(components[0], components[1], components[2], components[3]);

        LineEditName.Text = objectData.name;
        SpinBoxScale.Value = objectData.scale;
        TextEditDescription.Text = objectData.description;
        PanelBonificaciones.SetAllData(objectData.bonusDataArray);
        PanelEstadisticas.SetAllData(objectData.statsDataArray);
        PanelAtaque.SetAllData(objectData.damageDataArray);
        PanelDefensa.SetAllData(objectData.defenseDataArray);
        OptionButtonType.Select((int)objectData.characterType);
        OptionButtonBehavior.Select((int)objectData.characterBehaviorType);
        OptionButtonUnitType.Select((int)objectData.unitType);
        OptionButtonUnitDirectionType.Select((int)objectData.unitDirectionType);
        OptionButtonUnitMoveType.Select((int)objectData.unitMoveType);

        SpinBoxRadiusMove.Value = objectData.unitMoveData.radiusMove;
        SpinBoxRadiusSearch.Value = objectData.unitMoveData.radiusSearch;

        OptionButtonBehavior_ItemSelected(0);
        var dataInt = DataBaseManager.Instance.FindById<AnimationCharacterBaseData>(data.idAnimationCharacterBaseData);
        FrameData iFrame = dataInt.animationDataArray[0].animationData[0].frameDataArray[0];
        var dataTexture = MaterialManager.Instance.GetAtlasTexture(dataInt.animationDataArray[0].idMaterial, iFrame.x, iFrame.y, iFrame.widht, iFrame.height);
        Sprite2DView.Texture = dataTexture;
    }
}
