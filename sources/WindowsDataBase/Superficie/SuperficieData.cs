using Godot;
using GodotEcsArch.sources.utils;
using System;

public partial class SuperficieData:IdDataLong
{
    public long idDualTemplate { get; set; } // de la clase dualtileTemplate
    public SuperficieData()
    {
        id = EpochIdGenerator.NewId();
    }
}
