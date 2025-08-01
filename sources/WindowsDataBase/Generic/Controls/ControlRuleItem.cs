using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

public enum TypeRule
{
    Terreno =0,
    Recursos = 1,
    Edificios = 2
}
public partial class ControlRuleItem : PanelContainer
{
    TypeRule typeRule = TypeRule.Terreno;
    public RuleData tileRuleData;

    Button[] arrayButton;
    
    
    int position;
    bool tiledDataCheck;
                
    public delegate void RequestOrderItemHandler(int id, int position, ControlRuleItem windowAutoTileItem);
    public event RequestOrderItemHandler OnRequestOrderItem;

    public delegate void RequestOrderDeleteHandler(int position, ControlRuleItem windowAutoTileItem);
    public event RequestOrderDeleteHandler OnDeleteItem;
    [Signal]
    public delegate void OnSelectedEventHandler();
    // Called when the node enters the scene tree for the first time.
    public bool IsSelected { get; private set; }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        Modulate = selected ? new Color(1, 1, 1, 1) : Colors.LightSlateGray;
        // Opcional: cambiar color de fondo o borde visualmente
    }

    public void SetTypeItem(TypeRule typeRule)
    {
        this.typeRule = typeRule;
    }
    public override void _GuiInput(InputEvent @event)
    {

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {

            EmitSignal(nameof(OnSelected));
        }
    }

    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        AddToGroup("rule_items");
        tileRuleData = new RuleData();
        arrayButton = new Button[8];
        position = 0;
        tiledDataCheck = false;

        ButtonDelete.Pressed += Delete_Button;
          
     
    

        
        arrayButton[0] = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Button0");
        arrayButton[1] = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Button1");
        arrayButton[2] = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Button2");
        arrayButton[3] = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Button3");
        arrayButton[4] = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Button4");
        arrayButton[5] = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Button5");
        arrayButton[6] = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Button6");
        arrayButton[7] = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Button7");

        ButtonCentral.GuiInput += CentralButton_Pressed;
        ButtonCentral.SetMeta("check", false);
        for (int i = 0; i < 8; i++)
        {
            Button button = arrayButton[i];
            button.SetMeta("id", i);
            button.SetMeta("check", false);
            button.GuiInput += (InputEvent @event) => WindowAutoTileItem_Pressed(button, @event);
        }


    }

    private void CentralButton_Pressed(InputEvent @event)
    {
        bool click = false;
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            
            EmitSignal(nameof(OnSelected));

            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                // Click izquierdo: sin selección
                currentIndex = 1;
                click = true;
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                // Click derecho: con selección
                currentIndex = 0;
                click = true;
            }

            bool check = !(bool)ButtonCentral.GetMeta("check");
            ButtonCentral.SetMeta("check", check);

            if (click)
            {


                if (currentIndex == 1)
                {
                    switch (typeRule)
                    {
                        case TypeRule.Terreno:
                            FacadeWindowDataSearch<TerrainData> winTerrain = new FacadeWindowDataSearch<TerrainData>("res://sources/WindowsDataBase/Terrain/windowTerrain.tscn", this, WindowType.SELECTED);
                            winTerrain.OnNotifySelected += (TerrainData objectSelected) =>
                            {
                                ButtonCentral.Icon = objectSelected.textureVisual;
                                tileRuleData.dataCentral = objectSelected.spriteData;
                                tileRuleData.idDataCentral = objectSelected.id;
                            };
                            break;
                        case TypeRule.Recursos:
                            FacadeWindowDataSearch<ResourceData> winResources = new FacadeWindowDataSearch<ResourceData>("res://sources/WindowsDataBase/Resources/WindowResources.tscn", this, WindowType.SELECTED);
                            winResources.OnNotifySelected += (ResourceData objectSelected) =>
                            {
                                ButtonCentral.Icon = objectSelected.textureVisual;
                                tileRuleData.dataCentral = objectSelected.spriteData;
                                tileRuleData.idDataCentral = objectSelected.id;
                            };
                            break;
                        case TypeRule.Edificios:
                            FacadeWindowDataSearch<BuildingData> winBuilding = new FacadeWindowDataSearch<BuildingData>("res://sources/WindowsDataBase/Building/WindowBuilding.tscn", this, WindowType.SELECTED);
                            winBuilding.OnNotifySelected += (BuildingData objectSelected) =>
                            {
                                ButtonCentral.Icon = objectSelected.textureVisual;
                                tileRuleData.dataCentral = objectSelected.spriteData;
                                tileRuleData.idDataCentral = objectSelected.id;
                            };
                            break;
                        default:
                            break;
                    }


                }
                else
                {
                    tileRuleData.idDataCentral = 0;
                    tileRuleData.dataCentral = null;
                    string path = "res://resources/Textures/internal/cancel.png";
                    ButtonCentral.Icon = GD.Load<Texture2D>(path);
                }
            }
        }
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
      
        return data.VariantType == Variant.Type.Int; // Godot 4 puede usar Int64
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        ulong draggedId = (ulong)data;

        foreach (Node node in GetTree().GetNodesInGroup("rule_items"))
        {
            if (node.GetInstanceId() == draggedId && node is ControlRuleItem draggedItem && GetParent() is GridContainer container)
            {
                int dropIndex = container.GetChildren().IndexOf(this);
                int fromIndex = container.GetChildren().IndexOf(draggedItem);

                if (fromIndex == dropIndex)
                    return;

                container.MoveChild(draggedItem, dropIndex);
                UpdatePositions(container);
                break;
            }
        }
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        var id = GetInstanceId();
        SetDragPreview((Control)Duplicate());
        return id;
    }


    private void UpdatePositions(Node container)
    {
        for (int i = 0; i < container.GetChildCount(); i++)
        {
            if (container.GetChild(i) is ControlRuleItem item)
            {
                item.SetPosition(i);
            }
        }
    }
    int currentIndex;
  

  



    private void Delete_Button()
    {
        EmitSignal(nameof(OnSelected));
        // Crear el diálogo de confirmación
        var dialog = new ConfirmationDialog
        {
            DialogText = "¿Estás seguro de que deseas eliminar esta regla?" + "Regla N° "+ position,
            OkButtonText = "Sí",
            CancelButtonText = "Cancelar"
        };

        // Agregarlo como hijo temporal (se eliminará al cerrar)
        AddChild(dialog);

        // Conectar la señal Confirmed al cierre y eliminación
        dialog.Confirmed += () =>
        {
            OnDeleteItem?.Invoke(position, this);
            QueueFree();
        };

        dialog.VisibilityChanged += () =>
        {
            if (!dialog.Visible) // Solo cuando se oculta
            {
                dialog.QueueFree();
            }
        };


        // Mostrar el diálogo centrado
        dialog.PopupCentered();

       
    }

    private void WindowAutoTileItem_Pressed(Button button, InputEvent @event)
    {
        bool click = false;
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            EmitSignal(nameof(OnSelected));

            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                // Click izquierdo: sin selección
                currentIndex = 1;
                tiledDataCheck = true;
                click = true;
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                // Click derecho: con selección
                currentIndex = 0;
                tiledDataCheck = false;
                click = true;
            }
            if (click)
            {


                int id = (int)button.GetMeta("id");
                bool check = !(bool)button.GetMeta("check");
                button.SetMeta("check", check);

                var direction = tileRuleData.GetDirectionFromIndex(id);
                if (tiledDataCheck)
                {
                    if (check)
                    {
                        switch (typeRule)
                        {
                            case TypeRule.Terreno:
                                FacadeWindowDataSearch<TerrainData> winTerrain = new FacadeWindowDataSearch<TerrainData>("res://sources/WindowsDataBase/Terrain/windowTerrain.tscn", this, WindowType.SELECTED);
                                winTerrain.OnNotifySelected += (TerrainData objectSelected) =>
                                {
                                    button.Icon = objectSelected.textureVisual;
                                    tileRuleData.UpdateNeighborMask(direction, check, objectSelected.id);
                                    var condition = tileRuleData.neighborConditions[id];
                                    condition.State = NeighborState.Filled;
                                    condition.SpecificTileId = objectSelected.id;

                                };
                                break;
                            case TypeRule.Recursos:
                                FacadeWindowDataSearch<ResourceData> winResource = new FacadeWindowDataSearch<ResourceData>("res://sources/WindowsDataBase/Resources/WindowResources.tscn", this, WindowType.SELECTED);
                                winResource.OnNotifySelected += (ResourceData objectSelected) =>
                                {
                                    button.Icon = objectSelected.textureVisual;
                                    tileRuleData.UpdateNeighborMask(direction, check, objectSelected.id);
                                    var condition = tileRuleData.neighborConditions[id];
                                    condition.State = NeighborState.Filled;
                                    condition.SpecificTileId = objectSelected.id;

                                };
                                break;
                            case TypeRule.Edificios:
                                FacadeWindowDataSearch<BuildingData> winBuiling = new FacadeWindowDataSearch<BuildingData>("res://sources/WindowsDataBase/Building/WindowBuilding.tscn", this, WindowType.SELECTED);
                                winBuiling.OnNotifySelected += (BuildingData objectSelected) =>
                                {
                                    button.Icon = objectSelected.textureVisual;
                                    tileRuleData.UpdateNeighborMask(direction, check, objectSelected.id);
                                    var condition = tileRuleData.neighborConditions[id];
                                    condition.State = NeighborState.Filled;
                                    condition.SpecificTileId = objectSelected.id;

                                };
                                break;
                            default:
                                break;
                        }

                    }
                    else
                    {
                        button.Icon = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");
                        tileRuleData.UpdateNeighborMask(direction, check);
                    }
                }
                else
                {
                    var condition = tileRuleData.neighborConditions[id];
                    condition.SpecificTileId = 0;
                    condition.State = condition.State switch
                    {
                        NeighborState.Filled => NeighborState.Empty,
                        NeighborState.Empty => NeighborState.Any,
                        _ => NeighborState.Filled
                    };

                    string path = condition.State switch
                    {
                        NeighborState.Filled => "res://resources/Textures/internal/check-64.png",
                        NeighborState.Empty => "res://resources/Textures/internal/cancel.png",
                        _ => "res://resources/Textures/internal/exclamation.PNG"
                    };


                    button.Icon = GD.Load<Texture2D>(path);
                }
            }
        }
    }


    public void SetPosition(int position)
    {
        this.position = position;
        LabelRule.Text = "Regla:" + position.ToString();
    }

    public void LoadData(RuleData spriteRuleData)
    {
        this.tileRuleData = spriteRuleData;

        Texture2D texture2DNull = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");
        Texture2D texture2DSome = GD.Load<Texture2D>("res://resources/Textures/internal/check-64.png");
        Texture2D texture2DNullCheck = GD.Load<Texture2D>("res://resources/Textures/internal/exclamation.PNG");

        for (int i = 0; i <= 7; i++)
        {
            var condition = spriteRuleData.neighborConditions[i];

            if (condition.State == NeighborState.Filled && condition.SpecificTileId > 0)
            {
                var sprite = spriteRuleData.dataNeighbor[i];
                if (sprite != null)
                {
                    arrayButton[i].Icon = MaterialManager.Instance.GetAtlasTextureInternal(sprite);
                    continue;
                }
           
            }

            switch (condition.State)
            {
                case NeighborState.Filled:
                    arrayButton[i].Icon = texture2DSome;
                    break;
                case NeighborState.Empty:
                    arrayButton[i].Icon = texture2DNull;
                    break;
                case NeighborState.Any:
                default:
                    arrayButton[i].Icon = texture2DNullCheck;
                    break;
            }
        }

        if (spriteRuleData.dataCentral != null )
        {
            ButtonCentral.Icon = MaterialManager.Instance.GetAtlasTextureInternal(spriteRuleData.dataCentral);
        }
        else
        {
            ButtonCentral.Icon = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");
        }

    }


}
