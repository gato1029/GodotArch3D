using Godot;
using System;
using System.Reflection;
[GlobalClass]
public partial class KuroTextureButton : TextureButton
{
    [Export]
    public Color NormalColor
    {
        get => _normalColor;
        set
        {
            _normalColor = value;
            UpdateColor();
        }
    }

    [Export]
    public Color PressedColor
    {
        get => _pressedColor;
        set
        {
            _pressedColor = value;
            UpdateColor();
        }
    }

    [Export]
    public Color HoverColor
    {
        get => _hoverColor;
        set
        {
            _hoverColor = value;
            UpdateColor();
        }
    }

    [Export]
    public Color DisabledColor
    {
        get => _disabledColor;
        set
        {
            _disabledColor = value;
            UpdateColor();
        }
    }

    private Color _normalColor = Colors.White;
    private Color _pressedColor = new Color(0.784f, 0.784f, 0.784f);
    private Color _hoverColor = new Color(0.922f, 0.922f, 0.922f);
    private Color _disabledColor = new Color(0.627f, 0.627f, 0.627f);

    public override void _Ready()
    {
        // Generar mapa de clic desde la textura normal
        if (TextureNormal is Texture2D tex)
        {
            var image = tex.GetImage();
            var bitmap = new Bitmap();
            bitmap.CreateFromImageAlpha(image);
            //TextureClickMask = bitmap;
        }

        UpdateColor();
    }

    public override void _Draw()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        var drawMode = GetDrawMode();

        switch (drawMode)
        {
            case DrawMode.HoverPressed:
                Modulate = PressedColor;
                break;
            case DrawMode.Pressed:
                Modulate = PressedColor;
                break;
            case DrawMode.Normal:
                Modulate = NormalColor;
                break;
            case DrawMode.Hover:
                Modulate = HoverColor;
                break;
            case DrawMode.Disabled:
                Modulate = DisabledColor;
                break;
        }
    }
}
