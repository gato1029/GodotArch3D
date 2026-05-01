using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
[Tool]
public partial class KuroOptionButton : OptionButton
{
    // Evento de C# puro para máxima velocidad y limpieza
    public event Action<object> OnDataSelected;

    // Almacenamiento interno para evitar dependencias de GodotObject/Variant
    private readonly List<object> _itemsData = new();

    public override void _EnterTree()
    {
        GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        // Suscribimos al evento nativo de Godot para disparar nuestro Action
        ItemSelected += HandleInternalSelection;
    }

    private void HandleInternalSelection(long index)
    {
        if (index >= 0 && index < _itemsData.Count)
        {
            var data = _itemsData[(int)index];
            OnDataSelected?.Invoke(data);
        }
    }

    /// <summary>
    /// Agrega un ítem vinculando cualquier objeto de C#.
    /// </summary>
    public void AddItemWithData(string label, object data, Texture2D icon = null)
    {
        _itemsData.Add(data);
        int index = ItemCount;

        base.AddItem(label);

        if (icon != null)
        {
            SetItemIcon(index, icon);
        }
    }

    /// <summary>
    /// Selecciona programáticamente un ítem basado en la referencia de su objeto.
    /// </summary>
    public void SelectByData(object data, bool triggerEvent = true)
    {
        int index = _itemsData.IndexOf(data);
        if (index != -1)
        {
            Selected = index;
            if (triggerEvent)
            {
                OnDataSelected?.Invoke(data);
            }
        }
    }

    public T GetSelectedData<T>() where T : class
    {
        if (Selected < 0 || Selected >= _itemsData.Count) return null;
        return _itemsData[Selected] as T;
    }

    public new void Clear()
    {
        base.Clear();
        _itemsData.Clear();
    }

    public new void RemoveItem(int idx)
    {
        if (idx >= 0 && idx < _itemsData.Count)
        {
            _itemsData.RemoveAt(idx);
        }
        base.RemoveItem(idx);
    }

    public override void _ExitTree()
    {
        // Limpieza de eventos para evitar fugas de memoria
        ItemSelected -= HandleInternalSelection;
        OnDataSelected = null;
    }
}
