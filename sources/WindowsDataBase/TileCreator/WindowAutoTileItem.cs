using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;



public partial class WindowAutoTileItem : HBoxContainer
{
	public TileRuleData tileRuleData;

	Button[] arrayButton;
	Button centralButton;
	Label labelName;
	int position;
	bool tiledDataCheck;
    OptionButton optionButton;
    CheckBox checkIsnullcheckBox;

    Button up;
    Button down;
    public delegate void RequestOrderItemHandler(int id, int position, WindowAutoTileItem windowAutoTileItem);
    public event RequestOrderItemHandler OnRequestOrderItem;

    public delegate void RequestOrderDeleteHandler( int position, WindowAutoTileItem windowAutoTileItem);
    public event RequestOrderDeleteHandler OnDeleteItem;
    [Signal]
    public delegate void OnSelectedEventHandler();
    // Called when the node enters the scene tree for the first time.
    public bool IsSelected { get; private set; }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        Modulate = selected ? new Color(1, 1, 1, 1) : Colors.GreenYellow;
        // Opcional: cambiar color de fondo o borde visualmente
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
		tileRuleData = new TileRuleData();
		arrayButton = new Button[8];
		position = 0;
		tiledDataCheck = false;
		labelName	   = GetNode<Label>("VBoxContainer2/HBoxContainer/Label");
        optionButton   = GetNode<OptionButton>("VBoxContainer2/HBoxContainer/OptionButton");
        checkIsnullcheckBox = GetNode<CheckBox>("VBoxContainer2/CheckBox");

        checkIsnullcheckBox.Pressed += CheckIsnullcheckBox_Pressed;
        optionButton.ItemSelected += OptionButton_ItemSelected; 
        GetNode<Button>("VBoxContainer2/Button3").Pressed += Delete_Button;
        GetNode<Button>("VBoxContainer2/ButtonSelect").Pressed += ButtonSelect_Pressed;

        up = GetNode<Button>("VBoxContainer/Button2");
        down = GetNode<Button>("VBoxContainer/Button");

        up.Pressed += UP_Button;
        down.Pressed += Down_Pressed;

        centralButton  = GetNode<Button>("VBoxContainer2/GridContainer/ButtonCentral");
        arrayButton[0] = GetNode<Button>("VBoxContainer2/GridContainer/Button0");
        arrayButton[1] = GetNode<Button>("VBoxContainer2/GridContainer/Button1");
        arrayButton[2] = GetNode<Button>("VBoxContainer2/GridContainer/Button2");
        arrayButton[3] = GetNode<Button>("VBoxContainer2/GridContainer/Button3");
        arrayButton[4] = GetNode<Button>("VBoxContainer2/GridContainer/Button4");
        arrayButton[5] = GetNode<Button>("VBoxContainer2/GridContainer/Button5");
        arrayButton[6] = GetNode<Button>("VBoxContainer2/GridContainer/Button6");
        arrayButton[7] = GetNode<Button>("VBoxContainer2/GridContainer/Button7");

        centralButton.Pressed += CentralButton_Pressed;
        centralButton.SetMeta("check", false);
        for (int i = 0; i < 8; i++)
		{
			Button button = arrayButton[i];
			button.SetMeta("id",i);
			button.SetMeta("check",false);
            button.Pressed += () => WindowAutoTileItem_Pressed(button);
		}

        optionButton.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        OptionButton_ItemSelected(2);
    }

    private void ButtonSelect_Pressed()
    {
        EmitSignal(nameof(OnSelected));
    }


    private void CheckIsnullcheckBox_Pressed()
    {
        Texture2D texture2DNull = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png"); // vacio
        Texture2D texture2DSome = GD.Load<Texture2D>("res://resources/Textures/internal/check-64.png"); // obligado
        Texture2D texture2DNullCheck = GD.Load<Texture2D>("res://resources/Textures/internal/exclamation.PNG"); // indiferente

        tileRuleData.checkIsNull = checkIsnullcheckBox.ButtonPressed;
    
            for (int i = 0; i <= 7; i++)
            {
                if (tileRuleData.tileDataMask[i] == null)
                {           
                    if (tileRuleData.IsDirectionConnected(i))
                    {
                        if (tileRuleData.checkIsNull)
                        {

                            arrayButton[i].Icon = texture2DNullCheck;
                        }
                        else
                        {
                            arrayButton[i].Icon = texture2DSome;
                        }

                    }
                    else
                    {
                        arrayButton[i].Icon = texture2DNull;
                    }

                }
            }
        
    }

    private void Down_Pressed()
    {
        OnRequestOrderItem?.Invoke(1,position,this);
    }

    private void UP_Button()
    {
        OnRequestOrderItem?.Invoke(0,position, this);
    }

    int currentIndex;
    private void OptionButton_ItemSelected(long index)
    {
        if (index==0)
        {
            tiledDataCheck = false;
        }
        else
        {
            tiledDataCheck = true;
            currentIndex = (int)index;
        }
        
    }

    private void CentralButton_Pressed()
    {
        bool check = !(bool)centralButton.GetMeta("check");
        centralButton.SetMeta("check", check);

        if (check)
        {
            if (currentIndex ==1)
            {
                WindowFilterRules win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowFilterRules.tscn").Instantiate<WindowFilterRules>();
                AddChild(win);
                win.Show();
                win.SetType(1);
                win.OnRequestSelectedItem += (int id) =>
                {
                    var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileSimpleData>(id);
                    centralButton.Icon = data.textureVisual;
                    tileRuleData.tileDataCentral = data;
                    tileRuleData.idTileDataCentral = id;
                };
            }
            if (currentIndex == 2)
            {
                WindowFilterRules win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowFilterRules.tscn").Instantiate<WindowFilterRules>();
                AddChild(win);
                win.Show();
                win.SetType(2);
                win.OnRequestSelectedItem += (int id) =>
                {
                    var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileDynamicData>(id);
                    centralButton.Icon = data.textureVisual;
                    tileRuleData.tileDataCentral = data;
                    tileRuleData.idTileDataCentral = id;
                };
            }
            if (currentIndex == 3)
            {
                WindowFilterRules win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowFilterRules.tscn").Instantiate<WindowFilterRules>();
                AddChild(win);
                win.Show();
                win.SetType(3);
                win.OnRequestSelectedItem += (int id) =>
                {
                    var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileAnimateData>(id);
                    centralButton.Icon = data.textureVisual;
                    tileRuleData.tileDataCentral = data;
                    tileRuleData.idTileDataCentral = id;
                };
            }

        }
        else
        {
            Texture2D texture2D = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");
            centralButton.Icon = texture2D;
            tileRuleData.tileDataCentral = null;
            tileRuleData.idTileDataCentral = 0;
        }
    }

    private void Delete_Button()
    {
        OnDeleteItem?.Invoke(position, this);
        QueueFree();
    }

    private void WindowAutoTileItem_Pressed(Button button)
    {
        int id = (int)button.GetMeta("id");
        bool check = !(bool)button.GetMeta("check");
        button.SetMeta("check", check);

        var direction = tileRuleData.GetDirectionFromIndex(id);

        if (tiledDataCheck)
        {
            if (check)
            {
                ShowTileSelectionWindow(currentIndex, (data, _) =>
                {
                    button.Icon = data.textureVisual;
                    tileRuleData.UpdateNeighborMask(direction, check, data);
                    // ✅ Asegura que también se actualice el estado y el ID específico                    
                    var condition = tileRuleData.neighborConditions[id];
                    condition.State = NeighborState.Filled;
                    condition.SpecificTileId = data.id;
                });
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


    public void SetPosition(int position)
	{
		this.position = position;
		labelName.Text = "Regla:"+position.ToString();   
    }

    public void LoadData(TileRuleData tileRuleData)
	{
        this.tileRuleData = tileRuleData;

        Texture2D texture2DNull = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");
        Texture2D texture2DSome = GD.Load<Texture2D>("res://resources/Textures/internal/check-64.png");
        Texture2D texture2DNullCheck = GD.Load<Texture2D>("res://resources/Textures/internal/exclamation.PNG");

        for (int i = 0; i <= 7; i++)
        {
            var condition = tileRuleData.neighborConditions[i];

            if (condition.State == NeighborState.Filled && condition.SpecificTileId > 0)
            {
                var tile = TilesManager.Instance.GetTileData(condition.SpecificTileId);
                if (tile != null)
                {
                    arrayButton[i].Icon = tile.textureVisual;
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

        centralButton.Icon = tileRuleData.tileDataCentral?.textureVisual ?? GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");


    }

    private void ShowTileSelectionWindow(int type, Action<TileData, int> callback)
    {
        var win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowFilterRules.tscn").Instantiate<WindowFilterRules>();
        AddChild(win);
        win.Show();
        win.SetType(type);
        win.OnRequestSelectedItem += (int id) =>
        {
            TileData data = type switch
            {
                1 => DataBaseManager.Instance.FindById<TileSimpleData>(id),
                2 => DataBaseManager.Instance.FindById<TileDynamicData>(id),
                3 => DataBaseManager.Instance.FindById<TileAnimateData>(id),
                _ => null
            };
            if (data != null) callback(data, id);
        };
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
