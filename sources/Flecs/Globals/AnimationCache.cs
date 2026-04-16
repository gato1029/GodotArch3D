using GodotEcsArch.sources.managers.Accesories;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Globals;
internal static class AnimationCache
{
    public static  SpriteAnimationData GetAnimation( long idLong, EntityType entityType,
        AnimationType animationType,
        DirectionComponent dir)
    {           
        switch (entityType)
        {
            case EntityType.PERSONAJE:
                {
                    var model = MasterDataManager.GetData<TileSpriteData>(idLong).spriteMultipleAnimationDirection;
                    var animState = model.animationsTypes[animationType];  // model.animationCharacterBaseData.animationDataArray[stateAnimation];
                    var animData = animState.animations[dir.animationDirection];
                    return animData;
                }
            case EntityType.ACCESORIO:
                {
                    var model = MasterDataManager.GetData<TileSpriteData>(idLong).spriteMultipleAnimationDirection;
                    var animState = model.animationsTypes[ AnimationType.ARMA_ATACANDO];  // model.animationCharacterBaseData.animationDataArray[stateAnimation];
                    var animData = animState.animations[dir.animationDirection];
                    return animData;

                }
            default:
                throw new InvalidOperationException($"EntityType {idLong} no soportado para animación.");
        }
    }
}