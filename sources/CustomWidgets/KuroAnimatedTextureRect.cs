using System;
using Godot;

[Tool]
[GlobalClass]
public partial class KuroAnimatedTextureRect : TextureRect
{
    private int _hframes = 1;
    private int _vframes = 1;
    private int _frame = 0;
    private float _fps = 8.0f;
    private bool _isPlaying = false;

    private Texture2D? _texturaBase;
    private AtlasTexture? _frameTexture;
    private Vector2 _currentFrameSize = Vector2.Zero;
    private float _timer = 0.0f;
    private bool _updatingTexture = false;

    public Texture2D? TexturaBase => _texturaBase;

    [Export]
    public new Texture2D? Texture
    {
        get => base.Texture;
        set
        {
            if (_updatingTexture)
            {
                base.Texture = value;
                return;
            }

            if (value == _frameTexture)
            {
                ApplyFrame();
                return;
            }

            BuildTexturaBase(value);
            ApplyFrame();
        }
    }

    [ExportGroup("Spritesheet Grid")]
    [Export(PropertyHint.Range, "1,256,1")]
    public int Hframes
    {
        get => _hframes;
        set
        {
            _hframes = Math.Max(1, value);
            ApplyFrame();
        }
    }

    [Export(PropertyHint.Range, "1,256,1")]
    public int Vframes
    {
        get => _vframes;
        set
        {
            _vframes = Math.Max(1, value);
            ApplyFrame();
        }
    }

    [Export]
    public int Frame
    {
        get => _frame;
        set
        {
            int total = GetFrameCount();
            _frame = total > 0 ? PosMod(value, total) : 0;
            ApplyFrame();
        }
    }

    [ExportGroup("Playback")]
    [Export]
    public bool Autoplay { get; set; }

    [Export]
    public bool Loop { get; set; } = true;

    [Export(PropertyHint.Range, "0.1,60.0,0.1,suffix:fps")]
    public float Fps
    {
        get => _fps;
        set => _fps = Math.Max(0.1f, value);
    }

    [Export]
    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            _isPlaying = value;
            _timer = 0.0f;
            SetProcess(!Engine.IsEditorHint() && _isPlaying);
        }
    }

    public override void _Ready()
    {
        if (base.Texture != null && _texturaBase == null)
        {
            BuildTexturaBase(base.Texture);
        }

        ApplyFrame();

        if (!Engine.IsEditorHint())
        {
            SetProcess(_isPlaying);
            if (Autoplay)
            {
                Play();
            }
        }
        else
        {
            SetProcess(false);
        }
    }

    public override Vector2 _GetMinimumSize()
    {
        if (_currentFrameSize.X > 0.0f && _currentFrameSize.Y > 0.0f)
        {
            return _currentFrameSize;
        }
        return base._GetMinimumSize();
    }

    private void BuildTexturaBase(Texture2D? input)
    {
        _frameTexture = null;
        _currentFrameSize = Vector2.Zero;

        if (input == null)
        {
            _texturaBase = null;
            SetBaseTextureInternal(null);
            return;
        }

        if (input is AtlasTexture atlas)
        {
            _texturaBase = new AtlasTexture
            {
                Atlas = atlas.Atlas,
                Region = atlas.Region,
                FilterClip = true
            };
        }
        else
        {
            _texturaBase = input;
        }

        Texture2D? sheet = GetSheetTexture(_texturaBase);
        if (sheet != null)
        {
            _frameTexture = new AtlasTexture
            {
                Atlas = sheet,
                FilterClip = true
            };
        }
    }

    private static Texture2D? GetSheetTexture(Texture2D? tex)
    {
        if (tex is AtlasTexture atlas)
        {
            return atlas.Atlas;
        }
        return tex;
    }

    private Rect2 GetBaseRect()
    {
        if (_texturaBase == null)
        {
            return default;
        }

        if (_texturaBase is AtlasTexture atlas && atlas.Region.Size.X > 0.0f && atlas.Region.Size.Y > 0.0f)
        {
            return atlas.Region;
        }

        Vector2 size = _texturaBase.GetSize();
        return new Rect2(Vector2.Zero, size);
    }

    public int GetFrameCount() => _hframes * _vframes;

    public void Play(int fromFrame = -1)
    {
        if (fromFrame >= 0)
        {
            Frame = fromFrame;
        }
        IsPlaying = true;
    }

    public void Pause() => IsPlaying = false;

    public void Stop()
    {
        IsPlaying = false;
        Frame = 0;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() || !_isPlaying || _fps <= 0.0f)
        {
            return;
        }

        int total = GetFrameCount();
        if (total <= 1)
        {
            return;
        }

        _timer += (float)delta;
        float frameDuration = 1.0f / _fps;

        while (_timer >= frameDuration)
        {
            _timer -= frameDuration;

            if (_frame + 1 < total)
            {
                Frame = _frame + 1;
            }
            else if (Loop)
            {
                Frame = 0;
            }
            else
            {
                _frame = total - 1;
                ApplyFrame();
                IsPlaying = false;
                break;
            }
        }
    }

    private void ApplyFrame()
    {
        if (_texturaBase == null)
        {
            return;
        }

        Texture2D? sheet = GetSheetTexture(_texturaBase);
        if (sheet == null)
        {
            return;
        }

        Rect2 baseRect = GetBaseRect();
        if (baseRect.Size.X <= 0.0f || baseRect.Size.Y <= 0.0f)
        {
            return;
        }

        float frameW = baseRect.Size.X / _hframes;
        float frameH = baseRect.Size.Y / _vframes;
        _currentFrameSize = new Vector2(frameW, frameH);

        int total = GetFrameCount();
        int safeFrame = total > 0 ? PosMod(_frame, total) : 0;
        int col = safeFrame % _hframes;
        int row = safeFrame / _hframes;

        Rect2 frameRect = new Rect2(
            baseRect.Position.X + col * frameW,
            baseRect.Position.Y + row * frameH,
            frameW,
            frameH
        );

        _frameTexture = new AtlasTexture
        {
            Atlas = sheet,
            Region = frameRect,
            FilterClip = true
        };

        SetBaseTextureInternal(_frameTexture);
        UpdateMinimumSize();

        // En el editor, si no está expandiéndose libremente, ajusta Size al tamaño exacto del frame (ej. 64x64)
        if (Engine.IsEditorHint() && ExpandMode == ExpandModeEnum.KeepSize)
        {
            Size = _currentFrameSize;
        }

        QueueRedraw();
    }

    private void SetBaseTextureInternal(Texture2D? tex)
    {
        _updatingTexture = true;
        try
        {
            base.Texture = tex;
        }
        finally
        {
            _updatingTexture = false;
        }
    }

    private static int PosMod(int value, int modulus)
    {
        int r = value % modulus;
        return r < 0 ? r + modulus : r;
    }
}