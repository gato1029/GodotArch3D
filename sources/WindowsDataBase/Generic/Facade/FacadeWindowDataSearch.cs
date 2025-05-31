using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
internal class FacadeWindowDataSearch<T> where T : class
{
    PackedScene packedSceneNew;
    PackedScene packedSceneWindowData = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/WindowDataSearch.tscn");
    WindowDataSearch windowDataSearch;

    public FacadeWindowDataSearch(string pathWindowNew, Node node)
    {
        packedSceneNew = GD.Load<PackedScene>(pathWindowNew);

        windowDataSearch = packedSceneWindowData.Instantiate<WindowDataSearch>();
        node.AddChild(windowDataSearch);
        windowDataSearch.PopupCentered();
        windowDataSearch.LineEditSearch.TextChanged += LineEditSearch_TextChanged;
        windowDataSearch.ItemListView.GuiInput += ItemListView_GuiInput;
        windowDataSearch.ButtonNew.Pressed += ButtonNew_Pressed;
        LoadAll();
    }

    

    private void LoadAll()
    {
        windowDataSearch.ItemListView.Clear();
        List<T> result = new List<T>();
        result = DataBaseManager.Instance.FindAll<T>();
        foreach (T item in result)
        {
            IdData data = item as IdData;
            int id = windowDataSearch.ItemListView.AddItem(data.name, data.textureVisual);
            windowDataSearch.ItemListView.SetItemMetadata(id, data.id);
        }
    }

    private void ButtonNew_Pressed()
    {
        var control = packedSceneNew.Instantiate<Window>();
        control.PopupExclusiveCentered(windowDataSearch);

        var controlInternal = control as IFacadeWindow<T>;
        controlInternal.OnNotifyChanguedSimple += ControlInternal_OnNotifyChanguedSimple;
    }
    private void LineEditSearch_TextChanged(string newText)
    {
        string text = windowDataSearch.LineEditSearch.Text;
        windowDataSearch.ItemListView.Clear();
        List<T> result = new List<T>();
        if (newText != default)
        {

            result = DataBaseManager.Instance.FindAllByName<T>(text);
        }
        else
        {
            result = DataBaseManager.Instance.FindAll<T>();
        }


        foreach (T item in result)
        {
            IdData data = item as IdData;
            int id = windowDataSearch.ItemListView.AddItem(data.name, data.textureVisual);
            windowDataSearch.ItemListView.SetItemMetadata(id, data.id);
        }
    }

    private void ItemListView_GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            // Verifica doble clic con el botón izquierdo
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.DoubleClick)
            {
                // Obtener el índice del ítem bajo el mouse
                int itemIndex = windowDataSearch.ItemListView.GetItemAtPosition(mouseEvent.Position, true);

                if (itemIndex >= 0)
                {
                    int idInternal = (int)windowDataSearch.ItemListView.GetItemMetadata(itemIndex);
                    var objectData = DataBaseManager.Instance.FindById<T>(idInternal);
                    var control = packedSceneNew.Instantiate<Window>();
                    control.PopupExclusiveCentered(windowDataSearch);

                    var controlInternal = control as IFacadeWindow<T>;

                    controlInternal.SetData(objectData);
                    controlInternal.OnNotifyChanguedSimple += ControlInternal_OnNotifyChanguedSimple;
                }
            }
        }
    }

    private void ControlInternal_OnNotifyChanguedSimple()
    {
        LineEditSearch_TextChanged("");
    }
}
