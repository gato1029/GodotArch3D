using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
[Tool]
public partial class UiInterfaceGenerator : Control
{
    public delegate void OptionToggledEventHandler(int id, string label, bool state);
    public event OptionToggledEventHandler OnOptionToggled;

    private MenuButton _menuButton;
    private PopupMenu _popup;

    // Guarda el estado checkeado de los ítems
    public Dictionary<int, bool> ItemStates;

    private UIGenerator uIGenerator;
    private bool _isInitialized = false;
    public override void _EnterTree()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;
        
        InitializeUI(); // Insertado por el generador de UI
        ItemStates =new Dictionary<int, bool>();
        ButtonGenerate.Pressed += ButtonGenerate_Pressed;
        uIGenerator = new UIGenerator();        
        AddOptions();
    }
    public override void _ExitTree()
    {
        if (!_isInitialized)
            return;

        _isInitialized = false;

        //if (IsInstanceValid(_popup))
        //{
        //    try { _popup.IdPressed -= OnIdPressed; } catch { }
        //}

        //if (IsInstanceValid(ButtonGenerate))
        //{
        //    try { ButtonGenerate.Pressed -= ButtonGenerate_Pressed; } catch { }
        //}
        OnOptionToggled -= UiInterfaceGenerator_OnOptionToggled;
        ClearOptions();
        

    }
    public void ClearOptions()
    {
        if (_popup != null)
            _popup.Clear();

        if (ItemStates != null)
            ItemStates.Clear();
    }
    private void AddOptions()
    {
        _menuButton = MenuButtonOptions;
        _popup = _menuButton.GetPopup();
        _popup.IdPressed += OnIdPressed;
        AddItem("Public", 0);

        OnOptionToggled += UiInterfaceGenerator_OnOptionToggled;
    }

    private void UiInterfaceGenerator_OnOptionToggled(int id, string label, bool state)
    {
        switch (id)
        {
            case 0:
                if (state)
                {
                    uIGenerator.UiGeneratorMode = UIGeneratorMode.Public;
                    ButtonGenerate.Text = "Public";
                }
                else
                {
                    uIGenerator.UiGeneratorMode = UIGeneratorMode.Private;
                    ButtonGenerate.Text = "Private";
                }
                
                break;
            default:
                break;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
    
	}

    private void ButtonGenerate_Pressed()
    {
     
        uIGenerator._Run();
    }


    public void AddItem(string label, int id, bool isChecked = false)
    {
        _popup.AddCheckItem(label, id);
        _popup.SetItemChecked(id, isChecked);
        ItemStates[id] = isChecked;
    }

    public bool IsChecked(int id)
    {
        return ItemStates.TryGetValue(id, out var value) && value;
    }

    public void SetChecked(int id, bool value)
    {
        _popup.SetItemChecked(id, value);
        ItemStates[id] = value;
    }

    private void OnIdPressed(long idRaw)
    {
        int id = (int)idRaw;

        // Alternar el estado
        bool newState = !_popup.IsItemChecked(id);
        _popup.SetItemChecked(id, newState);
        ItemStates[id] = newState;
        
        string label = _popup.GetItemText(id);
        OnOptionToggled?.Invoke(id, label, newState);
        // <-- Aquí es donde evitamos que se cierre, para esta version 
        //Callable.From(() =>
        //{
        //    _popup.Popup();
        //}).CallDeferred();
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
