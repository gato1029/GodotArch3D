using Godot;
using GodotFlecs.sources.Flecs.Components;
using LiteDB;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Collision;

public class Slope : GeometricShape2D
{
    public SlopeType slopeType { get; set; }
    public float Width { get; private set; }
    public float Height { get; private set; }
    public Slope()
    {

    }
    [BsonCtor]
    public Slope(SlopeType slopeType,float widthPixel,float heightPixel, float originPixelX, float originPixelY) : base()
    {
        this.originPixelX = originPixelX;
        this.originPixelY = originPixelY;
        this.widthPixel = widthPixel;
        this.heightPixel = heightPixel;
        this.slopeType = slopeType;     
        OriginRelative = new Godot.Vector2(MeshCreator.PixelsToUnits(originPixelX), MeshCreator.PixelsToUnits(originPixelY));
        OriginCurrent = new Godot.Vector2(MeshCreator.PixelsToUnits(originPixelX), MeshCreator.PixelsToUnits(originPixelY));
        Width = MeshCreator.PixelsToUnits(widthPixel);
        Height = MeshCreator.PixelsToUnits(heightPixel);
    }

    public override Vector2 GetSizeQuad()
    {
        throw new NotImplementedException();
    }

    public override Vector2[] GetVertices(Vector2 position)
    {
        throw new NotImplementedException();
    }

    public override GeometricShape2D Multiplicity(float value)
    {
  

        // Escalamos también el origen relativo
        var newOriginRelative = OriginRelative * value;

        var n = new Slope()
        {
            OriginRelative = newOriginRelative,
            OriginCurrent = newOriginRelative, // mantenemos coherencia
            scale = this.scale * value,        // acumulamos escala
            widthPixel = this.widthPixel * value,
            heightPixel = this.heightPixel * value,
            originPixelX = this.originPixelX * value,
            originPixelY = this.originPixelY * value,
            Width = this.Width * value,
            Height = this.Height * value,
            slopeType = this.slopeType,
        };

        return n;
    }

    public override GeometricShape2D MultiplicityInternal(float value)
    {
        throw new NotImplementedException();
    }
}
