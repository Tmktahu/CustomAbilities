using CustomAbilities.Services;
using VampireCommandFramework;
using Unity.Entities;
using Stunlock.Core;

namespace CustomAbilities.Commands;

[CommandGroup("abilities", "ab")]
internal static class ItemCommands
{
  [Command("item set", "ias", "Set abilities on the currently held item", adminOnly: true)]
  public static void SetItemAbilitiesCommand(ChatCommandContext ctx, int placementId, int prefabGUID)
  {
    if (!AbilityService.PlacementIdToAbilityType.TryGetValue(placementId, out var slot))
    {
      ctx.Reply($"Invalid slot (<color=white>0</color> for Attack, <color=white>1</color> for Q, <color=white>2</color> for E, <color=white>3</color> for Dash, <color=white>4</color> for Spell 1, <color=white>5</color> for Spell 2, <color=white>6</color> for Ultimate)");
      return;
    }

    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Entity heldItemEntity = ItemService.GetHeldWeaponEntity(playerEntity);
    if (heldItemEntity == Entity.Null)
    {
      ctx.Reply("You are not holding any item.");
      return;
    }

    string reason;
    if (AbilityService.SetItemAbility(heldItemEntity, slot, new PrefabGUID(prefabGUID), out reason))
    {
      Core.StartCoroutine(AbilityService.RefreshEquipBuff(playerEntity));
      ctx.Reply($"Set ability {prefabGUID} on held item in slot {placementId}.");
    }
    else
    {
      ctx.Reply($"Failed to set ability: {reason}");
    }
  }

  [Command("item clear", "iac", "Clear abilities on the currently held item", adminOnly: true)]
  public static void ClearItemAbilitiesCommand(ChatCommandContext ctx, int placementId)
  {
    if (!AbilityService.PlacementIdToAbilityType.TryGetValue(placementId, out var slot))
    {
      ctx.Reply($"Invalid slot (<color=white>0</color> for Attack, <color=white>1</color> for Q, <color=white>2</color> for E, <color=white>3</color> for Dash, <color=white>4</color> for Spell 1, <color=white>5</color> for Spell 2, <color=white>6</color> for Ultimate)");
      return;
    }

    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Entity heldItemEntity = ItemService.GetHeldWeaponEntity(playerEntity);
    if (heldItemEntity == Entity.Null)
    {
      ctx.Reply("You are not holding any item.");
      return;
    }

    AbilityService.ClearItemAbility(heldItemEntity, slot);
    Core.StartCoroutine(AbilityService.RefreshEquipBuff(playerEntity));
    ctx.Reply($"Cleared ability on held item in slot {placementId}.");
  }

  [Command("item clearall", "iaca", "Clear all abilities on the currently held item", adminOnly: true)]
  public static void ClearAllItemAbilitiesCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Entity heldItemEntity = ItemService.GetHeldWeaponEntity(playerEntity);
    if (heldItemEntity == Entity.Null)
    {
      ctx.Reply("You are not holding any item.");
      return;
    }

    AbilityService.ClearAllItemAbilities(heldItemEntity);
    Core.StartCoroutine(AbilityService.RefreshEquipBuff(playerEntity));
    ctx.Reply("Cleared all abilities on held item.");
  }
}