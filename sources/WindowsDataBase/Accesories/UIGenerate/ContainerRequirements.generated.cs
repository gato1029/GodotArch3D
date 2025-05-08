// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ContainerRequirements : PanelContainer
{
    private SpinBox SpinBoxLevel;
    private StatsContainer ContainerStats;

    public  void InitializeUI()
    {
        SpinBoxLevel = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBoxLevel");
        ContainerStats = GetNode<StatsContainer>("MarginContainer/VBoxContainer/ContainerStats");
    }
}