using Godot;
using Godot.Collections;
using GodotEcsArch.sources.Helpers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

public partial class KuroSearchItems : PanelContainer
{
    private Vector2 _size;
    private bool _isExpanded = false;
    private double _duration = 0.6;

    [Export] bool _startExpanded = false;
    // Called when the node enters the scene tree for the first time.
    List<Object> _objects = new List<Object>();
    public event Action<Object> OnObjectPressed;
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		ButtonOcultar.Pressed += ButtonOcultar_Pressed;
        ButtonOcultarMostrar.Pressed += ButtonOcultarMostrar_Pressed;       
        if (_startExpanded)
        {
            Expandir();
        }
    }
    public void ReloadObjects<T>() where T : IdDataLong
    {
        ClearObjects();
        var items = DataBaseManager.Instance.FindAll<T>();        
        foreach (var obj in items)
        {
            AddObject(obj,obj.name, obj.textureVisual);
        }
    }
    public void ReloadObjectsByModIDLong<T>(string nameMod) where T:class
    {
        ClearObjects();
        var items = AtlasModsManager.GetAll<T>(nameMod);    
        foreach (dynamic obj in items)
        {
            AddObject(obj, obj.name, obj.textureVisual);
        }
    }
    public void ReloadObjectsByModID<T>(string nameMod) where T : class
    {
        ClearObjects();
        var items = AtlasModsManager.GetAll<T>(nameMod);
        foreach (dynamic obj in items)
        {
            AddObject(obj, obj.name, obj.textureVisual);
        }
    }

    public void RefreshObject(Object obj)
    {
        RemoveObject(obj);
        AddObject(obj, (string)obj.GetType().GetProperty("name").GetValue(obj), (Texture2D)obj.GetType().GetProperty("textureVisual").GetValue(obj));        
    }

    public  async void AddObject(Object obj,string name, Texture2D icon=null)
    {        
        _objects.Add(obj);
        var button = new KuroButton();
        button.SetInternalData(obj);
        if (icon != null)
        {
            button.IconTexture = icon;
        }

        button.ButtonText = name;
        button.TextPosition = KuroButton.TextPlacement.Left;
        button.IconMinSize = new Vector2(32, 32);
        button.IconExpand = false;
        button.Pressed += () =>
        {
            OnObjectPressed?.Invoke(obj);
        };
        Elementos.AddChild(button);
    }

  
    public void RemoveObject(Object obj)
    {
        _objects.Remove(obj);
        foreach (var child in Elementos.GetChildren())
        {
            if (child is KuroButton button && button.GetInternalData() == obj)
            {
                Elementos.RemoveChild(button);
                button.QueueFree();
                break;
            }
        }
    }
    public void ClearObjects()
    {
        _objects.Clear();
        Elementos.ClearChildrens();
    }
    private void ButtonOcultarMostrar_Pressed()
    {  
        if (_isExpanded)
        {
            Contraer();
        }
        else
        {
            Expandir();
        }        
    }
    private void ButtonOcultar_Pressed()
    {
        if (_isExpanded)
        {
            Contraer();
        }
        else
        {
            Expandir();
        }
    }

    private void Expandir()
    {
        _isExpanded = true;

        ContainerIzquierdo.CreateNewTween().AnimatedShow( _duration);
        ContainerSolo.CreateNewTween().AnimatedHide(_duration);
     
    }
    private void Contraer()
    {
        _isExpanded = false;
      

        ContainerIzquierdo.CreateNewTween().AnimatedHide( _duration);
        ContainerSolo.CreateNewTween().AnimatedShow(_duration);
 
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
