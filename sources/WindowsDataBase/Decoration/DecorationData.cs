using Godot;
using GodotEcsArch.sources.utils;
using System;

public class DecorationData :IdDataLong
{
    public long idTileSprite { get; set; }

    public DecorationData()
    {
        id = EpochIdGenerator.NewId();
    }
}
