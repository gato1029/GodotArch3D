using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Generic;
internal class AnimationTileSpriteSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<AnimationComponent>()           
          .With<RenderFrameDataComponent>()
          .With<TileSpriteAnimationTag>();

    }

    protected override void OnIter(Iter it)
    {        
        var animation = it.Field<AnimationComponent>(0);   
        var frame = it.Field<RenderFrameDataComponent>(1);
        float delta = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var a = ref animation[i];       
            ref var f = ref frame[i];

            var dataAnim = MasterDataManager.GetData<TileSpriteData>(a.idSpriteOrAnimation).animationData;
            a.TimeSinceLastFrame += delta;
            if (a.TimeSinceLastFrame >= a.frameDuration && a.active)
            {
                a.TimeSinceLastFrame = 0;
                if (a.currentFrameIndex >= dataAnim.uvFramesArray.Length)
                {
                    if (dataAnim.loop)
                    {
                        f.uvMap = dataAnim.uvFramesArray[0]; // Reinicia al frame inicial                        
                        a.animationComplete = true;
                        a.currentFrameIndex = 0;
                        a.active = true;
                    }
                    else
                    {
                        a.animationComplete = true;
                        a.active = false;
                    }
                }
                else
                {
                    f.uvMap = dataAnim.uvFramesArray[a.currentFrameIndex];
                    a.currentFrameIndex++;
                }

            }
        }
    }

}
