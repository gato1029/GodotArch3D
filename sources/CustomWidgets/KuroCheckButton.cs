using Godot;
using System;

[Tool]
[GlobalClass]
public partial class KuroCheckButton : CheckButton
{
    [Export]
    public Texture2D TextureOn
    {
        get => _textureOn;
        set
        {
            _textureOn = value;
            UpdateIcon();
        }
    }

    [Export]
    public Texture2D TextureOff
    {
        get => _textureOff;
        set
        {
            _textureOff = value;
            UpdateIcon();
        }
    }

    private Texture2D _textureOn;
    private Texture2D _textureOff;

    public override void _Ready()
    {
        Text = "";

        Toggled += (_) => UpdateIcon();

        UpdateIcon();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree || what == NotificationReady)
        {
            UpdateIcon();
        }    
    }
    public override void _ValidateProperty(Godot.Collections.Dictionary property)
    {
        UpdateIcon();
    }
    private void UpdateIcon()
    {
        if (!IsInsideTree()) return;

        Icon = ButtonPressed ? TextureOn : TextureOff;

        QueueRedraw(); // ayuda en editor      
        UpdateMinimumSize(); // 🔥 importante
   
    }
}