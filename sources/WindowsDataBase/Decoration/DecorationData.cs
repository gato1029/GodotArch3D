using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using LiteDB;
using System;

public class DecorationData :IdDataLong
{
    public long idTileSprite { get; set; }

    [BsonCtor]
    public DecorationData(long idTileSprite)
    {
        AtlasModsManager.GetSpriteUniqueId(idTileSprite, out TileSpriteData sprite);
        textureVisual = sprite.textureVisual;
    }

    public DecorationData()
    {
        id = EpochIdGenerator.NewId();
    }
}
