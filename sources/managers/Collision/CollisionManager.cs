using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Auios.QuadTree;

using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Tilemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


[Component]
public struct ColliderShape
{
    public Rect2 aabb;
    public GeometricShape2D shape;    
}
[Component]
public struct ColliderSprite
{  
    public GeometricShape2D shapeBody;
    public GeometricShape2D shapeMove;   
}
internal class CollisionManager : SingletonBase<CollisionManager>
{
    public  SpatialHashMap<Entity> dynamicCollidersEntities;
    public  SpatialHashMap<Entity> MoveCollidersEntities;
    public SpatialHashMap<IDataTile> tileColliders;

    public QuadTree<ColliderSprite> quadTreeColliders;

    public SpatialHashMap<Entity> characterCollidersEntities;
    protected override void Initialize()
    {
        characterCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128 

        dynamicCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128 
        MoveCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128
        tileColliders = new SpatialHashMap<IDataTile>(8, delegate (IDataTile er) { return er.IdCollider; });

    }

    protected override void Destroy()
    {
      
    }

    ColliderShape CreateColliderShape(GeometricShape2D shape2D)
    {
        ColliderShape colliderShape = new ColliderShape();
        colliderShape.shape = shape2D;

     
        return colliderShape;
    }

    public  bool CheckAABBCollision(Vector2 positionA, Rect2 rectA,  Vector2 positionB, Rect2 rectB)
    {
        
        Rect2 adjustedRectA = new Rect2(positionA - rectA.Position, rectA.Size);
        Rect2 adjustedRectB = new Rect2(positionB - rectB.Position, rectB.Size);

        Transform3D transform3D = new Transform3D(Basis.Identity, Vector3.Zero);
        transform3D = transform3D.Scaled(new Vector3(adjustedRectA.Size.X, adjustedRectA.Size.Y, 1));
        transform3D.Origin = new Vector3(adjustedRectA.Position.X, adjustedRectA.Position.Y, 2);
        
        //DebugDraw.Circle(transform3D, 4, Colors.Coral, 0.1f,1<<2);
        DebugDraw.Quad(transform3D, 1, Colors.Green, 0.0f); //debug   
        

        Transform3D transform3D2 = new Transform3D(Basis.Identity, Vector3.Zero);
        transform3D2 = transform3D2.Scaled(new Vector3(adjustedRectB.Size.X, adjustedRectB.Size.Y, 1));
        transform3D2.Origin = new Vector3(adjustedRectB.Position.X, adjustedRectB.Position.Y, 2);
        DebugDraw.Quad(transform3D2, 1, Colors.Blue, 0.0f); //debug   
        //DebugDraw.Circle(transform3D2, 2, Colors.Coral, 1);

        return (adjustedRectA.Position.X + adjustedRectA.Size.X >= adjustedRectB.Position.X &&  
                adjustedRectA.Position.X <= adjustedRectB.Position.X + adjustedRectB.Size.X &&  
                adjustedRectA.Position.Y + adjustedRectA.Size.Y >= adjustedRectB.Position.Y  &&  
                adjustedRectA.Position.Y <= adjustedRectB.Position.Y + adjustedRectB.Size.Y);   

    }

    public  Vector2 CalculateSizeBasedOnDirection(Vector2 originalSize, Vector2 direction)
    {
        ////Vector2 normalizedDirection = direction.Normalized();

        float width = originalSize.X;
        float height = originalSize.Y;

        // Se establece un umbral para determinar si la direccion es principalmente horizontal o vertical
        float threshold = 0.707f; // Aproximadamente 45 grados en términos de vector

        // Determinar el ancho y el alto basado en la direccion
        if (Math.Abs(direction.X) > threshold) // Direcciones horizontales
        {
            width = originalSize.X;  // Mantiene el ancho original
            height = 0;               // Altura se convierte en cero
        }
        else if (Math.Abs(direction.Y) > threshold) // Direcciones verticales
        {
            width = 0;                // Ancho se convierte en cero
            height = originalSize.Y;  // Mantiene la altura original
        }
        else // Direcciones diagonales
        {
            // Puedes elegir mantener ambas dimensiones o una combinación
            width = originalSize.X;   // Mantiene el ancho original
            height = originalSize.Y;  // Mantiene la altura original
        }

        return new Vector2(width, height);
    }


}

