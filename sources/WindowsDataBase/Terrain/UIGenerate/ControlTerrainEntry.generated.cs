// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlTerrainEntry : MarginContainer
{
    public delegate void EventNotifyChangued(ControlTerrainEntry objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private SpinBox SpinBoxHeight;
    private ControlKuroAutoTile ControlBase;
    private ControlKuroAutoTile ControlDecorBase;
    private ControlKuroAutoTile ControlSuperficie;
    private ControlKuroAutoTile ControlDecorSuperficie;
    private KuroTextureButton ButtonRemove;

    public void InitializeUI()
    {
        SpinBoxHeight = GetNode<SpinBox>("GridContainer/SpinBoxHeight");
        ControlBase = GetNode<ControlKuroAutoTile>("GridContainer/ControlBase");
        ControlDecorBase = GetNode<ControlKuroAutoTile>("GridContainer/ControlDecorBase");
        ControlSuperficie = GetNode<ControlKuroAutoTile>("GridContainer/ControlSuperficie");
        ControlDecorSuperficie = GetNode<ControlKuroAutoTile>("GridContainer/ControlDecorSuperficie");
        ButtonRemove = GetNode<KuroTextureButton>("ButtonRemove");
    }
}