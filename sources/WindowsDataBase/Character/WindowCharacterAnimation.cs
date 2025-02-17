using Godot;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Runtime.CompilerServices;

public partial class WindowCharacterAnimation : PanelContainer
{
	public AnimationStateData data;

    public delegate void RequestDeleteHandler(WindowCharacterAnimation item);
    public event RequestDeleteHandler OnRequestDelete;

    public delegate void RequestNotifyHandler(AnimationStateData itemData, int state);
    public event RequestNotifyHandler OnNotifyChangue;

    
    public delegate void RequestOrderItemHandler(int id, int position, WindowCharacterAnimation windowAutoTileItem);
    public event RequestOrderItemHandler OnRequestOrderItem;

    Button up;
    Button down;

    SpinBox idSpin;
    SpinBox idSpinBox;
    LineEdit framesLine;
    SpinBox frameDuration;
    TextureRect textureSelection;
    CheckButton tipoCheckButton;
    CheckBox isloop;
    CheckBox mirrorHorizontal;
    int idMaterial;
    public int idPosition;
    // Called when the node enters the scene tree for the first time.

    public int currentIdState = 0;

    public void SetData(AnimationStateData In_data)
    {
        if (In_data.animationData[0].idFrames!=null)
        {
            data = In_data;
            if (In_data.animationData[0] != null)
            {
                string strFrame = "";
                foreach (var frame in In_data.animationData[0].idFrames)
                {
                    strFrame = strFrame + "," + frame.ToString();
                }
                strFrame = strFrame.Substr(1, strFrame.Length);
                framesLine.Text = strFrame;
            }

            frameDuration.Value = data.frameDuration;
            isloop.ButtonPressed = data.loop;
            mirrorHorizontal.ButtonPressed = data.mirrorHorizontal;
            idSpin.Value = data.id;
            idSpinBox.Value = 0;
        }
     
    }
    public void SetID(int id)
    {
        idSpin.Value = id;
        idPosition = id;
        data.id = id;
    }
    public void SetMaterial(int idMat)
    {
        idMaterial = idMat;
    }
    public override void _Ready()
	{
        GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/Button2").Pressed += Delete_Press;
        GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/Button").Pressed += Refresh_Press;
        textureSelection = GetNode<TextureRect>("MarginContainer/VBoxContainer/TextureRect");

        up = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/Button2");
        down = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/Button");

        up.Pressed += up_Pressed;
        down.Pressed += down_Pressed;

        idSpin = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer2/SpinBox");
        idSpinBox = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBox2");
        framesLine = GetNode<LineEdit>("MarginContainer/VBoxContainer/GridContainer/LineEdit3");
        frameDuration = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBox");
        tipoCheckButton = GetNode<CheckButton>("MarginContainer/VBoxContainer/GridContainer/CheckButton");
        isloop = GetNode<CheckBox>("MarginContainer/VBoxContainer/GridContainer/CheckBox");
        mirrorHorizontal = GetNode<CheckBox>("MarginContainer/VBoxContainer/GridContainer/CheckBox2");
        mirrorHorizontal.Pressed += MirrorHorizontal_Pressed;
        isloop.Pressed += isloop_Press;
        tipoCheckButton.Pressed += Tipo_Pressed;
        idSpinBox.ValueChanged += IdSpinBox_ValueChanged;
        data = new AnimationStateData();
    }

    private void down_Pressed()
    {
        OnRequestOrderItem?.Invoke(1, idPosition, this);
    }

    private void up_Pressed()
    {
        OnRequestOrderItem?.Invoke(0, idPosition, this);
    }

    private void MirrorHorizontal_Pressed()
    {
        data.mirrorHorizontal = mirrorHorizontal.ButtonPressed;
        OnNotifyChangue?.Invoke(data, currentIdState);
    }

    private void isloop_Press()
    {
        data.loop = isloop.ButtonPressed;
        OnNotifyChangue?.Invoke(data, currentIdState);
    }

    private void Tipo_Pressed()
    {
        if (tipoCheckButton.ButtonPressed)
        {
            tipoCheckButton.Text = "8 Direcciones";
            data.eightDirection = true;
            idSpinBox.MaxValue = 7;
            data= new AnimationStateData(true);
        }
        else
        {
            tipoCheckButton.Text = "4 Direcciones";
            data.eightDirection = false;
            idSpinBox.MaxValue = 3;
            data = new AnimationStateData(false);
        }
        
    }

    private void IdSpinBox_ValueChanged(double value)
    {
        currentIdState = (int) value;  
        switch (currentIdState)
        {
            case 0:
                idSpinBox.Suffix = "- IZQUIERDA";
                break;
            case 1:
                idSpinBox.Suffix = "- DERECHA";
                break;
            case 2:
                idSpinBox.Suffix = "- ARRIBA";
                break;
            case 3:
                idSpinBox.Suffix = "- ABAJO";
                break;
            default:
                break;
        }
        var arrayData = data.animationData[currentIdState].idFrames;
        if (arrayData!=null)
        {
            string strFrame = "";
            foreach (var frame in arrayData)
            {
                strFrame = strFrame + "," + frame.ToString();
            }
            strFrame = strFrame.Substr(1, strFrame.Length);
            framesLine.Text = strFrame;
        }
        else
        {
            framesLine.Text = "";
        }
        frameDuration.Value = data.frameDuration;
        OnNotifyChangue?.Invoke(data, currentIdState);
    }

    private void Refresh_Press()
    {

        currentIdState = (int)idSpinBox.Value; 
        if (framesLine.Text.Contains(",") || framesLine.Text.Contains("-"))
        {
            string[] frames = framesLine.Text.Split(',');
            if (framesLine.Text.Contains("-"))
            {
                frames = framesLine.Text.Split('-');
                int ini = int.Parse(frames[0]);
                int fin = int.Parse(frames[1]);
                int[] arrayFrame = new int[(fin - ini) + 1];


                int index = 0;
                for (int i = ini; i <= fin; i++)
                {
                    arrayFrame[index] = i;            
                    index++;
                }

                data.id = (int)idSpin.Value;
                data.animationData[currentIdState].id = currentIdState;
                data.animationData[currentIdState].idFrames = arrayFrame;
                data.frameDuration = (float)frameDuration.Value;
            }
            else
            {
                int index = 0;
                int[] arrayFrame = new int[frames.Length];

                foreach (string item in frames)
                {
                    int frame = int.Parse(item);
                    arrayFrame[index] = frame;              
                    index++;
                }
                data.id = (int) idSpin.Value;
                data.animationData[currentIdState].id = currentIdState;
                data.animationData[currentIdState].idFrames = arrayFrame;
                data.frameDuration = (float)frameDuration.Value;
            }
        }
        OnNotifyChangue?.Invoke(data, currentIdState);
    }

    private void Delete_Press()
    {
        OnRequestDelete?.Invoke(this);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    int indexFrame = 0;
    double currentfps = 0;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (data.animationData[currentIdState]!= null && data.animationData[currentIdState].idFrames != null)
        {
            currentfps += delta;
            if (currentfps >= data.frameDuration && data.animationData[currentIdState].idFrames.Length > 0)
            {
                var iFrame = data.animationData[currentIdState].idFrames[indexFrame];
                indexFrame++;
                currentfps = 0;
                var dataTexture = MaterialManager.Instance.GetAtlasTexture(idMaterial, iFrame);
                textureSelection.Texture = dataTexture;
            }
            if (indexFrame >= data.animationData[currentIdState].idFrames.Length)
            {
                indexFrame = 0;
            }
        }

    }
}
