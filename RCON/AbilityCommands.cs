// using CustomAbilities.Services;
// using CustomAbilities.Commands;

// namespace CustomAbilities.RCON;

// [RconCommandCategory("Abilities")]
// public static class AbilityCommands
// {
//   [RconCommand("ability.set", "Set an ability for the target player")]
//   public static string SetAbilityCommand(int placementId, int prefabGuid, string playerName, string weaponCategory)
//   {
//     CommandResult result = AbilityService.SetAbilityCommand(placementId, prefabGuid, playerName, weaponCategory);
//     return result.Message;
//   }

//   [RconCommand("ability.clear", "Remove an ability from the target player")]
//   public static string ClearAbilityCommand(int placementId, string playerName, string weaponCategory)
//   {
//     CommandResult result = AbilityService.ClearAbilitySlotCommand(placementId, playerName, weaponCategory);
//     return result.Message;
//   }

//   [RconCommand("ability.clearall", "Remove all abilities from the target player")]
//   public static string ClearAllAbilitiesCommand(string playerName, string weaponCategory = null)
//   {
//     CommandResult result = AbilityService.ClearAllAbilitySlotsCommand(playerName, weaponCategory);
//     return result.Message;
//   }
// }
