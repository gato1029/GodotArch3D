using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Godot;

//using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Godot.TextServer;

using XNA = FarseerPhysics.Dynamics;

[Component]
public struct ColliderSprite
{

}
[Component]
public struct Collider
{
    public Godot.Rect2 rect;
    public Godot.Rect2 rectTransform; 
    public bool aplyRotation;
    public XNA.Body body;
}
internal class CollisionManager : SingletonBase<CollisionManager>
{
    public  SpatialHashMap<Entity> dynamicCollidersEntities;
    public  SpatialHashMap<Entity> MoveCollidersEntities;
    public FarseerPhysics.Dynamics.World worldPhysic;
    protected override void Initialize()
    {
        dynamicCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128 
        MoveCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128 
        worldPhysic = new FarseerPhysics.Dynamics.World(new Microsoft.Xna.Framework.Vector2(0,0));

   
       
        //worldPhysic.QueryAABB()
        
    }

    protected override void Destroy()
    {
      
    }

    public  bool CheckAABBCollision(Vector2 positionA, Vector2 sizeA, Vector2 directionA, Vector2 positionB, Vector2 sizeB, Vector2 directionB)
    {
        //Inicializo values sin rotacion
        Vector2 topLeftA = positionA - sizeA / 2; ;
        Vector2 topLeftB = positionB - sizeB / 2;

        if (directionA != Vector2.Zero)
        {
            topLeftA = positionA - CalculateSizeBasedOnDirection(sizeA, directionA) / 2;
        }

        if (directionB != Vector2.Zero)
        {
            topLeftB = positionA - CalculateSizeBasedOnDirection(sizeB, directionB) / 2;
        }

        // Verifica si hay intersección
        return (topLeftA.X + sizeA.X >= topLeftB.X &&
                topLeftA.X <= topLeftB.X + sizeB.X &&
                topLeftA.Y + sizeA.Y >= topLeftB.Y &&
                topLeftA.Y <= topLeftB.Y + sizeB.Y);
    }

    public  bool CheckAABBCollision2(Vector2 positionA, Collider colliderA, Vector2 positionB, Collider colliderB)
    {
        //Inicializo values sin rotacion
        Vector2 OriginA = positionA - colliderA.rectTransform.Size / 2 + colliderA.rectTransform.Position;
        Vector2 OriginB = positionB - colliderB.rectTransform.Size / 2 + colliderB.rectTransform.Position;

        //Transform3D transform3D = new Transform3D(Basis.Identity, Vector3.Zero);
        //transform3D = transform3D.Scaled(new Vector3(colliderA.rectTransform.Size.X, colliderA.rectTransform.Size.Y, 1));
        //transform3D.Origin = new Vector3(positionA.X, positionA.Y, 2);

        ////DebugDraw.Circle(transform3D, 4, Colors.Coral, 0.1f,1<<2);
        //DebugDraw.Quad(transform3D, 1, Colors.Cornsilk, 4.0f); //debug   


        //Transform3D transform3D2 = new Transform3D(Basis.Identity, Vector3.Zero);
        //transform3D2 = transform3D2.Scaled(new Vector3(colliderB.rectTransform.Size.X, colliderB.rectTransform.Size.Y, 1));
        //transform3D2.Origin = new Vector3(positionB.X, positionB.Y, 2);
        //DebugDraw.Quad(transform3D2, 1, Colors.Yellow, 5.0f); //debug   

        // Verifica si hay intersección
        return (OriginA.X + colliderA.rectTransform.Size.X >= OriginB.X &&
                OriginA.X <= OriginB.X + colliderB.rectTransform.Size.X &&
                OriginA.Y + colliderA.rectTransform.Size.Y >= OriginB.Y &&
                OriginA.Y <= OriginB.Y + colliderB.rectTransform.Size.Y);
    }

    public  bool CheckAABBCollision(Vector2 positionA, Collider colliderA, Vector2 positionB, Collider colliderB)
    {
        //Inicializo values sin rotacion
        Vector2 topLeftA = positionA - colliderA.rectTransform.Size / 2 + colliderA.rectTransform.Position; 
        Vector2 topLeftB = positionB - colliderB.rectTransform.Size / 2 + colliderB.rectTransform.Position;

        //Transform3D transform3D = new Transform3D(Basis.Identity, Vector3.Zero);
        //transform3D = transform3D.Scaled(new Vector3(colliderA.rectTransform.Size.X, colliderA.rectTransform.Size.Y, 1));
        //transform3D.Origin = new Vector3(topLeftA.X, topLeftA.Y, 2);

        ////DebugDraw.Circle(transform3D, 4, Colors.Coral, 0.1f,1<<2);
        //DebugDraw.Quad(transform3D, 1, Colors.Green, 0.0f); //debug   


        //Transform3D transform3D2 = new Transform3D(Basis.Identity, Vector3.Zero);
        //transform3D2 = transform3D2.Scaled(new Vector3(colliderB.rectTransform.Size.X, colliderB.rectTransform.Size.Y, 1));
        //transform3D2.Origin = new Vector3(topLeftB.X, topLeftB.Y, 2);
        //DebugDraw.Quad(transform3D2, 1, Colors.Blue, 0.0f); //debug   

        // Verifica si hay intersección
        return (topLeftA.X + colliderA.rectTransform.Size.X >= topLeftB.X &&
                topLeftA.X <= topLeftB.X + colliderB.rectTransform.Size.X &&
                topLeftA.Y + colliderA.rectTransform.Size.Y >= topLeftB.Y &&
                topLeftA.Y <= topLeftB.Y + colliderB.rectTransform.Size.Y);
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

