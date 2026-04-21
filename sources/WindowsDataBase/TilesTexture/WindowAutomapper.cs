using Godot;
using GodotEcsArch.sources.CustomWidgets.Internals;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Generic;


[KuroRegisterWindow("res://sources/WindowsDataBase/TilesTexture/WindowAutomapper.tscn")]
public partial class WindowAutomapper : Window, IFacadeWindow<AutomapperData>
{
    public event IFacadeWindow<AutomapperData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    private AutomapperData data;

    public override void _Ready()
    {
        InitializeUI();
        data = new AutomapperData();
        data.ReGerenateId();

        ButtonAdd.Pressed += ButtonAdd_Pressed;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
        ButtonSave.Pressed += ButtonSave_Pressed;
        ItemsContainer.ItemSelected += ItemsContainer_ItemSelected;
    }

    private void ItemsContainer_ItemSelected(long index)
    {
        ItemsContainer.DeselectAll();
        // Usamos el índice directamente para obtener el elemento correcto
        int selectedIndex = (int)index;
        if (selectedIndex < 0 || selectedIndex >= data.Phases.Count) return;

        var element = data.Phases[selectedIndex];

        var scene = GD.Load<PackedScene>("res://sources/WindowsDataBase/TilesTexture/WindowGroupTileTexture.tscn");
        var widget = scene.Instantiate<WindowGroupTileTexture>();
        AddChild(widget);

        widget.SetParentData(data, selectedIndex);
        widget.SetData(element, element.materialId);
        

        
        widget.Popup();
    }

    private void ButtonAdd_Pressed()
    {
        // Crear nueva fase
        var newPhase = new AutoTilePhase();
        // Asignar un nombre por defecto basado en la posición
        newPhase.name = $"Phase {data.Phases.Count}";

        data.Phases.Add(newPhase);

        // Refrescamos para asignar órdenes correctos
        RefreshUI();

        // Abrir el editor para el nuevo elemento (el último)
        int lastIndex = data.Phases.Count - 1;
        var scene = GD.Load<PackedScene>("res://sources/WindowsDataBase/TilesTexture/WindowGroupTileTexture.tscn");
        var widget = scene.Instantiate<WindowGroupTileTexture>();

        AddChild(widget);
        widget.Popup();
        widget.SetParentData(data, lastIndex);
    }

    private void ButtonDelete_Pressed()
    {
        var selected = ItemsContainer.GetSelectedItems();
        if (selected.Length == 0) return;

        int indexToRemove = selected[0];

        if (indexToRemove >= 0 && indexToRemove < data.Phases.Count)
        {
            // Al remover de la lista de C#, los elementos posteriores se "recorren" solos
            data.Phases.RemoveAt(indexToRemove);

            // Re-sincronizamos órdenes y actualizamos la UI
            RefreshUI();
        }
    }

    public void ButtonSave_Pressed()
    {
        data.name = LineEditName.Text;
        // Aseguramos que el orden esté actualizado antes de guardar
        UpdateInternalOrder();
        DataBaseManager.Instance.InsertUpdate<AutomapperData>(data);
        OnNotifyChanguedSimple?.Invoke();        
        Message.ShowMessage(this, "Guardado Exitoso :)!");
        QueueFree();
    }

    /// <summary>
    /// Sincroniza la propiedad 'order' de cada objeto con su posición real en la lista
    /// </summary>
    private void UpdateInternalOrder()
    {
        for (int i = 0; i < data.Phases.Count; i++)
        {
            // Asume que AutoTilePhase tiene una propiedad Order o similar
             data.Phases[i].order = i; 
        }
    }

    /// <summary>
    /// Limpia y reconstruye la lista visual basada en los datos actuales
    /// </summary>
    public void RefreshUI()
    {
        ItemsContainer.Clear();
        for (int i = 0; i < data.Phases.Count; i++)
        {
            var item = data.Phases[i];
            // Si el nombre está vacío, le ponemos uno temporal
            string displayName = string.IsNullOrEmpty(item.name) ? $"Phase {i}" : item.name;
            ItemsContainer.AddItem(displayName);
        }
    }

    void IFacadeWindow<AutomapperData>.SetData(AutomapperData data)
    {
        this.data = data;
        LineEditName.Text = data.name;
        RefreshUI();
    }
}
