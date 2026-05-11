using Godot;
using Godot.Collections;
using GodotEcsArch.sources.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

public partial class KuroSearchItems : PanelContainer
{
    private Vector2 _size;
    private bool _isExpanded = false;
    private double _duration = 0.6;
    // Called when the node enters the scene tree for the first time.
    List<Object> _objects = new List<Object>();
    public event Action<Object> OnObjectPressed;
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		ButtonOcultar.Pressed += ButtonOcultar_Pressed;
        ButtonOcultarMostrar.Pressed += ButtonOcultarMostrar_Pressed;
       // AddObject("prueba2", "gatito");
    }

    public  async void AddObject(Object obj,string name, Texture2D icon=null)
    {
        //if (!IsNodeReady())
        //{
        //    CallDeferred(nameof(AddObject), (Variant)obj, name, icon);
        //    return;
        //}
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
