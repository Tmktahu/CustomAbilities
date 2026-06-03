using Unity.Entities;
using Stunlock.Core;
using ProjectM;
using CustomAbilities.Services;

namespace CustomAbilities;

/// <summary>
/// Public API for CustomAbilities. Reference CustomAbilities.dll and call these methods from other mods.
/// Add [BepInDependency("io.vrising.CustomAbilities")] to your Plugin class.
/// </summary>
public static class AbilityAPI
{
  // Special case handlers registered by other mods.
  // Each handler receives (playerEntity, weaponEquipBuff, abilitySlotPrefabGUIDs) and returns the modified array.
  private static readonly List<Func<Entity, PrefabGUID, int[], int[]>> _specialCaseHandlers = new();

  /// <summary>
  /// Register a callback that runs during weapon equip buff application to modify the ability slot array.
  /// Use this to inject tag- or state-specific slot overrides (e.g. Beyla blood moon, Lucius).
  /// </summary>
  public static void RegisterSpecialCaseHandler(Func<Entity, PrefabGUID, int[], int[]> handler)
  {
    _specialCaseHandlers.Add(handler);
  }

  internal static int[] RunSpecialCaseHandlers(Entity playerEntity, PrefabGUID weaponEquipBuff, int[] abilitySlotPrefabGUIDs)
  {
    foreach (var handler in _specialCaseHandlers)
    {
      abilitySlotPrefabGUIDs = handler(playerEntity, weaponEquipBuff, abilitySlotPrefabGUIDs);
    }
    return abilitySlotPrefabGUIDs;
  }

  // Form ability override handlers registered by other mods.
  // Each handler receives (playerEntity, formBuffPrefabGUID) and returns an int[] to use, or null to skip.
  // First handler to return a non-null array wins.
  private static readonly List<Func<Entity, PrefabGUID, int[]>> _formAbilityOverrideHandlers = new();

  /// <summary>
  /// Register a callback that can supply an ability slot array for a shapeshift form buff.
  /// Return a non-null int[8] to override the form's abilities, or null to pass to the next handler.
  /// First non-null result wins. Use this to define form abilities from external mods.
  /// </summary>
  public static void RegisterFormAbilityOverride(Func<Entity, PrefabGUID, int[]> handler)
  {
    _formAbilityOverrideHandlers.Add(handler);
  }

  internal static int[] RunFormAbilityOverrides(Entity playerEntity, PrefabGUID formBuffPrefabGUID)
  {
    foreach (var handler in _formAbilityOverrideHandlers)
    {
      int[] result = handler(playerEntity, formBuffPrefabGUID);
      if (result != null) return result;
    }
    return null;
  }

  // --- Ability assignment ---

  public static void SetAbility(Entity playerEntity, AbilityTypeEnum slot, PrefabGUID ability, string weaponCategory, out string warningMessage)
    => AbilityService.SetAbility(playerEntity, slot, ability, weaponCategory, out warningMessage);

  public static void SetAbilityForAllCategories(Entity playerEntity, AbilityTypeEnum slot, PrefabGUID ability, out string warningMessage)
    => AbilityService.SetAbilityForAllCategories(playerEntity, slot, ability, out warningMessage);

  public static void ClearAbilitySlot(Entity playerEntity, AbilityTypeEnum slot, string weaponCategory)
    => AbilityService.ClearAbilitySlot(playerEntity, slot, weaponCategory);

  public static void ClearAbilitySlotForAllCategories(Entity playerEntity, AbilityTypeEnum slot)
    => AbilityService.ClearAbilitySlotForAllCategories(playerEntity, slot);

  public static void ClearAllAbilitySlots(Entity playerEntity, string weaponCategory)
    => AbilityService.ClearAllAbilitySlots(playerEntity, weaponCategory);

  public static void ClearAllAbilitySlotsForAllCategories(Entity playerEntity)
    => AbilityService.ClearAllAbilitySlotsForAllCategories(playerEntity);

  // --- Item ability assignment ---

  public static bool SetItemAbility(Entity itemEntity, AbilityTypeEnum slot, PrefabGUID ability, out string reason)
    => AbilityService.SetItemAbility(itemEntity, slot, ability, out reason);

  public static void ClearItemAbility(Entity itemEntity, AbilityTypeEnum slot)
    => AbilityService.ClearItemAbility(itemEntity, slot);

  public static void ClearAllItemAbilities(Entity itemEntity)
    => AbilityService.ClearAllItemAbilities(itemEntity);

  // --- Buff-level application ---

  public static void ApplyAbilities(Entity playerEntity, Entity buffEntity, int[] abilitySlotPrefabGUIDs, int priority = 1)
    => AbilityService.ApplyAbilities(playerEntity, buffEntity, abilitySlotPrefabGUIDs, priority);

  public static void ApplyAbilityBuff(Entity buffEntity, AbilityTypeEnum abilityType, PrefabGUID abilityGUID, int priority = 0)
    => AbilityService.ApplyAbilityBuff(buffEntity, abilityType, abilityGUID, priority);

  // --- Refresh ---

  public static void RefreshEquipBuff(Entity playerEntity)
    => Core.StartCoroutine(AbilityService.RefreshEquipBuff(playerEntity));

  // --- Spell Pool ---

  // Spell pool providers registered by other mods.
  // Each provider receives a player entity and returns a list of spell PrefabGUID hashes available to that player.
  private static readonly List<Func<Entity, List<int>>> _spellPoolProviders = new();

  /// <summary>
  /// Register a spell pool provider that returns spell PrefabGUID hashes available to a player.
  /// Called on-demand when querying available spells. Providers are source-agnostic;
  /// register from any mod that wants to contribute spells to a player's pool.
  /// </summary>
  public static void RegisterSpellPoolProvider(Func<Entity, List<int>> provider)
  {
    _spellPoolProviders.Add(provider);
  }

  /// <summary>
  /// Get all spells available to a player from all registered providers. Deduplicated, excludes 0 entries.
  /// </summary>
  public static List<int> GetAvailableSpells(Entity playerEntity)
  {
    var spells = new HashSet<int>();
    foreach (var provider in _spellPoolProviders)
    {
      var result = provider(playerEntity);
      if (result != null)
        foreach (var guid in result)
          if (guid != 0)
            spells.Add(guid);
    }
    return spells.ToList();
  }

  /// <summary>
  /// Check if a specific spell is available to a player from any registered provider.
  /// </summary>
  public static bool HasAvailableSpell(Entity playerEntity, int spellPrefabGuid)
  {
    foreach (var provider in _spellPoolProviders)
    {
      var result = provider(playerEntity);
      if (result != null && result.Contains(spellPrefabGuid))
        return true;
    }
    return false;
  }

  // --- Utility ---

  public static bool TryGetStandardizedWeaponCategory(string input, out string standardizedCategory)
    => AbilityService.TryGetStandardizedWeaponCategory(input, out standardizedCategory);

  public static string GetEquippedWeaponCategory(Entity playerEntity)
    => PlayerService.GetEquippedWeaponCategory(playerEntity);
}
