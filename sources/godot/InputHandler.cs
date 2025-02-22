using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal partial class InputHandler : Node
{
    private Dictionary<string, bool> inputStates = new Dictionary<string, bool>();
    public override void _Input(InputEvent @event)
    {
        foreach (var action in inputStates.Keys)
        {
            if (@event.IsActionPressed(action))
                inputStates[action] = true;
            else if (@event.IsActionReleased(action))
                inputStates[action] = false;
        }
        if (@event.IsActionPressed("move_up"))
        {
            inputStates["move_up"] = true;
        }
        if (@event.IsActionReleased("move_up"))
        {
            inputStates["move_up"] = false;            
        }
    }   
    public override void _Ready()
    { 
        inputStates["move_up"] = false;
        inputStates["move_down"] = false;
        inputStates["move_left"] = false;
        inputStates["move_right"] = false;
        inputStates["attack"] = false;
        inputStates["debugColliders"] = false;
        inputStates["debugGridCollider"] = false;
        ServiceLocator.Instance.RegisterService(this);
    }

    public override void _Process(double delta)
    {
        if (RenderWindowGui.Instance.IsActive)
        {
            inputStates["move_up"] = Input.IsActionPressed("move_up");
            inputStates["move_down"] = Input.IsActionPressed("move_down");
            inputStates["move_left"] = Input.IsActionPressed("move_left");
            inputStates["move_right"] = Input.IsActionPressed("move_right");
            inputStates["attack"] = Input.IsActionPressed("attack");
            inputStates["debugColliders"] = Input.IsActionJustPressed("debugColliders");
            inputStates["debugGridCollider"] = Input.IsActionJustPressed("debugGridCollider");
        }
    }
    
    public bool IsActionActive(string action)
    {
        return inputStates.ContainsKey(action) && inputStates[action];
    }
}

