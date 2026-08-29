using Allumeria.EntitySystem;
using Allumeria.EntitySystem.Effects;

namespace LongJumpModule.Effects;

public class EffectLongJump(
    int intID,
    string strID,
    int textureX,
    int textureY,
    Effect.EffectType type
) : Effect(intID, strID, textureX, textureY, type)
{
    public override void OnTick(Entity entity, ActiveEffect activeEffect, EffectManager manager)
    {
        base.OnTick(entity, activeEffect, manager);
    }
}
