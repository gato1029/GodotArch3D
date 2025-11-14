using Godot;
using System.Collections.Generic;
using static Godot.Control;
using static Godot.DisplayServer;

public partial class GameLogConsole : Window
{
    private static GameLogConsole _instance;
    public static GameLogConsole Instance => _instance;

    private TextEdit _textEdit;
    private Queue<string> _lines = new();
    private Queue<string> _pendingLines = new(); // ← Buffer de mensajes previos
    private bool _initialized = false;

    [Export] public int MaxLines = 200;
    // 👈 Tecla configurable para toggle (por defecto F1)
    [Export] public Key ToggleKey = Key.F5;
    public override void _Ready()
    {
        _instance = this;
        InitializeUI();

        // Procesar mensajes pendientes
        while (_pendingLines.Count > 0)
            AddLine(_pendingLines.Dequeue());
    }
    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(ToggleKey))
        {
            // Evitar múltiples toggles en un solo frame
            if (!_toggleLock)
            {
                Visible = !Visible;
                _toggleLock = true;
            }
        }
        else
        {
            _toggleLock = false;
        }
    }

    private bool _toggleLock = false;
    private void InitializeUI()
    {
        if (_initialized) return;

        Title = "Game Log Console";
        Size = new Vector2I(800, 600);
        Unresizable = false;

        // Fondo contenedor
        var panel = new PanelContainer
        {
            AnchorsPreset = (int)LayoutPreset.FullRect,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        panel.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(panel);

        // TextEdit que llena el Panel
        _textEdit = new TextEdit
        {
            Editable = true,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            WrapMode = TextEdit.LineWrappingMode.None,
            ScrollFitContentHeight = false,
            ScrollFitContentWidth = false,
            ThemeTypeVariation = "EditorTextEdit"
        };
        _textEdit.SetAnchorsPreset(LayoutPreset.FullRect);

        panel.AddChild(_textEdit);
        _initialized = true;
    }

    public void AddLine(string message)
    {
        // Si no está inicializado, almacenar en buffer
        if (!_initialized || _textEdit == null)
        {
            _pendingLines.Enqueue(message);
            return;
        }

        _lines.Enqueue(message);
        if (_lines.Count > MaxLines)
            _lines.Dequeue();

        _textEdit.Text = string.Join("\n", _lines);
        _textEdit.ScrollVertical = (int)_textEdit.GetLineCount();
    }

    public static void ShowWindow()
    {
        if (_instance != null)
        {
            _instance.Visible = true;
            _instance.MoveToCenter();
            return;
        }

        var sceneTree = Engine.GetMainLoop() as SceneTree;
        var currentScene = sceneTree.CurrentScene;

        if (currentScene == null)
        {
            GD.PrintErr("❌ No hay escena actual para agregar GameLogConsole.");
            return;
        }

        var logWindow = new GameLogConsole
        {
            Position = new Vector2I(100, 100),
            Mode = ModeEnum.Windowed,
        };

        // Añadir de forma deferida
        currentScene.CallDeferred(Node.MethodName.AddChild, logWindow);
        logWindow.CallDeferred(MethodName.Popup);
        logWindow.CallDeferred("show");
        logWindow.CallDeferred(MethodName.MoveToCenter);

        _instance = logWindow;
    }
}
