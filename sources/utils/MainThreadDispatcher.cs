using Godot;
using System;
using System.Collections.Generic;

public partial class MainThreadDispatcher : Node
{
    private static MainThreadDispatcher _instance;
    private readonly Queue<Action> _pendingActions = new();

    public static MainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
                GD.PushError("MainThreadDispatcher not initialized! Make sure it's added to the scene or autoloaded.");
            return _instance;
        }
    }

    public override void _Ready()
    {
        _instance = this;
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        lock (_pendingActions)
        {
            while (_pendingActions.Count > 0)
            {
                var action = _pendingActions.Dequeue();
                action?.Invoke();
            }
        }
    }

    public void Enqueue(Action action)
    {
        lock (_pendingActions)
        {
            _pendingActions.Enqueue(action);
        }
    }
}
