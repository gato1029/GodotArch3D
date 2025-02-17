using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    // Called when the node enters the scene tree for the first time.
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
        GetNode<Button>("VBoxContainer2/Button3").Pressed += Delete_Button; ;


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

        centralButton.Pressed += CentralButton_Pressed; ;
        centralButton.SetMeta("check", false);
        for (int i = 0; i < 8; i++)
		{
			Button button = arrayButton[i];
			button.SetMeta("id",i);
			button.SetMeta("check",false);
            button.Pressed += () => WindowAutoTileItem_Pressed(button);
		}

        optionButton.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
    }

    private void CheckIsnullcheckBox_Pressed()
    {
        Texture2D texture2DNull = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");
        Texture2D texture2DSome = GD.Load<Texture2D>("res://resources/Textures/internal/check-64.png");
        Texture2D texture2DNullCheck = GD.Load<Texture2D>("res://resources/Textures/internal/exclamation.PNG");

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

        if (tiledDataCheck)
		{
			if (check)
			{
                
                if (currentIndex == 1)
                {
                    WindowFilterRules win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowFilterRules.tscn").Instantiate<WindowFilterRules>();
                    AddChild(win);
                    win.Show();
                    win.SetType(1);
                    win.OnRequestSelectedItem += (int idTile) =>
                    {
                        var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileSimpleData>(idTile);
                        button.Icon = data.textureVisual;
                        var indexNeighbord = tileRuleData.GetDirectionFromIndex(id);
                        tileRuleData.UpdateNeighborMask(indexNeighbord, check, data);
                    };
                }
                if (currentIndex == 2)
                {
                    WindowFilterRules win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowFilterRules.tscn").Instantiate<WindowFilterRules>();
                    AddChild(win);
                    win.Show();
                    win.SetType(2);
                    win.OnRequestSelectedItem += (int idTile) =>
                    {
                        var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileDynamicData>(idTile);
                        button.Icon = data.textureVisual;
                        var indexNeighbord = tileRuleData.GetDirectionFromIndex(id);
                        tileRuleData.UpdateNeighborMask(indexNeighbord, check, data);
                    };
                }
                if (currentIndex == 3)
                {
                    WindowFilterRules win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowFilterRules.tscn").Instantiate<WindowFilterRules>();
                    AddChild(win);
                    win.Show();
                    win.SetType(3);
                    win.OnRequestSelectedItem += (int idTile) =>
                    {
                        var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileAnimateData>(idTile);
                        button.Icon = data.textureVisual;
                        var indexNeighbord = tileRuleData.GetDirectionFromIndex(id);
                        tileRuleData.UpdateNeighborMask(indexNeighbord, check, data);
                    };
                }
            }
			else
			{
                Texture2D texture2D = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");
                button.Icon = texture2D;
                var indexNeighbord = tileRuleData.GetDirectionFromIndex(id);
                tileRuleData.UpdateNeighborMask(indexNeighbord, check);
            }
           
        }
		else
		{						
			if (check)
			{
                if (checkIsnullcheckBox.ButtonPressed)
                {
                    Texture2D texture2D = GD.Load<Texture2D>("res://resources/Textures/internal/exclamation.PNG");
                    button.Icon = texture2D;
                }
                else
                {
                    Texture2D texture2D = GD.Load<Texture2D>("res://resources/Textures/internal/check-64.png");
                    button.Icon = texture2D;
                }
				
            }
			else
			{
                Texture2D texture2D = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");
                button.Icon = texture2D;
            }
            var indexNeighbord = tileRuleData.GetDirectionFromIndex(id);
            tileRuleData.UpdateNeighborMask(indexNeighbord, check);
        }
		       
    }


    public void SetPosition(int position)
	{
		this.position = position;
		labelName.Text = "Regla:"+position.ToString();   
    }

    public void LoadData(TileRuleData tileRuleData)
	{
        Texture2D texture2DNull = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");
        Texture2D texture2DSome = GD.Load<Texture2D>("res://resources/Textures/internal/check-64.png");
        Texture2D texture2DNullCheck = GD.Load<Texture2D>("res://resources/Textures/internal/exclamation.PNG");
        this.tileRuleData = tileRuleData;
        checkIsnullcheckBox.ButtonPressed = tileRuleData.checkIsNull;
        for (int i = 0; i <= 7; i++)
        {
            if (tileRuleData.tileDataMask[i] != null)
            {
                arrayButton[i].Icon = tileRuleData.tileDataMask[i].textureVisual;
            }
            else
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
        
        centralButton.Icon = tileRuleData.tileDataCentral?.textureVisual ?? texture2DNull;                      
        
      

   
    }
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
