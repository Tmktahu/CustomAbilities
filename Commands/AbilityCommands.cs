using CustomAbilities.Services;
using System.Text;
using VampireCommandFramework;
using Unity.Entities;
using Stunlock.Core;
using ProjectM;
using CustomAbilities.Resources;

namespace CustomAbilities.Commands;

[CommandGroup("abilities", "ab")]
internal static class AbilityCommands
{
  [Command("set primary", "set q", "Set your currently selected R spell to Q", adminOnly: false)]
  public static void SetSpellsCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;

    PrefabGUID spellPrefabGuid = AbilityService.GetEquippedAbilityPrefabGuid(playerEntity, AbilityTypeEnum.SpellSlot1);
    string warningMessage;
    AbilityService.SetAbility(playerEntity, AbilityTypeEnum.Secondary, spellPrefabGuid, AbilityService.CATEGORY_UNARMED, out warningMessage);
    AbilityService.SetAbility(playerEntity, AbilityTypeEnum.Secondary, spellPrefabGuid, AbilityService.CATEGORY_FISHING_POLE, out warningMessage);
    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(spellPrefabGuid);
    string replyMessage = warningMessage != null ? $"{warningMessage} Set Q to {prefabName}. Swap weapons to update your UI." : $"Set Q to {prefabName}. Swap weapons to update your UI.";
    ctx.Reply(replyMessage);
  }

  [Command("set secondary", "set e", "Set your currently selected C spell to E", adminOnly: false)]
  public static void SetSecondarySpellCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;

    PrefabGUID spellPrefabGuid = AbilityService.GetEquippedAbilityPrefabGuid(playerEntity, AbilityTypeEnum.SpellSlot2);
    string warningMessage;
    AbilityService.SetAbility(playerEntity, AbilityTypeEnum.Power, spellPrefabGuid, AbilityService.CATEGORY_UNARMED, out warningMessage);
    AbilityService.SetAbility(playerEntity, AbilityTypeEnum.Power, spellPrefabGuid, AbilityService.CATEGORY_FISHING_POLE, out warningMessage);
    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(spellPrefabGuid);
    string replyMessage = warningMessage != null ? $"{warningMessage} Set E to {prefabName}. Swap weapons to update your UI." : $"Set E to {prefabName}. Swap weapons to update your UI.";
    ctx.Reply(replyMessage);
  }

  [Command("clear primary", "clear q", "Clear the Q ability slot", adminOnly: false)]
  public static void ClearPrimaryAbilityCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;

    AbilityService.ClearAbilitySlot(playerEntity, AbilityTypeEnum.Secondary, AbilityService.CATEGORY_UNARMED);
    AbilityService.ClearAbilitySlot(playerEntity, AbilityTypeEnum.Secondary, AbilityService.CATEGORY_FISHING_POLE);
    ctx.Reply($"Cleared Q ability slot. Swap weapons to update your UI.");
  }

  [Command("clear secondary", "clear e", "Clear the E ability slot", adminOnly: false)]
  public static void ClearSecondaryAbilityCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;

    AbilityService.ClearAbilitySlot(playerEntity, AbilityTypeEnum.Power, AbilityService.CATEGORY_UNARMED);
    AbilityService.ClearAbilitySlot(playerEntity, AbilityTypeEnum.Power, AbilityService.CATEGORY_FISHING_POLE);
    ctx.Reply($"Cleared E ability slot. Swap weapons to update your UI.");
  }

  [Command("set", "as", "Set an ability to a slot", adminOnly: true)]
  public static void SetAbilityCommand(ChatCommandContext ctx, int placementId, int prefabGuid, string playerName = null, string weaponCategory = null)
  {
    if (string.IsNullOrEmpty(playerName))
      playerName = ctx.Event.SenderCharacterEntity.GetUser().CharacterName.ToString();

    if (string.IsNullOrEmpty(weaponCategory))
      weaponCategory = PlayerService.GetEquippedWeaponCategory(ctx.Event.SenderCharacterEntity);
    else
      AbilityService.TryGetStandardizedWeaponCategory(weaponCategory, out weaponCategory);

    CommandResult result = AbilityService.SetAbilityCommand(placementId, prefabGuid, playerName, weaponCategory);
    ctx.Reply(result.Message);
  }

  [Command("clear", "ac", "Clear an ability slot", adminOnly: true)]
  public static void ClearAbilitySlotCommand(ChatCommandContext ctx, int placementId, string playerName = null, string weaponCategory = null)
  {
    if (string.IsNullOrEmpty(playerName))
      playerName = ctx.Event.SenderCharacterEntity.GetUser().CharacterName.ToString();

    if (string.IsNullOrEmpty(weaponCategory))
      weaponCategory = PlayerService.GetEquippedWeaponCategory(ctx.Event.SenderCharacterEntity);
    else
      AbilityService.TryGetStandardizedWeaponCategory(weaponCategory, out weaponCategory);

    CommandResult result = AbilityService.ClearAbilitySlotCommand(placementId, playerName, weaponCategory);
    ctx.Reply(result.Message);
  }

  [Command("clearall", "aca", "Clear all ability slots", adminOnly: true)]
  public static void ClearAllAbilitySlotsCommand(ChatCommandContext ctx, string playerName = null, string weaponCategory = null)
  {
    if (string.IsNullOrEmpty(playerName))
      playerName = ctx.Event.SenderCharacterEntity.GetUser().CharacterName.ToString();

    if (string.IsNullOrEmpty(weaponCategory))
      weaponCategory = PlayerService.GetEquippedWeaponCategory(ctx.Event.SenderCharacterEntity);
    else
      AbilityService.TryGetStandardizedWeaponCategory(weaponCategory, out weaponCategory);

    CommandResult result = AbilityService.ClearAllAbilitySlotsCommand(playerName, weaponCategory);
    ctx.Reply(result.Message);
  }

  [Command("cast", null, "Cast an ability by prefab GUID", adminOnly: true)]
  public static void CastAbilityCommand(ChatCommandContext ctx, int prefabGuid)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;

    PrefabGUID abilityPrefabGuid = new PrefabGUID(prefabGuid);
    AbilityService.CastAbility(playerEntity, abilityPrefabGuid);
    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(abilityPrefabGuid);
    ctx.Reply($"Casting ability {prefabName}.");
  }

  [Command("spell list", "spl", "List available spells (optionally for another player as admin)", adminOnly: false)]
  public static void SpellListCommand(ChatCommandContext ctx, string playerName = null)
  {
    Entity playerEntity;
    string displayName;

    if (string.IsNullOrEmpty(playerName))
    {
      playerEntity = ctx.Event.SenderCharacterEntity;
      displayName = "You";
    }
    else
    {
      if (!ctx.Event.User.IsAdmin)
      {
        ctx.Reply("Only admins can view another player's spell pool.");
        return;
      }

      if (!EntityService.TryFindPlayer(playerName, out Entity targetEntity, out _))
      {
        ctx.Reply($"Could not find player '{playerName}'.");
        return;
      }

      playerEntity = targetEntity;
      displayName = playerName;
    }

    var spells = AbilityAPI.GetAvailableSpells(playerEntity);

    if (spells.Count == 0)
    {
      ctx.Reply(string.IsNullOrEmpty(playerName) ? "You have no spells available." : $"{playerName} has no spells available.");
      return;
    }

    var sb = new StringBuilder(string.IsNullOrEmpty(playerName) ? "Available spells:\n" : $"Spell pool for {playerName}:\n");
    foreach (int guid in spells)
    {
      var prefabGuid = new PrefabGUID(guid);
      string name = PrefabGUIDsExtensions.GetPrefabGUIDName(prefabGuid) ?? guid.ToString();
      sb.AppendLine($"  {name} ({guid})");
    }

    ctx.Reply(sb.ToString());
  }

  [Command("spell assign", "spa", "Assign a spell from your pool to a slot", adminOnly: false)]
  public static void SpellAssignCommand(ChatCommandContext ctx, int placementId, int prefabGuid)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    CommandResult result = AbilityService.SpellAssignCommand(placementId, prefabGuid, playerEntity);
    ctx.Reply(result.Message);
  }

  [Command("spell unassign", "spu", "Clear a spell from a slot", adminOnly: false)]
  public static void SpellUnassignCommand(ChatCommandContext ctx, int placementId)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    CommandResult result = AbilityService.SpellUnassignCommand(placementId, playerEntity);
    ctx.Reply(result.Message);
  }

}
