using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Blueprint.DataBase;
using LiteDB;

namespace GodotEcsArch.sources.WindowsDataBase.Chess;

/// <summary>
/// Template puro de variante de cofre en LiteDB.
/// El contenido se decide y asigna al crear/instanciar el cofre según el contexto del juego.
/// </summary>
public class ChestData : IdDataLong
{
    public long IdTileSpriteClosed { get; set; }
    public long IdTileSpriteOpened { get; set; }
    public string Description { get; set; }
    public ItemsRarity RarityTier { get; set; } = ItemsRarity.Vestigio;
    public bool DespawnAfterOpen { get; set; } = true;

    [BsonIgnore]
    public Godot.Texture2D OpenedTextureVisual { get; private set; }

    public ChestData()
    {
        id = EpochIdGenerator.NewId();
    }

    [BsonCtor]
    public ChestData(long idTileSpriteClosed, long idTileSpriteOpened) : this()
    {
        IdTileSpriteClosed = idTileSpriteClosed;
        IdTileSpriteOpened = idTileSpriteOpened;
        RefreshTextureVisual();
    }

    public override void RefreshTextureVisual()
    {
        if (IdTileSpriteClosed > 0)
        {
            textureVisual = MasterDataManager.GetData<TileSpriteData>(IdTileSpriteClosed).textureVisual;
        }
        if (IdTileSpriteOpened > 0)
        {
            OpenedTextureVisual = MasterDataManager.GetData<TileSpriteData>(IdTileSpriteOpened).textureVisual;
        }
    }
}