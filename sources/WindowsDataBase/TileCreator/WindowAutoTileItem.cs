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
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tileRuleData = new TileRuleData();
		arrayButton = new Button[8];
		position = 0;
		tiledDataCheck = true;
		labelName	   = GetNode<Label>("VBoxContainer2/Label");

        GetNode<Button>("VBoxContainer2/Button3").Pressed += Delete_Button; ;


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
    }

    private void CentralButton_Pressed()
    {
        bool check = !(bool)centralButton.GetMeta("check");
        centralButton.SetMeta("check", check);

        if (check)
        {
            WindowDataGeneric win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
            AddChild(win);
            win.Show();
            PackedScene ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowTileSimple.tscn");
            win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.SELECTOR);
            win.SetLoaddBAction(() =>
            {
                var collection = DataBaseManager.Instance.FindAll<TileSimpleData>();
                List<IdData> ids = new List<IdData>();
                foreach (var item in collection)
                {
                    IdData iddata = item;
                    ids.Add(iddata);
                }
                return ids;
            }
            );
            win.OnRequestSelectedItem += (int id) =>
            {
                var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileSimpleData>(id);
                centralButton.Icon = data.textureVisual;           
                tileRuleData.tileDataCentral = data;
            };
        }
        else
        {
            Texture2D texture2D = GD.Load<Texture2D>("res://resources/Textures/internal/cancel.png");
            centralButton.Icon = texture2D;
            tileRuleData.tileDataCentral = null;
        }
    }

    private void Delete_Button()
    {
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
                WindowDataGeneric win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
                AddChild(win);
                win.Show();
                PackedScene ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowTileSimple.tscn");
                win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.SELECTOR);
                win.SetLoaddBAction(() =>
                {
                    var collection = DataBaseManager.Instance.FindAll<TileSimpleData>();
                    List<IdData> ids = new List<IdData>();
                    foreach (var item in collection)
                    {
                        IdData iddata = item;
                        ids.Add(iddata);
                    }
                    return ids;
                }
                );
                win.OnRequestSelectedItem += (int idTile) => {
                    var data =   DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileSimpleData>(idTile);
                    button.Icon = data.textureVisual;
                    var indexNeighbord = tileRuleData.GetDirectionFromIndex(id);
                    tileRuleData.UpdateNeighborMask(indexNeighbord, check,data);
                };
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
				Texture2D texture2D = GD.Load<Texture2D>("res://resources/Textures/internal/check-64.png");
				button.Icon = texture2D;
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
        this.tileRuleData = tileRuleData;        
        centralButton.Icon = tileRuleData.tileDataCentral?.textureVisual ?? texture2DNull;                      
        
        arrayButton[0].Icon = tileRuleData.tileDataMask[0]?.textureVisual ?? texture2DNull; 
        arrayButton[1].Icon = tileRuleData.tileDataMask[1]?.textureVisual ?? texture2DNull;
        arrayButton[2].Icon = tileRuleData.tileDataMask[2]?.textureVisual ?? texture2DNull;
        arrayButton[3].Icon = tileRuleData.tileDataMask[3]?.textureVisual ?? texture2DNull;
        arrayButton[4].Icon = tileRuleData.tileDataMask[4]?.textureVisual ?? texture2DNull;
        arrayButton[5].Icon = tileRuleData.tileDataMask[5]?.textureVisual ?? texture2DNull;
        arrayButton[6].Icon = tileRuleData.tileDataMask[6]?.textureVisual ?? texture2DNull;
        arrayButton[7].Icon = tileRuleData.tileDataMask[7]?.textureVisual ?? texture2DNull;

   
    }
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
