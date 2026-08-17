using Allumeria.EntitySystem.Effects;

namespace LongJumpMod.Effects;

public static class EffectRegistry
{
    public static Effect long_jump = null!;

    internal static void Initialize(string modId)
    {
        var highestId = Effect.effectsByString.Values.OrderBy(v => v.intID).First().intID;
        long_jump = new EffectLongJump(
            ++highestId,
            $"{modId}.long_jump",
            192,
            368,
            Effect.EffectType.Hidden
        );
    }
}
