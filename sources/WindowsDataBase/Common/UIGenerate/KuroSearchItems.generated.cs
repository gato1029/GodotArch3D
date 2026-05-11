// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class KuroSearchItems : PanelContainer
{
    public delegate void EventNotifyChangued(KuroSearchItems objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private AnimatedPanelContainer ContainerSolo;
    private KuroButton ButtonOcultarMostrar;
    private AnimatedPanelContainer ContainerIzquierdo;
    private KuroButton ButtonOcultar;
    private Label LabelName;
    private ScrollContainer ScrollContainerPadre;
    private VBoxContainer Elementos;

    public void InitializeUI()
    {
        ContainerSolo = GetNode<AnimatedPanelContainer>("ContainerSolo");
        ButtonOcultarMostrar = GetNode<KuroButton>("ContainerSolo/ButtonOcultarMostrar");
        ContainerIzquierdo = GetNode<AnimatedPanelContainer>("ContainerIzquierdo");
        ButtonOcultar = GetNode<KuroButton>("ContainerIzquierdo/MarginContainer/VBoxContainer/HBoxContainer/ButtonOcultar");
        LabelName = GetNode<Label>("ContainerIzquierdo/MarginContainer/VBoxContainer/HBoxContainer/LabelName");
        ScrollContainerPadre = GetNode<ScrollContainer>("ContainerIzquierdo/MarginContainer/VBoxContainer/ScrollContainerPadre");
        Elementos = GetNode<VBoxContainer>("ContainerIzquierdo/MarginContainer/VBoxContainer/ScrollContainerPadre/Elementos");
    }
}