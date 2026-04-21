using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;

public class BlackyTilePalette
{
    //1
    public Dictionary<ushort, ushort> paletteReal;

    public Dictionary<ushort, string> paletteMaterial;

    public  Color GetUvFormatFromSprite(SpriteData spriteData)
    {
        var material = MaterialManager.Instance.GetMaterial(spriteData.idMaterial);
        var offset = new Vector2(material.originXTextureMaster, material.originYTextureMaster);

        // Calcular nueva posición y tamaño
        float newX = offset.X + spriteData.x;
        float newY = offset.Y + spriteData.y;

        Color uvColor = new Color();
        uvColor.R = newX;
        uvColor.G = newY;
        uvColor.B = spriteData.mirrorX ? -spriteData.widht : spriteData.widht;
        uvColor.A = spriteData.mirrorY ? -spriteData.height : spriteData.height;
        return uvColor;
    }
}
