using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;



namespace GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
internal class FacadeWindowDataSearch<T> where T : class
{
    public delegate void EventNotifySelected(T objectSelected);
    public event EventNotifySelected OnNotifySelected;

    PackedScene packedSceneNew;
    PackedScene packedSceneWindowData = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/WindowDataSearch.tscn");
    WindowDataSearch windowDataSearch;

    WindowType windowType = WindowType.CREATOR;
    
    public FacadeWindowDataSearch(string pathWindowNew, Node node, WindowType pWindowType= WindowType.CREATOR)
    {
        windowType = pWindowType;

        packedSceneNew = GD.Load<PackedScene>(pathWindowNew);
        windowDataSearch = packedSceneWindowData.Instantiate<WindowDataSearch>();
        node.AddChild(windowDataSearch);
        windowDataSearch.PopupCentered();
        windowDataSearch.LineEditSearch.TextChanged += LineEditSearch_TextChanged;
        windowDataSearch.ItemListView.GuiInput += ItemListView_GuiInput;
        windowDataSearch.ButtonNew.Pressed += ButtonNew_Pressed;
        LoadAll();
    }

    public FacadeWindowDataSearch(Node node, WindowType pWindowType = WindowType.SELECTED)
    {
        windowType = pWindowType;        
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
            int id = windowDataSearch.ItemListView.AddItem(data.id +":"+ data.name, data.textureVisual);
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
            int id = windowDataSearch.ItemListView.AddItem(data.id +":"+data.name, data.textureVisual);
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
                    
                    switch (windowType)
                    {
                        case WindowType.SELECTED:
                            OnNotifySelected?.Invoke(objectData);
                            windowDataSearch.QueueFree();
                            break;
                        case WindowType.CREATOR:                            
                            var control = packedSceneNew.Instantiate<Window>();
                            control.PopupExclusiveCentered(windowDataSearch);

                            var controlInternal = control as IFacadeWindow<T>;
                            controlInternal.SetData(objectData);
                            controlInternal.OnNotifyChanguedSimple += ControlInternal_OnNotifyChanguedSimple;
                            break;
                        default:
                            break;
                    }

                 
                }
            }
        }
    }

    private void ControlInternal_OnNotifyChanguedSimple()
    {
        LineEditSearch_TextChanged("");
    }
}
