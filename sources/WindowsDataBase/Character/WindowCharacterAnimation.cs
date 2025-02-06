using Godot;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;

public partial class WindowCharacterAnimation : PanelContainer
{
	public CharacterAnimationStateData data;

    public delegate void RequestDeleteHandler(WindowCharacterAnimation item);
    public event RequestDeleteHandler OnRequestDelete;

    SpinBox idSpinBox;
    LineEdit framesLine;
    SpinBox frameDuration;
    TextureRect textureSelection;
    CheckButton tipoCheckButton;
    int idMaterial;
    // Called when the node enters the scene tree for the first time.

    int currentIdState = 0;
    public void SetMaterial(int idMat)
    {
        idMaterial = idMat;
    }
    public override void _Ready()
	{
        GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/Button2").Pressed += Delete_Press;
        GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/Button").Pressed += Refresh_Press;
        textureSelection = GetNode<TextureRect>("MarginContainer/VBoxContainer/TextureRect");

        idSpinBox = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBox2");
        framesLine = GetNode<LineEdit>("MarginContainer/VBoxContainer/GridContainer/LineEdit3");
        frameDuration = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBox");
        tipoCheckButton = GetNode<CheckButton>("MarginContainer/VBoxContainer/GridContainer/CheckButton");

        tipoCheckButton.Pressed += Tipo_Pressed;
        idSpinBox.ValueChanged += IdSpinBox_ValueChanged;
        data = new CharacterAnimationStateData();
    }

    private void Tipo_Pressed()
    {
        if (tipoCheckButton.ButtonPressed)
        {
            tipoCheckButton.Text = "8 Direcciones";
            data.eightDirection = true;
            idSpinBox.MaxValue = 7;
            data= new CharacterAnimationStateData(true);
        }
        else
        {
            tipoCheckButton.Text = "4 Direcciones";
            data.eightDirection = false;
            idSpinBox.MaxValue = 3;
            data = new CharacterAnimationStateData(false);
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
                data.animationData[currentIdState].id = currentIdState;
                data.animationData[currentIdState].idFrames = arrayFrame;
                data.frameDuration = (float)frameDuration.Value;
            }
        }
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
