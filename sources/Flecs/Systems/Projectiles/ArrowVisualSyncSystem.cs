
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Projectile.DataBase;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Projectiles;

internal class ArrowVisualSyncSystem : FlecsSystemBase
{
    // 🔥 IMPORTANTE: Falso para que corra estrictamente en el hilo principal
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<ProjectileTargetComponent>()
          .With<ProjectilePositionComponent>()
          // .With<RenderFrameDataComponent>()
          .With<ActiveProjectileTag>()
          .Without<ProjectileInitializedTag>(); // Un tag temporal para saber qué flecha acaba de nacer visualmente
    }

    protected override void OnIter(Iter it)
    {
        var projectileArray = it.Field<ProjectileTargetComponent>(0);
        var posArray = it.Field<ProjectilePositionComponent>(1);
        //var gpuArray= it.Field<RenderGPUComponent>(1);
        //var renderFrameArray = it.Field<RenderGPUComponent>(2);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var projType = ref projectileArray[i];
            ref var pos = ref posArray[i];
            //ref var gpu = ref gpuArray[i];
            //ref var frame = ref renderFrameArray[i];
            var entity = it.Entity(i);
     
            AtlasModsManager.TryGetFast<BulletData>(projType.idMod, projType.idProjectile, out var templateProjectile);
            var idTileSprite = templateProjectile.idTileSprite;
            
            int idInternal =AtlasModsManager.GetSpriteUniqueId(idTileSprite, out var spriteTemplate);
            switch (spriteTemplate.tileSpriteType)
            {
                case TileSpriteType.Static:
                    CreateSpriteSingle(entity, idInternal, spriteTemplate.spriteData,pos.Position);
                    break;
                case TileSpriteType.Animated:
                    break;             
                default:
                    break;
            }
            // Marcamos projectil como inicializada visualmente para no repetir esto cada frame
            entity.Add<ProjectileInitializedTag>();
        }
    }

    private void CreateSpriteSingle(Entity entity, int id, SpriteData spriteData, Vector2 position)
    {
        var instance = AtlasTexturesModsManager.Instance.CreateInstanceRender(spriteData.idModMaterial);

        Vector2 offset = spriteData.offsetInternal;

        int height = 10; // altura en el mundo
        int layer = 8;
        
        Godot.Vector2 originOffset = new Vector2(spriteData.offsetInternal.X * spriteData.scale, spriteData.offsetInternal.Y * spriteData.scale);

        float depthOffset = spriteData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, height, layer, position); // debemos usar esto apartir de ahora

        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(position.X, position.Y, z);
        transform = transform.ScaledLocal(new Godot.Vector3(spriteData.scale, spriteData.scale, 1));

        // render
        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new RenderGPUComponent(instance.rid, instance.instance, 0, instance.layerTexture, layer, depthOffset, spriteData.scale, originOffset));
        //entity.Set(new AnimationComponent(id, EntityType.PERSONAJE, AnimationType.PARADO, AnimationType.NINGUNA, 0, 0, 0, false, true, true));
        entity.Set(new RenderFrameDataComponent { uvMap = spriteData.uv });

        RenderingServer.MultimeshInstanceSetTransform(instance.rid, instance.instance, transform); // luego comentar esto
        RenderingServer.MultimeshInstanceSetCustomData(instance.rid, instance.instance, spriteData.uv); // solo esto cambia por la animacion
        RenderingServer.MultimeshInstanceSetColor(instance.rid, instance.instance, new Color(1, 1, 1, instance.layerTexture));
    }
}