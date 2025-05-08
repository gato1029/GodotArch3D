using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;

public partial class AnimationScene : VBoxContainer
{
    [Export] Button buttonUp;
    [Export] Button buttonDown;
    [Export] Button buttonRemove;
    [Export] Button buttonPrevisualizar;

    [Export] SpinBox idSpin;
    [Export] SpinBox idDirectionSpinBox;
    [Export] LineEdit framesLine;
    [Export] SpinBox frameDuration;
    [Export] TextureRect textureSelection;
    [Export] CheckBox isloop;
    [Export] CheckBox mirrorHorizontal;
    [Export] ColliderScene colliderScene;
    [Export] CheckBox hasCollision;

    public delegate void RequestNotifyPreview(AnimationStateData itemData, int currentIdState);
    public event RequestNotifyPreview OnNotifyPreview;

    bool eightDirection = false;
    public AnimationStateData data;


    WindowState windowState;
    int idMaterial;
    int currentIdState = 0;
    int indexFrame = 0;
    double currentfps = 0;
    
    public void SetDataBase(int id, bool EightDirection)
    {
        
        eightDirection = EightDirection;
        idSpin.Value = id;        
    }
    public void SetMaterialID(int IdMaterial)
    {
        idMaterial = IdMaterial;
    }
    public void SetData(AnimationStateData In_data, int IdMaterial, WindowState InWindowState= WindowState.SELECTOR)
    {
        idMaterial = IdMaterial;
        windowState = InWindowState;
        if (In_data.animationData[0].idFrames != null)
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
            idDirectionSpinBox.Value = 0;         
            
        }
        if (data.animationData[currentIdState].hasCollider)
        {
            colliderScene.Visible = data.animationData[currentIdState].hasCollider;
            hasCollision.ButtonPressed = data.animationData[currentIdState].hasCollider;
            if (data.animationData[currentIdState].collider != null)
            {
                var dataCollider = data.animationData[currentIdState].collider;
                colliderScene.SetData(dataCollider);
            }
        }

        switch (windowState)
        {
            case WindowState.NEW:
                break;
            case WindowState.UPDATE:
                colliderScene.SetOcluccionButton();
                break;
            case WindowState.CRUD:
                break;
            case WindowState.SELECTOR:
                buttonDown.Disabled = true;
                buttonUp.Disabled = true;              
                buttonRemove.Visible = false;
                colliderScene.SetOcluccionButton();
                break;
            default:
                break;
        }
        OnNotifyPreview?.Invoke(data, currentIdState);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        windowState = WindowState.NEW;
        data = new AnimationStateData();
        buttonDown.Pressed += ButtonDown_Pressed;
        buttonUp.Pressed += ButtonUp_Pressed;
        buttonRemove.Pressed += ButtonRemove_Pressed;
        buttonPrevisualizar.Pressed += ButtonPrevisualizar_Pressed;

        mirrorHorizontal.Pressed += MirrorHorizontal_Pressed;
        isloop.Pressed += isloop_Press;

        idDirectionSpinBox.ValueChanged += idDirectionSpinBox_ValueChanged;
        frameDuration.ValueChanged += FrameDuration_ValueChanged;
        framesLine.TextSubmitted += FramesLine_TextSubmitted;

        hasCollision.Pressed += HasCollision_Pressed;
        colliderScene.OnNotifyPreview += ColliderScene_OnNotifyPreview;
    }

    private void HasCollision_Pressed()
    {
        if (hasCollision.ButtonPressed)
        {
            colliderScene.Visible = true;
            data.animationData[currentIdState].hasCollider = true;
            if (data.animationData[currentIdState].collider != null)
            {                
                var dataCollider = data.animationData[currentIdState].collider;
                colliderScene.SetData(dataCollider);
            }
            else
            {
                colliderScene.SetData(new Rectangle(0,0,0,0));
            }
        }
        else
        {
            colliderScene.Visible = false;
            data.animationData[currentIdState].hasCollider = false;
            data.animationData[currentIdState].collider = null;
        }
    }

    private void ButtonPrevisualizar_Pressed()
    {
        OnNotifyPreview?.Invoke(data,currentIdState);
    }

    private void FramesLine_TextSubmitted(string newText)
    {
        Refresh_Press();
    }

    private void FrameDuration_ValueChanged(double value)
    {
        data.frameDuration = (float)frameDuration.Value;
    }

    private void MirrorHorizontal_Pressed()
    {
        data.mirrorHorizontal = mirrorHorizontal.ButtonPressed;
     
    }
    private void Refresh_Press()
    {

        currentIdState = (int)idDirectionSpinBox.Value;
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
                data.id = (int)idSpin.Value;
                data.animationData[currentIdState].id = currentIdState;
                data.animationData[currentIdState].idFrames = arrayFrame;
                data.frameDuration = (float)frameDuration.Value;
            }
        }
       
    }
    private void isloop_Press()
    {
        data.loop = isloop.ButtonPressed;
    }

    private void idDirectionSpinBox_ValueChanged(double value)
    {
        currentIdState = (int)value;
        switch (currentIdState)
        {
            case 0:
                idDirectionSpinBox.Suffix = "- IZQUIERDA";
                break;
            case 1:
                idDirectionSpinBox.Suffix = "- DERECHA";
                break;
            case 2:
                idDirectionSpinBox.Suffix = "- ARRIBA";
                break;
            case 3:
                idDirectionSpinBox.Suffix = "- ABAJO";
                break;
            default:
                break;
        }
        var arrayData = data.animationData[currentIdState].idFrames;
        if (arrayData != null)
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

        ChangueCollider(currentIdState);
    }

    private void ChangueCollider(int currentIdState)
    {
        colliderScene.Visible = data.animationData[currentIdState].hasCollider;             
        hasCollision.ButtonPressed = data.animationData[currentIdState].hasCollider;

        if (data.animationData[currentIdState].collider!=null)
        {
            var dataCollider = data.animationData[currentIdState].collider;
            colliderScene.SetData(dataCollider);
        }
                                  
    }

    private void ColliderScene_OnNotifyPreview(GodotEcsArch.sources.managers.Collision.GeometricShape2D itemData)
    {
        data.animationData[currentIdState].collider = itemData;
    }

    private void ButtonRemove_Pressed()
    {
        QueueFree();
    }

    private void ButtonUp_Pressed()
    {
        throw new NotImplementedException();
    }

    private void ButtonDown_Pressed()
    {
        throw new NotImplementedException();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (data.animationData[currentIdState] != null && data.animationData[currentIdState].idFrames != null)
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
