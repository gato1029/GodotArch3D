using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;



namespace GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
internal class FacadeWindowDataSearch<T> where T : class
{
    public delegate void EventNotifySelected(T objectSelected);
    public event EventNotifySelected OnNotifySelected;

    PackedScene packedSceneNew;
    PackedScene packedSceneWindowData = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/WindowDataSearch.tscn");
    public WindowDataSearch windowDataSearch;

    WindowType windowType = WindowType.CREATOR;
    bool gridView = false;
    GroupingData filterGrouping = null;
    public void EnableFilterGrouping(bool enable)
    {
        windowDataSearch.HBoxContainerAgrupador.Visible = enable;
    }
    public FacadeWindowDataSearch(string pathWindowNew, Node node, WindowType pWindowType= WindowType.CREATOR,  bool ForcedTop = false, bool GridView = false)
    {
        windowType = pWindowType;

        packedSceneNew = GD.Load<PackedScene>(pathWindowNew);
        windowDataSearch = packedSceneWindowData.Instantiate<WindowDataSearch>();
        windowDataSearch.AlwaysOnTop = ForcedTop;
        node.AddChild(windowDataSearch);
        windowDataSearch.PopupCentered();
        windowDataSearch.LineEditSearch.TextChanged += LineEditSearch_TextChanged;
        windowDataSearch.ItemListView.GuiInput += ItemListView_GuiInput;        
        windowDataSearch.ButtonNew.Pressed += ButtonNew_Pressed;
        windowDataSearch.ControlGroupingSearch.OnNotifyChangued += ControlGroupingSearch_OnNotifyChangued;
        gridView= GridView;
        if (GridView)
        {
            // Cada ítem tiene un tamaño base de 30 px + un pequeño margen opcional
            float itemWidth = 30.0f;
            float spacing = 0.0f; // margen opcional entre ítems
            float totalItemWidth = itemWidth + spacing;

            // Obtener el ancho actual del control
            float availableWidth = 880f;
            // Calcular columnas máximas posibles
            int maxColumns = Mathf.Max(1, (int)(availableWidth / totalItemWidth));

            windowDataSearch.ItemListView.MaxColumns = maxColumns;
            windowDataSearch.ItemListView.IconMode = ItemList.IconModeEnum.Top;
        }
        LoadAll();
    }

    private void ControlGroupingSearch_OnNotifyChangued(ControlGrouping objectControl)
    {
        filterGrouping = objectControl.GetData();
        LineEditSearch_TextChanged("");
    }

    public FacadeWindowDataSearch(Node node, WindowType pWindowType = WindowType.SELECTED, bool ForcedTop=false)
    {
        windowType = pWindowType;        
        windowDataSearch = packedSceneWindowData.Instantiate<WindowDataSearch>();
        windowDataSearch.AlwaysOnTop = ForcedTop;
        node.AddChild(windowDataSearch);
        windowDataSearch.PopupCentered();
        windowDataSearch.LineEditSearch.TextChanged += LineEditSearch_TextChanged;
        windowDataSearch.ItemListView.GuiInput += ItemListView_GuiInput;
        windowDataSearch.ButtonNew.Pressed += ButtonNew_Pressed;
        LoadAll();
    }
    public void ForcedTop()
    {
        windowDataSearch.AlwaysOnTop = true;
    }
    private void LoadAll()
    {
        windowDataSearch.ItemListView.Clear();
        List<T> result = new List<T>();
        result = DataBaseManager.Instance.FindAll<T>();
        foreach (T item in result)
        {
            ResolveData(item);

        }
    }

    private void ResolveData(T item)
    {
        switch (item)
        {
            case IdData data:
                {
                    if (gridView)
                    {
                        int id = windowDataSearch.ItemListView.AddIconItem(                        
                        data.textureVisual
                    );
                        windowDataSearch.ItemListView.SetItemMetadata(id, data.id);
                        windowDataSearch.ItemListView.SetItemTooltip(id, data.name);
                    }
                    else
                    {
                        int id = windowDataSearch.ItemListView.AddItem(
                        $"{data.id}:{data.name}",
                        data.textureVisual
                    );
                        windowDataSearch.ItemListView.SetItemMetadata(id, data.id);
                        windowDataSearch.ItemListView.SetItemTooltip(id, data.name);
                    }
                    
                    
                    break;
                }

            case IdDataLong datalong:
                {
                    if (gridView)
                    {
                        int id = windowDataSearch.ItemListView.AddIconItem(
                        datalong.textureVisual
                    );
                        windowDataSearch.ItemListView.SetItemMetadata(id, datalong.id);
                        windowDataSearch.ItemListView.SetItemTooltip(id, datalong.name);
                    }
                    else
                    {
                        int id = windowDataSearch.ItemListView.AddItem(
                        $"{datalong.name}:{datalong.id}",
                        datalong.textureVisual
                    );
                        windowDataSearch.ItemListView.SetItemMetadata(id, datalong.id);
                        windowDataSearch.ItemListView.SetItemTooltip(id, datalong.name);
                    }
                    break;
                }
         
            default:
                GD.Print($"Tipo no manejado: {item.GetType().Name}");
                break;
        }
    }

    private T GetResolveData(int index)
    {
        var baseType = typeof(T).BaseType;
        if (baseType != null && baseType == typeof(IdData))
        {
            int idInternal = (int)windowDataSearch.ItemListView.GetItemMetadata(index);
            return DataBaseManager.Instance.FindById<T>(idInternal);
        }
        if (baseType != null && baseType == typeof(IdDataLong))
        {
            long idInternal = (long)windowDataSearch.ItemListView.GetItemMetadata(index);
            return DataBaseManager.Instance.FindById<T>(idInternal);
        }
        GD.Print($"Tipo no manejado: {typeof(T).Name}");
        return null;
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

        if (filterGrouping!=null)
        {
            result = DataBaseManager.Instance.FindAllByGroupName<T>(filterGrouping.id,text);
        }
        else
        {
            if (newText != "")
            {
                result = DataBaseManager.Instance.FindAllByName<T>(text);
            }
            else
            {
                result = DataBaseManager.Instance.FindAll<T>();
            }
        }                      
        foreach (T item in result)
        {
            ResolveData(item);     
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
                    var objectData= GetResolveData(itemIndex);
                                      
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
