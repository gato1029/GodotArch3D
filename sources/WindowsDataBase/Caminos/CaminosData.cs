using Godot;
using GodotEcsArch.sources.utils;
using System;

public class CaminosData :IdDataLong
{
    public long idDualTemplate { get; set; } // de la clase dualtileTemplate
    public CaminosData()
    {
        id = EpochIdGenerator.NewId();
    }
}
