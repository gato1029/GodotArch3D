using Godot;

[GlobalClass]
[Tool]
public partial class AnimatedTextureRect : TextureRect
{
    // =========================================
    // SOURCE TEXTURE
    // =========================================

    private Texture2D _sourceTexture;

    [ExportGroup("Source")]

    [Export]
    public Texture2D SourceTexture
    {
        get => _sourceTexture;
        set
        {
            _sourceTexture = value;
            UpdateFrame();
        }
    }

    // =========================================
    // ANIMATION SETTINGS
    // =========================================

    private Rect2 _region = new(0, 0, 0, 0);
    private int _hFrames = 1;
    private int _vFrames = 1;
    private int _maxFrameIndex = -1;
    private float _fps = 12.0f;
    private bool _loop = true;
    private bool _autoplay = true;

    [ExportGroup("Animation")]

    

    [Export]
    public int MaxFrameIndex
    {
        get => _maxFrameIndex;
        set
        {
            _maxFrameIndex = value;
            ValidateFrameIndex();
            UpdateFrame();
        }
    }
    [Export]
    public Rect2 Region
    {
        get => _region;
        set
        {
            _region = value;
            UpdateFrame();
        }
    }

    [Export(PropertyHint.Range, "1,64,1")]
    public int HFrames
    {
        get => _hFrames;
        set
        {
            _hFrames = Mathf.Max(1, value);
            ValidateFrameIndex();
            UpdateFrame();
        }
    }

    [Export(PropertyHint.Range, "1,64,1")]
    public int VFrames
    {
        get => _vFrames;
        set
        {
            _vFrames = Mathf.Max(1, value);
            ValidateFrameIndex();
            UpdateFrame();
        }
    }

    [Export(PropertyHint.Range, "0,60,0.1")]
    public float FPS
    {
        get => _fps;
        set => _fps = Mathf.Max(0, value);
    }

    [Export]
    public bool Loop
    {
        get => _loop;
        set => _loop = value;
    }

    [Export]
    public bool Autoplay
    {
        get => _autoplay;
        set => _autoplay = value;
    }

    // =========================================
    // PLAYBACK
    // =========================================

    private int _frameIndex;
    private float _elapsedTime;
    private bool _playing;

    [ExportGroup("Playback")]

    [Export(PropertyHint.Range, "0,4095,1")]
    public int FrameIndex
    {
        get => _frameIndex;
        set
        {
            _frameIndex = Mathf.Clamp(
                value,
                0,
                GetTotalFrames() - 1
            );

            UpdateFrame();
        }
    }

    [Export]
    public bool Playing
    {
        get => _playing;
        set => _playing = value;
    }

    public override void _ValidateProperty(Godot.Collections.Dictionary property)
    {
        if (property["name"].AsString() == "texture")
        {
            property["usage"] = (int)PropertyUsageFlags.None;
        }
    }
    // =========================================
    // INITIALIZATION
    // =========================================

    public override void _Ready()
    {
        UpdateFrame();

        if (Autoplay)
            Play();
    }

    // =========================================
    // PROCESS
    // =========================================

    public override void _Process(double delta)
    {
        if (!_playing || FPS <= 0)
            return;

        _elapsedTime += (float)delta;

        float frameDuration = 1.0f / FPS;

        while (_elapsedTime >= frameDuration && _playing)
        {
            _elapsedTime -= frameDuration;
            AdvanceFrame();
        }
    }

    // =========================================
    // FRAME MANAGEMENT
    // =========================================

    private int GetTotalFrames()
    {
        int totalFrames = Mathf.Max(1, HFrames * VFrames);

        if (MaxFrameIndex < 0)
            return totalFrames;

        return Mathf.Clamp(MaxFrameIndex + 1, 1, totalFrames);
    }

    private void ValidateFrameIndex()
    {
        _frameIndex = Mathf.Clamp(
            _frameIndex,
            0,
            GetTotalFrames() - 1
        );
    }

    private void AdvanceFrame()
    {
        NextFrame();
    }

    // =========================================
    // FRAME RENDERING
    // =========================================

    private void UpdateFrame()
    {
        if (_sourceTexture == null)
        {
            base.Texture = null;
            return;
        }

        Vector2 textureSize = _sourceTexture.GetSize();

        Rect2 animationRegion = Region;

        if (animationRegion.Size.X <= 0 ||
            animationRegion.Size.Y <= 0)
        {
            animationRegion = new Rect2(
                Vector2.Zero,
                textureSize
            );
        }

        float frameWidth = animationRegion.Size.X / HFrames;
        float frameHeight = animationRegion.Size.Y / VFrames;

        int column = FrameIndex % HFrames;
        int row = FrameIndex / HFrames;

        Rect2 frameRegion = new Rect2(
            animationRegion.Position.X + column * frameWidth,
            animationRegion.Position.Y + row * frameHeight,
            frameWidth,
            frameHeight
        );

        AtlasTexture frameTexture = new AtlasTexture
        {
            Atlas = _sourceTexture,
            Region = frameRegion
        };

        base.Texture = frameTexture;
    }

    // =========================================
    // PUBLIC API
    // =========================================
    public void NextFrame()
    {
        int lastFrame = GetTotalFrames() - 1;

        if (_frameIndex >= lastFrame)
        {
            if (Loop)
                FrameIndex = 0;
            else
                Stop();

            return;
        }

        FrameIndex++;
    }
    public void Play()
    {
        _elapsedTime = 0;
        _playing = true;
    }

    public void Stop()
    {
        _playing = false;
        _elapsedTime = 0;
        FrameIndex = 0;
    }

    public void Pause()
    {
        _playing = false;
    }

    public void Resume()
    {
        _playing = true;
    }

    public void SetFrame(int frame)
    {
        FrameIndex = frame;
    }
}