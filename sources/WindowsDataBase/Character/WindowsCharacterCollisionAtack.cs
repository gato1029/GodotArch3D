using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;


public partial class WindowsCharacterCollisionAtack : PanelContainer
{
	public CharacterColliderAtackData data;

	SpinBox[] spinBoxesWidht = new SpinBox[4];
    SpinBox[] spinBoxesHeight = new SpinBox[4];
    SpinBox[] spinBoxesOffsetX = new SpinBox[4];
    SpinBox[] spinBoxesOffsetY = new SpinBox[4];

    Button buttonRefresh;
    SpinBox spinBoxId;

    public delegate void RequestDeleteHandler(WindowsCharacterCollisionAtack item);
    public event RequestDeleteHandler OnRequestDelete;

    public delegate void RequestNotifyHandler(CharacterColliderAtackData itemData);
    public event RequestNotifyHandler OnNotifyChangue;

    public void SetData(CharacterColliderAtackData In_data)
    {
        data = In_data;

        spinBoxId.Value = data.id;
        for (int i = 0; i < In_data.colliders.Length; i++)
        {
            Rectangle item = (Rectangle) In_data.colliders[i];
            spinBoxesWidht[i].Value = item.Width;
            spinBoxesHeight[i].Value = item.Height;
            spinBoxesOffsetX[i].Value = item.OriginCurrent.X;
            spinBoxesOffsetY[i].Value = item.OriginCurrent.Y;
        }
    }
    public void SetID(int id)
    { 
        spinBoxId.Value = id;
    }  
        // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        data = new CharacterColliderAtackData();
        spinBoxesWidht[0]  = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer3/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox");
        spinBoxesHeight[0] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer3/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox2");
        spinBoxesOffsetX[0] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer3/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox");
        spinBoxesOffsetY[0] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer3/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox2");

        spinBoxesWidht[0].ValueChanged += ValueChanguedUP;
        spinBoxesHeight[0].ValueChanged += ValueChanguedUP;
        spinBoxesOffsetX[0].ValueChanged += ValueChanguedUP;
        spinBoxesOffsetY[0].ValueChanged += ValueChanguedUP;

        spinBoxesWidht[1] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer4/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox");
        spinBoxesHeight[1] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer4/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox2");
        spinBoxesOffsetX[1] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer4/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox");
        spinBoxesOffsetY[1] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer4/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox2");

        spinBoxesWidht[1].ValueChanged += ValueChanguedDOWN;
        spinBoxesHeight[1].ValueChanged += ValueChanguedDOWN;
        spinBoxesOffsetX[1].ValueChanged += ValueChanguedDOWN;
        spinBoxesOffsetY[1].ValueChanged += ValueChanguedDOWN;

        spinBoxesWidht[2] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer5/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox");
        spinBoxesHeight[2] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer5/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox2");
        spinBoxesOffsetX[2] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer5/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox");
        spinBoxesOffsetY[2] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer5/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox2");

        spinBoxesWidht[2].ValueChanged += ValueChanguedLEFT;
        spinBoxesHeight[2].ValueChanged += ValueChanguedLEFT;
        spinBoxesOffsetX[2].ValueChanged += ValueChanguedLEFT;
        spinBoxesOffsetY[2].ValueChanged += ValueChanguedLEFT;

        spinBoxesWidht[3] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer6/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox");
        spinBoxesHeight[3] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer6/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox2");
        spinBoxesOffsetX[3] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer6/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox");
        spinBoxesOffsetY[3] = GetNode<SpinBox>("MarginContainer/Node/VBoxContainer6/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox2");

        spinBoxesWidht[3].ValueChanged += ValueChanguedRIGHT;
        spinBoxesHeight[3].ValueChanged += ValueChanguedRIGHT;
        spinBoxesOffsetX[3].ValueChanged += ValueChanguedRIGHT;
        spinBoxesOffsetY[3].ValueChanged += ValueChanguedRIGHT;

        GetNode<Button>("MarginContainer/Node/HBoxContainer/Button2").Pressed += Delete_Press;

        buttonRefresh = GetNode<Button>("MarginContainer/Node/Button");

        buttonRefresh.Pressed += ButtonRefresh_Pressed;
        spinBoxId = GetNode<SpinBox>("MarginContainer/Node/HBoxContainer/SpinBox");

        spinBoxId.ValueChanged += SpinBoxId_ValueChanged;
        data.colliders = new GeometricShape2D[4];
        data.colliders[0] = new Rectangle(20,20);
        data.colliders[1] = new Rectangle(20, 20);
        data.colliders[2] = new Rectangle(20, 20);
        data.colliders[3] = new Rectangle(20, 20);
    }

    private void ButtonRefresh_Pressed()
    {
        
        data.id = (int)spinBoxId.Value;
        data.colliders[0] = new Rectangle((float)spinBoxesWidht[0].Value, (float)spinBoxesHeight[0].Value, new Vector2((float)spinBoxesOffsetX[0].Value, (float)spinBoxesOffsetY[0].Value));
        data.colliders[1] = new Rectangle((float)spinBoxesWidht[1].Value, (float)spinBoxesHeight[1].Value, new Vector2((float)spinBoxesOffsetX[1].Value, (float)spinBoxesOffsetY[1].Value));
        data.colliders[2] = new Rectangle((float)spinBoxesWidht[2].Value, (float)spinBoxesHeight[2].Value, new Vector2((float)spinBoxesOffsetX[2].Value, (float)spinBoxesOffsetY[2].Value));
        data.colliders[3] = new Rectangle((float)spinBoxesWidht[3].Value, (float)spinBoxesHeight[3].Value, new Vector2((float)spinBoxesOffsetX[3].Value, (float)spinBoxesOffsetY[3].Value));
        
        OnNotifyChangue?.Invoke(data);
    }

    private void Delete_Press()
    {
        OnRequestDelete?.Invoke(this);
    }

    private void SpinBoxId_ValueChanged(double value)
    {
        data.id = (int)value;
    }

    private void ValueChanguedRIGHT(double value)
    {
        data.colliders[3] = new Rectangle((float)spinBoxesWidht[3].Value, (float)spinBoxesHeight[3].Value, new Vector2((float)spinBoxesOffsetX[3].Value, (float)spinBoxesOffsetY[3].Value));
        OnNotifyChangue?.Invoke(data);
    }

    private void ValueChanguedLEFT(double value)
    {
        data.colliders[2] = new Rectangle((float)spinBoxesWidht[2].Value, (float)spinBoxesHeight[2].Value, new Vector2((float)spinBoxesOffsetX[2].Value, (float)spinBoxesOffsetY[2].Value));
        OnNotifyChangue?.Invoke(data);
    }


    private void ValueChanguedDOWN(double value)
    {
        data.colliders[1] = new Rectangle((float)spinBoxesWidht[1].Value, (float)spinBoxesHeight[1].Value, new Vector2((float)spinBoxesOffsetX[1].Value, (float)spinBoxesOffsetY[1].Value));
        OnNotifyChangue?.Invoke(data);
    }

    private void ValueChanguedUP(double value)
    {
        data.colliders[0] = new Rectangle((float)spinBoxesWidht[0].Value, (float)spinBoxesHeight[0].Value, new Vector2((float)spinBoxesOffsetX[0].Value, (float)spinBoxesOffsetY[0].Value));
        OnNotifyChangue?.Invoke(data);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
