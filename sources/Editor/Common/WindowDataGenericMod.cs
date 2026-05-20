using Godot;
using System;

public partial class WindowDataGenericMod : Window
{
    public event Action<Object> OnItemSelected;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		KuroItemsData.OnObjectPressed += KuroItemsData_OnObjectPressed;
    }

    private void KuroItemsData_OnObjectPressed(object obj)
    {
        OnItemSelected?.Invoke(obj);
        QueueFree();
    }

    public void LoadData<T>() where T : IdDataLong
	{
		KuroItemsData.ReloadObjects<T>();
	}
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
