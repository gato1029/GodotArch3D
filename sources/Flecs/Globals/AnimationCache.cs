using GodotEcsArch.sources.managers.Accesories;
using GodotEcsArch.sources.managers.Characters;
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
    public static  AnimationData GetAnimation( long idLong, EntityType entityType,
        int state,
        DirectionComponent dir)
    {
        int id = (int)idLong;
        switch (entityType)
        {
            case EntityType.PERSONAJE:
                {
                    var model = CharacterModelManager.Instance.GetCharacterModel(id);
                    var animState = model.animationCharacterBaseData.animationDataArray[state];
                    var animData = animState.animationData[(int)dir.animationDirection];
                    return animData;
                }            
            case EntityType.ACCESORIO:
                {                    
                    var model = AccesoryManager.Instance.GetAccesory(id);
                    var animState = model.accesoryAnimationBodyData.animationStateData;
                    var animData = animState.animationData[(int)dir.animationDirection];
                    return animData;
                }                
            default:
                throw new InvalidOperationException($"EntityType {id} no soportado para animación.");
        }
    }
}