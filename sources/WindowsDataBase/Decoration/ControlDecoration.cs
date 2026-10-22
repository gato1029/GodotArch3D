using Godot;
using System;

public partial class ControlDecoration : Control
{
    DecorationData data;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI

        // Aseguramos que el TextureRect reciba el click derecho aunque su
        // mouse_filter por defecto sea Ignore.
        TextureRectImage.MouseFilter = MouseFilterEnum.Stop;
        TextureRectImage.GuiInput += TextureRectImage_GuiInput;
        data = new DecorationData();
	}
    public void SetData(DecorationData data)
    {
        if (data==null)
        {
            return;
        }
        this.data = data;
        TextureRectImage.Texture= data.textureVisual;
    }
    public DecorationData GetData()
    {
        return data;
    }
    private void TextureRectImage_GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton
            && mouseButton.ButtonIndex == MouseButton.Right
            && mouseButton.Pressed)
        {
            TextureRectImage.AcceptEvent();
            RemoveDecoration();
        }
    }

    /// <summary>
    /// Elimina este nodo del árbol al final del frame.
    /// Se puede llamar también desde código, no solo desde el click derecho.
    /// </summary>
    public void RemoveDecoration()
    {
        if (IsQueuedForDeletion())
            return;

        QueueFree();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
