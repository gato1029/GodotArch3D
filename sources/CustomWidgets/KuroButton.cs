using Godot;
using System;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class KuroButton : TextureButton
{
    public enum TextPlacement { Bottom, Top, Left, Right }
    public enum CaptionHAlign { Left, Center, Right }
    public enum CaptionVAlign { Top, Center, Bottom }
    public enum IconAlign  { Left, Center, Right  }

    private Texture2D _iconTexture;
    private string _buttonText = "";
    private TextPlacement _textPosition = TextPlacement.Bottom;

    private CaptionHAlign _captionHAlign = CaptionHAlign.Center;
    private CaptionVAlign _captionVAlign = CaptionVAlign.Center;

    private int _spacing = 8;
    private int _padding = 6;
    private int _fontSize = 16;
    private float _iconScale = 1.0f;
    private Vector2 _iconMinSize = new Vector2(32, 32);
    private bool _iconExpand = false;
    private float _lineSpacingFactor = 0.75f;

    private Font _customFont;
    private Color _textColor = Colors.White;

    private Color _normalColor = Colors.White;
    private Color _hoverColor = new Color(0.92f, 0.92f, 0.92f);
    private Color _pressedColor = new Color(0.78f, 0.78f, 0.78f);
    private Color _disabledColor = new Color(0.6f, 0.6f, 0.6f);

    private bool _useOutline = true;
    private Color _outlineColor = Colors.Black;
    private int _outlineSize = 1;

    private bool _useShadow = true;
    private Color _shadowColor = new Color(0, 0, 0, 0.4f);
    private Vector2 _shadowOffset = new Vector2(2, 2);

    private bool _animatedScale = true;
    private float _hoverScale = 1.05f;
    private float _pressScale = 0.95f;
    private float _animationSpeed = 12f;

    private Vector2 _targetScale = Vector2.One;

    private Object _internalData; // For external use, not serialized or exposed

    private IconAlign _iconAlign = IconAlign.Left;

    
    [ExportGroup("Main")]
    [Export] public Texture2D IconTexture { get => _iconTexture; set { _iconTexture = value; RefreshControl(); } }
    [Export] public string ButtonText { get => _buttonText; set { _buttonText = value; RefreshControl(); } }
    [Export] public TextPlacement TextPosition { get => _textPosition; set { _textPosition = value; RefreshControl(); } }
    [Export] public IconAlign IconHorizontalAlignment { get => _iconAlign; set { _iconAlign = value; RefreshControl(); }  }

    [ExportGroup("Text Alignment")]
    [Export] public CaptionHAlign TextHorizontalAlignment { get => _captionHAlign; set { _captionHAlign = value; RefreshControl(); } }
    [Export] public CaptionVAlign TextVerticalAlignment { get => _captionVAlign; set { _captionVAlign = value; RefreshControl(); } }

    [ExportGroup("Layout")]
    [Export] public int Spacing { get => _spacing; set { _spacing = value; RefreshControl(); } }
    [Export] public int Padding { get => _padding; set { _padding = value; RefreshControl(); } }
    [Export] public int FontSize { get => _fontSize; set { _fontSize = value; RefreshControl(); } }
    [Export(PropertyHint.Range, "0.1,1.0,0.05")] public float IconScale { get => _iconScale; set { _iconScale = value; RefreshControl(); } }
    [Export] public Vector2 IconMinSize { get => _iconMinSize; set { _iconMinSize = value; RefreshControl(); } }
    [Export] public bool IconExpand { get => _iconExpand; set { _iconExpand = value; RefreshControl(); } }
    [Export(PropertyHint.Range, "0.5,1.2,0.01")] public float LineSpacingFactor { get => _lineSpacingFactor; set { _lineSpacingFactor = value; RefreshControl(); } }

    [Export] public Font CustomFont { get => _customFont; set { _customFont = value; RefreshControl(); } }

    [ExportGroup("Colors")]
    [Export] public Color TextColor { get => _textColor; set { _textColor = value; RefreshControl(); } }
    [Export] public Color NormalColor { get => _normalColor; set { _normalColor = value; RefreshControl(); } }
    [Export] public Color HoverColor { get => _hoverColor; set { _hoverColor = value; RefreshControl(); } }
    [Export] public Color PressedColor { get => _pressedColor; set { _pressedColor = value; RefreshControl(); } }
    [Export] public Color DisabledColor { get => _disabledColor; set { _disabledColor = value; RefreshControl(); } }

    [ExportGroup("Outline")]
    [Export] public bool UseOutline { get => _useOutline; set { _useOutline = value; RefreshControl(); } }
    [Export] public Color OutlineColor { get => _outlineColor; set { _outlineColor = value; RefreshControl(); } }
    [Export] public int OutlineSize { get => _outlineSize; set { _outlineSize = value; RefreshControl(); } }

    [ExportGroup("Shadow")]
    [Export] public bool UseShadow { get => _useShadow; set { _useShadow = value; RefreshControl(); } }
    [Export] public Color ShadowColor { get => _shadowColor; set { _shadowColor = value; RefreshControl(); } }
    [Export] public Vector2 ShadowOffset { get => _shadowOffset; set { _shadowOffset = value; RefreshControl(); } }

    [ExportGroup("Animation")]
    [Export] public bool AnimatedScale { get => _animatedScale; set => _animatedScale = value; }
    [Export] public float HoverScale { get => _hoverScale; set => _hoverScale = value; }
    [Export] public float PressScale { get => _pressScale; set => _pressScale = value; }
    [Export] public float AnimationSpeed { get => _animationSpeed; set => _animationSpeed = value; }

    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        ButtonDown += OnButtonDown;
        ButtonUp += OnButtonUp;

        RefreshControl();
    }
    public void SetInternalData(Object data) => _internalData = data;
    public Object GetInternalData() => _internalData;

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint() && _animatedScale)
            Scale = Scale.Lerp(_targetScale, (float)delta * _animationSpeed);

        QueueRedraw();
    }

    private Font GetWorkingFont() => _customFont ?? GetThemeDefaultFont();

    private void RefreshControl()
    {
        CustomMinimumSize = _GetMinimumSize();
        UpdateMinimumSize();
        QueueRedraw();
    }

    private List<string> WrapText(Font font, string text, float maxWidth)
    {
        List<string> lines = new();
        if (string.IsNullOrEmpty(text) || font == null)
        {
            lines.Add(text);
            return lines;
        }

        string[] words = text.Split(' ');
        string current = "";

        foreach (string word in words)
        {
            string test = string.IsNullOrEmpty(current) ? word : current + " " + word;
            float width = font.GetStringSize(test, HorizontalAlignment.Left, -1, _fontSize).X;

            if (width > maxWidth && !string.IsNullOrEmpty(current))
            {
                lines.Add(current);
                current = word;
            }
            else
            {
                current = test;
            }
        }

        if (!string.IsNullOrEmpty(current))
            lines.Add(current);

        return lines;
    }

    private Vector2 MeasureWrappedText(Font font, List<string> lines, float maxWidth)
    {
        if (font == null || lines.Count == 0) return Vector2.Zero;

        float maxLine = 0f;
        float lineHeight = (font.GetAscent(_fontSize) + font.GetDescent(_fontSize)) * _lineSpacingFactor;

        foreach (string line in lines)
        {
            float w = font.GetStringSize(line, HorizontalAlignment.Left, -1, _fontSize).X;
            if (w > maxLine) maxLine = w;
        }

        return new Vector2(Mathf.Min(maxLine, maxWidth), lineHeight * lines.Count);
    }

    public override Vector2 _GetMinimumSize()
    {
        Vector2 iconSize = _iconMinSize * _iconScale;

        if (string.IsNullOrEmpty(_buttonText))
            return iconSize;

        Font font = GetWorkingFont();

        float maxTextWidth = (_textPosition == TextPlacement.Left || _textPosition == TextPlacement.Right)
            ? 160
            : iconSize.X;

        List<string> lines = WrapText(font, _buttonText, maxTextWidth);
        Vector2 textSize = MeasureWrappedText(font, lines, maxTextWidth);

        int effectiveSpacing = _spacing;

        if (_textPosition == TextPlacement.Bottom || _textPosition == TextPlacement.Top)
        {
            return new Vector2(
                Mathf.Max(iconSize.X, textSize.X),
                iconSize.Y + textSize.Y + effectiveSpacing
            ) + new Vector2(_padding * 2, _padding * 2);
        }
        else
        {
            return new Vector2(
                iconSize.X + textSize.X + effectiveSpacing,
                Mathf.Max(iconSize.Y, textSize.Y)
            ) + new Vector2(_padding * 2, _padding * 2);
        }
    }

    public override void _Draw()
    {
        DrawCompleteButton();
    }

    private void DrawCompleteButton()
    {
        if (string.IsNullOrEmpty(_buttonText))
        {
            Rect2 soloRect = new Rect2(Vector2.Zero, Size);
            Rect2 fittedSoloIcon = _iconExpand ? soloRect : GetAspectFitRect(_iconTexture, soloRect, _iconScale);

            if (_iconTexture != null)
                DrawTextureRect(_iconTexture, fittedSoloIcon, false, GetCurrentColor());
            return;
        }

        Font font = GetWorkingFont();
        if (font == null) return;

        Rect2 contentRect = new Rect2(new Vector2(_padding, _padding), Size - new Vector2(_padding * 2, _padding * 2));

        float maxTextWidth = (_textPosition == TextPlacement.Left || _textPosition == TextPlacement.Right)
            ? Mathf.Max(80, Size.X * 0.45f)
            : Size.X - (_padding * 2);

        List<string> lines = WrapText(font, _buttonText, maxTextWidth);
        Vector2 textSize = MeasureWrappedText(font, lines, maxTextWidth);
        int effectiveSpacing = string.IsNullOrEmpty(_buttonText) ? 0 : _spacing;

        if (_textPosition == TextPlacement.Left || _textPosition == TextPlacement.Right)
        {
            DrawHorizontalLayout(font, contentRect, lines, textSize, effectiveSpacing);
            return;
        }

        DrawVerticalLayout(font, contentRect, lines, textSize, effectiveSpacing);
    }

    private void DrawHorizontalLayout(Font font, Rect2 contentRect, List<string> lines, Vector2 textSize, int spacing)
    {
        Vector2 rawIconSize = _iconMinSize * _iconScale;

        float iconX = contentRect.Position.X;

        switch (_iconAlign)
        {
            case IconAlign.Center:
                iconX = contentRect.Position.X +
                        (contentRect.Size.X - rawIconSize.X) * 0.5f;
                break;

            case IconAlign.Right:
                iconX = contentRect.End.X - rawIconSize.X;
                break;

            case IconAlign.Left:
            default:
                iconX = contentRect.Position.X;
                break;
        }

        Rect2 iconRect = new();
        Rect2 textRect = new();

        if (_textPosition == TextPlacement.Left)
        {
            // Icono a la izquierda, texto a la derecha
            iconRect = new Rect2(
                iconX,
                contentRect.Position.Y,
                rawIconSize.X,
                contentRect.Size.Y
            );

            textRect = new Rect2(
                iconRect.End.X + spacing,
                contentRect.Position.Y,
                contentRect.End.X - (iconRect.End.X + spacing),
                contentRect.Size.Y
            );
        }
        else
        {
            // Texto a la izquierda, icono a la derecha
            iconRect = new Rect2(
                iconX,
                contentRect.Position.Y,
                rawIconSize.X,
                contentRect.Size.Y
            );

            textRect = new Rect2(
                contentRect.Position.X,
                contentRect.Position.Y,
                iconRect.Position.X - spacing - contentRect.Position.X,
                contentRect.Size.Y
            );
        }

        Rect2 fittedIcon = _iconExpand
            ? iconRect
            : GetAspectFitRect(_iconTexture, iconRect, 1f);

        if (_iconTexture != null)
            DrawTextureRect(_iconTexture, fittedIcon, false, GetCurrentColor());

        DrawCaption(font, lines, textRect);
    }

    private void DrawVerticalLayout(Font font, Rect2 contentRect, List<string> lines, Vector2 textSize, int spacing)
    {
        Rect2 textRect;
        Rect2 iconRect;

        float reserved = textSize.Y + spacing;

        if (_textPosition == TextPlacement.Bottom)
        {
            iconRect = new Rect2(contentRect.Position, new Vector2(contentRect.Size.X, contentRect.Size.Y - reserved));
            textRect = new Rect2(new Vector2(contentRect.Position.X, iconRect.End.Y + spacing), new Vector2(contentRect.Size.X, textSize.Y));
        }
        else
        {
            textRect = new Rect2(contentRect.Position, new Vector2(contentRect.Size.X, textSize.Y));
            iconRect = new Rect2(new Vector2(contentRect.Position.X, textRect.End.Y + spacing), new Vector2(contentRect.Size.X, contentRect.Size.Y - reserved));
        }

        Rect2 fittedIcon = _iconExpand ? iconRect : GetAspectFitRect(_iconTexture, iconRect, _iconScale);

        if (_iconTexture != null)
            DrawTextureRect(_iconTexture, fittedIcon, false, GetCurrentColor());

        DrawCaption(font, lines, textRect);
    }

    private void DrawCaption(Font font, List<string> lines, Rect2 textRect)
    {
        if (lines == null || lines.Count == 0 || string.IsNullOrEmpty(_buttonText)) return;

        float lineHeight = (font.GetAscent(_fontSize) + font.GetDescent(_fontSize)) * _lineSpacingFactor;
        float totalHeight = lineHeight * lines.Count;

        float startY = textRect.Position.Y + font.GetAscent(_fontSize);

        switch (_captionVAlign)
        {
            case CaptionVAlign.Center:
                startY = textRect.Position.Y + (textRect.Size.Y - totalHeight) / 2f + font.GetAscent(_fontSize);
                break;
            case CaptionVAlign.Bottom:
                startY = textRect.End.Y - totalHeight + font.GetAscent(_fontSize);
                break;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            float lineWidth = font.GetStringSize(line, HorizontalAlignment.Left, -1, _fontSize).X;
            float x = textRect.Position.X;

            switch (_captionHAlign)
            {
                case CaptionHAlign.Center:
                    x = textRect.Position.X + (textRect.Size.X - lineWidth) / 2f;
                    break;
                case CaptionHAlign.Right:
                    x = textRect.End.X - lineWidth;
                    break;
            }

            Vector2 pos = new Vector2(x, startY + (i * lineHeight));

            if (_useShadow)
                DrawString(font, pos + _shadowOffset, line, HorizontalAlignment.Left, -1, _fontSize, _shadowColor);

            if (_useOutline)
            {
                for (int ox = -_outlineSize; ox <= _outlineSize; ox++)
                    for (int oy = -_outlineSize; oy <= _outlineSize; oy++)
                    {
                        if (ox == 0 && oy == 0) continue;
                        DrawString(font, pos + new Vector2(ox, oy), line, HorizontalAlignment.Left, -1, _fontSize, _outlineColor);
                    }
            }

            DrawString(font, pos, line, HorizontalAlignment.Left, -1, _fontSize, _textColor);
        }
    }

    private Rect2 GetAspectFitRect(Texture2D tex, Rect2 target, float scale)
    {
        if (tex == null) return target;

        Vector2 texSize = tex.GetSize();
        float ratio = Mathf.Min(target.Size.X / texSize.X, target.Size.Y / texSize.Y) * scale;
        Vector2 finalSize = texSize * ratio;
        Vector2 finalPos = target.Position + (target.Size - finalSize) / 2f;

        return new Rect2(finalPos, finalSize);
    }

    private Color GetCurrentColor()
    {
        switch (GetDrawMode())
        {
            case DrawMode.Hover: return _hoverColor;
            case DrawMode.Pressed:
            case DrawMode.HoverPressed: return _pressedColor;
            case DrawMode.Disabled: return _disabledColor;
            default: return _normalColor;
        }
    }

    private void OnMouseEntered() { if (_animatedScale) _targetScale = new Vector2(_hoverScale, _hoverScale); }
    private void OnMouseExited() { if (_animatedScale) _targetScale = Vector2.One; }
    private void OnButtonDown() { if (_animatedScale) _targetScale = new Vector2(_pressScale, _pressScale); }
    private void OnButtonUp() { if (_animatedScale) _targetScale = new Vector2(_hoverScale, _hoverScale); }
}
