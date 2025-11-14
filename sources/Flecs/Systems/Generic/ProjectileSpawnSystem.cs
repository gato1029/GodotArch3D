using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Projectile;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;

namespace GodotFlecs.sources.Flecs.Systems.Generic
{
    internal class ProjectileSpawnSystem : FlecsSystemBase
    {
        protected override ulong Phase => flecs.EcsPostUpdate; // se ejecuta después del update normal
        protected override bool MultiThreaded => false;

        protected override void BuildQuery(ref QueryBuilder qb)
        {
            qb.With<PositionComponent>()
              .With<AttackPendingComponent>()
              .With<RangedAttackComponent>()
              .With<TeamComponent>()
              .With<DirectionComponent>()
              .Without<DeadTag>()
              .Without<DestroyRequestTag>();
        }

        protected override void OnIter(Iter it)
        {
            var posArray = it.Field<PositionComponent>(0);
            var atpArray = it.Field<AttackPendingComponent>(1);
            var rangedArray = it.Field<RangedAttackComponent>(2);
            var teamArray = it.Field<TeamComponent>(3);
            var dirArray = it.Field<DirectionComponent>(4);

            for (int i = 0; i < it.Count(); i++)
            {
                ref var pos = ref posArray[i];
                ref var atp = ref atpArray[i];
                ref var ranged = ref rangedArray[i];
                ref var team = ref teamArray[i];
                ref var dir = ref dirArray[i];

                // Solo procesar si hay ataque pendiente y un objetivo válido
                if (!atp.Active || !atp.Target.IsAlive())
                    continue;

                var targetPos = atp.Target.Get<PositionComponent>().position;
                var direction = (targetPos - pos.position).Normalized();
       
                // Crear el proyectil como nueva entidad Flecs
                var projectile = FlecsManager.Instance.WorldFlecs.Entity();
                projectile.Set(new PositionComponent { position = pos.position });
                    projectile.Set(new ProjectileComponent
                    {
                        Owner = it.Entity(i),
                        Target = atp.Target,
                        Speed = ranged.SpeedProjectile,               // puedes ajustarlo según ranged
                        Direction = direction,
                        Damage = ranged.Damage,
                        idProjectile = ranged.idProjectile,
                        Homing = ranged.Homing,
                        LifeTime = 5.0f,            // tiempo de vida en segundos
                        TargetPosition = targetPos
                    });
                    projectile.Set(new TeamComponent { TeamId = team.TeamId });

                var spriteData = ProjectileManager.Instance.GetData(ranged.idProjectile).spriteData;
                var instance = MultimeshManager.Instance.CreateInstance(spriteData.idMaterial);

                Transform3D transform = new Transform3D(Basis.Identity, Vector3.Zero);
                transform.Origin = new Vector3(pos.position.X, pos.position.Y, pos.position.Y * CommonAtributes.LAYER_MULTIPLICATOR + 0);
                transform = transform.ScaledLocal(new Vector3(spriteData.scale, spriteData.scale, 1));
                projectile.Set(new RenderGPUComponent(instance.rid, instance.instance,  instance.material,instance.layerTexture, 20 , 0, 1, spriteData.offsetInternal));
                projectile.Set(new RenderTransformComponent { transform = transform });
                projectile.Set(new RenderFrameDataComponent { uvMap = new Color(spriteData.xFormat, spriteData.yFormat, spriteData.widhtFormat, spriteData.heightFormat) });

                // Resetear el ataque pendiente
                atp.Active = false;
                atp.Target = default;
            }
        }
    }
}
