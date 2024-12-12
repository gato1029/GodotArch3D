using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;


public interface IAttackBehavior
{
    void Attack(Entity entity, ref Position position, ref Direction direction);
    bool AttackPosible(Entity entity, ref Position position, ref Direction direction);
}

