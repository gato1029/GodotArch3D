

using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GodotEcsArch.sources.managers.SpriteMapChunk;


public class SpriteRender
{
    public SpriteRender(Vector2I tilePosition)
    {
        this.tilePosition = tilePosition;
        transform3D = new Transform3D(Basis.Identity, Vector3.Zero);
    }

    public SpriteRender()
    {

    }

    public Entity entityReference { get; set; }
    public int idMaterial { get; set; }
    public int instance { get; set; }
    public bool isAnimate { get; set; }
    public Rid rid { get; set; }
    public Vector2I tilePosition { get; set; }
    public Transform3D transform3D { get; set; }
    public void FreeRidRender()
    {
        rid = default;
        instance = -1;
        if (isAnimate)
        {
            if (entityReference.IsAlive())
            {
                FlecsManager.Instance.DestroyEntitySafe(entityReference);
                //GD.Print("Elimine entidad animada" + tilePosition);
            }

        }

    }

    internal void FreeRidRenderForced()
    {
        rid = default;
        instance = -1;
        if (isAnimate)
        {
            if (entityReference.IsAlive())
            {
                entityReference.Destruct();
            }
        }
    }
    public void UpdateTile(long id, Rid rid, int instance, int textureBatchPosition, Vector3 worldPosition, int layer, SpriteData data)
    {
       
        transform3D = new Transform3D(Basis.Identity, worldPosition);
        transform3D = transform3D.ScaledLocal(new Vector3(data.scale, data.scale, 1));

        idMaterial = data.idMaterial;
        isAnimate = false;
        this.rid = rid;
        this.instance = instance;
        

        RenderingServer.MultimeshInstanceSetTransform(rid, instance, transform3D);
        RenderingServer.MultimeshInstanceSetCustomData(rid, instance, data.GetUv());
        RenderingServer.MultimeshInstanceSetColor(rid, instance,new Color(0,0,0, textureBatchPosition));
          
    }
    public void UpdateTile(int x, int y,long Id, Rid rid, int instance, int textureBatchPosition, Vector3 WorldPosition, int renderLayer, SpriteAnimationData data)
    {
        tilePosition = new Vector2I(x, y);         
        //  Para animaciones
        isAnimate = true;
        transform3D = new Transform3D(Basis.Identity, WorldPosition);
        transform3D = transform3D.ScaledLocal(new Vector3(data.scale, data.scale, 1));

        this.idMaterial = data.idMaterial;
        this.rid = rid;
        this.instance = instance;
        
        Godot.Vector2 originOffset = data.offsetInternal;

        // Todo lo que toca entidades => ejecutar en el hilo principal
        FlecsManager.Instance.RunOnMainThread(() =>
        {
            var world = FlecsManager.Instance.WorldFlecs;

            // ✅ Si la entidad ya existe y está viva, solo actualizar sus componentes
            if (entityReference.IsAlive())
            {
                entityReference.Set(new RenderTransformComponent(transform3D));
                entityReference.Set(new RenderGPUComponent(
                    rid,
                    instance,
                    idMaterial,
                    textureBatchPosition,
                    renderLayer,
                    data.yDepthRender,
                    data.scale,
                    data.offsetInternal));

                entityReference.Set(new AnimationComponent(Id,EntityType.TERRENO,1,
                    -1,
                    1,
                    0,
                    data.frameDuration,
                    false,
                    true,
                    true));

                var uv = data.uvFramesArray[0];
                entityReference.Set(new RenderFrameDataComponent { uvMap = uv });
                entityReference.Set(new PositionComponent
                {
                    position = new Vector2(WorldPosition.X, WorldPosition.Y),
                    tilePosition = tilePosition
                });
            }
            else
            {
                // 🆕 Si no existe, crearla
                var entity = world.Entity();

                entity.Set(new RenderTransformComponent(transform3D));
                entity.Set(new RenderGPUComponent(
                    rid,
                    instance,
                    idMaterial,
                    textureBatchPosition,
                    renderLayer,
                    data.yDepthRender,
                    data.scale,
                    data.offsetInternal));

                entity.Set(new AnimationComponent(
                    Id,
                    EntityType.TERRENO,
                    1,
                    -1,
                    1,
                    0,
                    data.frameDuration,
                    false,
                    true,
                    true));

                var uv = data.uvFramesArray[0];
                entity.Set(new RenderFrameDataComponent { uvMap = uv });
                entity.Set(new PositionComponent
                {
                    position = new Vector2(WorldPosition.X, WorldPosition.Y),
                    tilePosition = tilePosition
                });

                entity.Add<TileSpriteAnimationTag>();

                entityReference = entity;
            }
        });
    }

}
