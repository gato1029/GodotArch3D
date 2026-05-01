using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
[Tool]
public partial class KuroItemList : ItemList
{
    // Eventos para selección simple y múltiple
    public event Action<object> OnDataSelected;
    public event Action<IEnumerable<object>> OnMultipleDataSelected;

    private readonly List<object> _itemsData = new();

    public override void _EnterTree()
    {
        ItemSelected += HandleItemSelected;
        MultiSelected += HandleMultiSelected;
    }

    private void HandleItemSelected(long index)
    {
        var data = GetDataAt<object>((int)index);
        OnDataSelected?.Invoke(data);
    }

    private void HandleMultiSelected(long index, bool selected)
    {
        var selectedIndices = GetSelectedItems();
        var selectedData = selectedIndices.Select(idx => _itemsData[idx]);
        OnMultipleDataSelected?.Invoke(selectedData);
    }

    /// <summary>
    /// Agrega un ítem con su objeto de datos y un icono opcional.
    /// </summary>
    public void AddItemWithData(string label, object data, Texture2D icon = null, bool selectable = true)
    {
        _itemsData.Add(data);
        int index = ItemCount;

        base.AddItem(label);

        if (icon != null) SetItemIcon(index, icon);
        SetItemSelectable(index, selectable);
    }

    /// <summary>
    /// Retorna todos los objetos seleccionados (útil si SelectMode es Multi).
    /// </summary>
    public IEnumerable<T> GetSelectedDatas<T>() where T : class
    {
        return GetSelectedItems().Select(idx => _itemsData[idx] as T);
    }

    /// <summary>
    /// Retorna el primer objeto seleccionado.
    /// </summary>
    public T GetFirstSelectedData<T>() where T : class
    {
        var selected = GetSelectedItems();
        if (selected.Length == 0) return null;
        return _itemsData[selected[0]] as T;
    }

    public T GetDataAt<T>(int index) where T : class
    {
        if (index < 0 || index >= _itemsData.Count) return null;
        return _itemsData[index] as T;
    }

    /// <summary>
    /// Selecciona un ítem basado en su referencia de objeto.
    /// </summary>
    public void SelectByData(object data, bool addToExisting = false)
    {
        int index = _itemsData.IndexOf(data);
        if (index != -1)
        {
            Select(index, !addToExisting);
            if (!addToExisting) HandleItemSelected(index);
        }
    }

    public new void Clear()
    {
        base.Clear();
        _itemsData.Clear();
    }

    public new void RemoveItem(int idx)
    {
        if (idx >= 0 && idx < _itemsData.Count) _itemsData.RemoveAt(idx);
        base.RemoveItem(idx);
    }

    public override void _ExitTree()
    {
        ItemSelected -= HandleItemSelected;
        MultiSelected -= HandleMultiSelected;
        OnDataSelected = null;
        OnMultipleDataSelected = null;
    }
}
