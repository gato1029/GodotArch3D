// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlTileDual : PanelContainer
{
    public delegate void EventNotifyChangued(ControlTileDual objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonRemove;
    private ControlListTileSprite ControlTileSprite;

    public void InitializeUI()
    {
        ButtonRemove = GetNode<Button>("Panel/ButtonRemove");
        ControlTileSprite = GetNode<ControlListTileSprite>("ControlTileSprite");
    }
}