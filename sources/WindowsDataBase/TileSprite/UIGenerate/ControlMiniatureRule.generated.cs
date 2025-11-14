// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlMiniatureRule : PanelContainer
{
    public delegate void EventNotifyChangued(ControlMiniatureRule objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private VBoxContainer vbox;
    private TextureRect TextureRectCenter;
    private PanelContainer PanelBackgroud;
    private ControlKuroRuleItem ControlKuroTileWid;
    private KuroTextureButton ButtonAddBehind;
    private KuroTextureButton ButtonEdit;
    private KuroTextureButton ButtonDelete;
    private KuroTextureButton ButtonDuplicate;
    private KuroTextureButton ButtonAddAfter;

    public void InitializeUI()
    {
        vbox = GetNode<VBoxContainer>("vbox");
        TextureRectCenter = GetNode<TextureRect>("vbox/HBoxContainer/TextureRectCenter");
        PanelBackgroud = GetNode<PanelContainer>("vbox/HBoxContainer/PanelBackgroud");
        ControlKuroTileWid = GetNode<ControlKuroRuleItem>("vbox/HBoxContainer/PanelBackgroud/ControlKuroTileWid");
        ButtonAddBehind = GetNode<KuroTextureButton>("vbox/HBoxContainer/VBoxContainer/ButtonAddBehind");
        ButtonEdit = GetNode<KuroTextureButton>("vbox/HBoxContainer/VBoxContainer/VBoxContainer/ButtonEdit");
        ButtonDelete = GetNode<KuroTextureButton>("vbox/HBoxContainer/VBoxContainer/VBoxContainer/ButtonDelete");
        ButtonDuplicate = GetNode<KuroTextureButton>("vbox/HBoxContainer/VBoxContainer/VBoxContainer/ButtonDuplicate");
        ButtonAddAfter = GetNode<KuroTextureButton>("vbox/HBoxContainer/VBoxContainer/ButtonAddAfter");
    }
}