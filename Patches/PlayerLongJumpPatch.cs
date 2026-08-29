using System.Reflection;
using System.Reflection.Emit;
using Allumeria.ChunkManagement;
using Allumeria.EntitySystem;
using Allumeria.EntitySystem.Components;
using Allumeria.EntitySystem.Entities;
using HarmonyLib;
using LongJumpModule.Effects;
using OpenTK.Mathematics;

namespace LongJumpModule.Patches;

[HarmonyPatch(typeof(PlayerEntity), nameof(PlayerEntity.Movement))]
internal static class PlayerLongJumpMovementPatch
{
    private const float HorizontalSpeed = 1.6f;
    private const float VerticalSpeed = 0.18f;
    private const float AirFriction = 0.95f;

    private const float MinTriggerSpeed = 0.045f;

    private const int StaminaCost = 120;

    private static readonly Dictionary<PlayerEntity, float> AirFrictionBeforeLongJump = [];

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions
    )
    {
        MethodInfo lengthGetter = AccessTools.PropertyGetter(
            typeof(Vector3),
            nameof(Vector3.Length)
        );
        MethodInfo jump = AccessTools.Method(
            typeof(PhysicsComponent),
            nameof(PhysicsComponent.Jump)
        );
        MethodInfo hook = AccessTools.Method(
            typeof(PlayerLongJumpMovementPatch),
            nameof(TryLongJump)
        );

        CodeMatcher matcher = new CodeMatcher(instructions)
            // `if (moveDirection.Length > 0f)` - identifies the local holding the accumulated input direction
            .MatchStartForward(
                new CodeMatch(i => i.opcode == OpCodes.Ldloca_S || i.opcode == OpCodes.Ldloca),
                new CodeMatch(i => i.Calls(lengthGetter))
            )
            .ThrowIfInvalid("could not locate the movement direction local");

        CodeInstruction loadMoveDirection = new(
            matcher.Instruction.opcode == OpCodes.Ldloca ? OpCodes.Ldloc : OpCodes.Ldloc_S,
            matcher.Instruction.operand
        );

        return matcher
            // inject right after the grounded `phys.Jump(ignoreGrounded: true)` so we can override the jump velocity
            .MatchStartForward(new CodeMatch(i => i.Calls(jump)))
            .ThrowIfInvalid("could not locate the jump call")
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                loadMoveDirection,
                new CodeInstruction(OpCodes.Call, hook)
            )
            .InstructionEnumeration();
    }

    private static void TryLongJump(PlayerEntity player, Vector3 moveDirection)
    {
        if (player.phys.noclip || !player.phys.grounded)
            return;

        if (!player.sneaking || moveDirection.LengthSquared <= 0f)
            return;

        Vector3 horizontalVelocity = player.phys.velocity * new Vector3(1f, 0f, 1f);
        if (horizontalVelocity.LengthSquared < MinTriggerSpeed * MinTriggerSpeed)
            return;

        if (!player.effects.effectManager.activeEffects.ContainsKey(EffectRegistry.long_jump.intID))
            return;

        if (!player.TryConsumeStamina(StaminaCost))
            return;

        Vector3 direction = Vector3.Normalize(moveDirection * new Vector3(1f, 0f, 1f));
        float speed = HorizontalSpeed * player.effects.effectManager.stats.speedModifier;

        if (!AirFrictionBeforeLongJump.ContainsKey(player))
            AirFrictionBeforeLongJump[player] = player.phys.airFriction;

        player.phys.airFriction = MathF.Max(0.0001f, AirFriction);
        player.phys.velocity.X = direction.X * speed;
        player.phys.velocity.Z = direction.Z * speed;
        player.phys.velocity.Y = VerticalSpeed * player.phys.stats.jumpModifier;
        player.phys.ResetFallDamage();
    }

    internal static void RestoreAirFrictionIfGrounded(PlayerEntity player)
    {
        if (
            !player.phys.grounded
            || !AirFrictionBeforeLongJump.TryGetValue(player, out float originalAirFriction)
        )
            return;

        player.phys.airFriction = originalAirFriction;
        AirFrictionBeforeLongJump.Remove(player);
    }
}

[HarmonyPatch(typeof(PlayerEntity), nameof(PlayerEntity.Tick))]
internal static class PlayerLongJumpTickPatch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions
    )
    {
        MethodInfo entityTick = AccessTools.Method(
            typeof(Entity),
            nameof(Entity.Tick),
            [typeof(Chunk), typeof(World)]
        );
        MethodInfo restore = AccessTools.Method(
            typeof(PlayerLongJumpMovementPatch),
            nameof(PlayerLongJumpMovementPatch.RestoreAirFrictionIfGrounded)
        );

        CodeMatcher matcher = new CodeMatcher(instructions);

        while (matcher.MatchForward(false, new CodeMatch(i => i.Calls(entityTick))).IsValid)
        {
            matcher
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, restore)
                );
        }

        return matcher.InstructionEnumeration();
    }
}
