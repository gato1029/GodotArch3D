using Godot;
using GodotEcsArch.sources.utils;
using System;

public  class RampsData :IdDataLong
{
    public long idTileSprite { get; set; }
    public RampsData()
    {
        id = EpochIdGenerator.NewId();
    }
}
