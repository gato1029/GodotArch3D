// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlKuroRuleItem : MarginContainer
{
    public delegate void EventNotifyChangued(ControlKuroRuleItem objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private GridContainer GridContainerItems;
    private CustomButtonRule CBArribaIzquierda;
    private CustomButtonRule CBArriba;
    private CustomButtonRule CBArribaDerecha;
    private CustomButtonRule CBIzquierda;
    private CustomButtonRule CBCenter;
    private CustomButtonRule CBDerecha;
    private CustomButtonRule CBAbajoIzquierda;
    private CustomButtonRule CBAbajo;
    private CustomButtonRule CBAbajoDerecha;
    private CustomButtonRule CBCentralDebajo;
    private CheckButton CheckButtonRandom;

    public void InitializeUI()
    {
        GridContainerItems = GetNode<GridContainer>("VBoxContainer/GridContainerItems");
        CBArribaIzquierda = GetNode<CustomButtonRule>("VBoxContainer/GridContainerItems/CBArribaIzquierda");
        CBArriba = GetNode<CustomButtonRule>("VBoxContainer/GridContainerItems/CBArriba");
        CBArribaDerecha = GetNode<CustomButtonRule>("VBoxContainer/GridContainerItems/CBArribaDerecha");
        CBIzquierda = GetNode<CustomButtonRule>("VBoxContainer/GridContainerItems/CBIzquierda");
        CBCenter = GetNode<CustomButtonRule>("VBoxContainer/GridContainerItems/CBCenter");
        CBDerecha = GetNode<CustomButtonRule>("VBoxContainer/GridContainerItems/CBDerecha");
        CBAbajoIzquierda = GetNode<CustomButtonRule>("VBoxContainer/GridContainerItems/CBAbajoIzquierda");
        CBAbajo = GetNode<CustomButtonRule>("VBoxContainer/GridContainerItems/CBAbajo");
        CBAbajoDerecha = GetNode<CustomButtonRule>("VBoxContainer/GridContainerItems/CBAbajoDerecha");
        CBCentralDebajo = GetNode<CustomButtonRule>("VBoxContainer/CBCentralDebajo");
        CheckButtonRandom = GetNode<CheckButton>("VBoxContainer/CheckButtonRandom");
    }
}