using Unity.Entities;
using Stunlock.Core;
using ProjectM;
using CustomAbilities.Resources;
using UnityEngine;
using System.Collections;
using ProjectM.Network;
using Il2CppSystem;
using Unity.Mathematics;
using CustomAbilities.Commands;

namespace CustomAbilities.Services;

// AbilityTypeEnum values for reference:
// None = -1
// Primary = 0
// Secondary = 1
// Travel = 2
// Dash = 3
// Power = 4
// Offensive = 5
// SpellSlot1 = 5
// Defensive = 6
// SpellSlot2 = 6
// Ultimate = 7
public static class AbilityService
{
  static EntityManager EntityManager => Core.EntityManager;

  public static PrefabGUID DEFAULT_AXE_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Axe_Base;
  public static PrefabGUID DEFAULT_CLAWS_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Claws_Base;
  public static PrefabGUID DEFAULT_CROSSBOW_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Crossbow_Base;
  public static PrefabGUID DEFAULT_DAGGERS_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Daggers_Base;
  public static PrefabGUID DEFAULT_FISHING_POLE_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_FishingPole_Base;
  public static PrefabGUID DEFAULT_GREATSWORD_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_GreatSword_Base;
  public static PrefabGUID DEFAULT_LONGBOW_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Longbow_Base;
  public static PrefabGUID DEFAULT_MACE_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Mace_Base;
  public static PrefabGUID DEFAULT_PISTOLS_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Pistols_Base;
  public static PrefabGUID DEFAULT_REAPER_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Reaper_Base;
  public static PrefabGUID DEFAULT_SLASHERS_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Slashers_Base;
  public static PrefabGUID DEFAULT_SPEAR_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Spear_Base;
  public static PrefabGUID DEFAULT_SWORD_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Sword_Base;
  public static PrefabGUID DEFAULT_TWINBLADES_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_TwinBlades_Base;
  public static PrefabGUID DEFAULT_WHIP_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Whip_Base;
  public static PrefabGUID DEFAULT_UNARMED_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Unarmed_Start01;

  // define category constants
  public const string CATEGORY_AXE = "Axe";
  public const string CATEGORY_CLAWS = "Claws";
  public const string CATEGORY_CROSSBOW = "Crossbow";
  public const string CATEGORY_DAGGERS = "Daggers";
  public const string CATEGORY_FISHING_POLE = "FishingPole";
  public const string CATEGORY_GREATSWORD = "GreatSword";
  public const string CATEGORY_LONGBOW = "Longbow";
  public const string CATEGORY_MACE = "Mace";
  public const string CATEGORY_PISTOLS = "Pistols";
  public const string CATEGORY_REAPER = "Reaper";
  public const string CATEGORY_SLASHERS = "Slashers";
  public const string CATEGORY_SPEAR = "Spear";
  public const string CATEGORY_SWORD = "Sword";
  public const string CATEGORY_TWINBLADES = "TwinBlades";
  public const string CATEGORY_WHIP = "Whip";
  public const string CATEGORY_UNARMED = "Unarmed";

  public static List<string> weaponCategories = new List<string>
  {
      CATEGORY_UNARMED,
      CATEGORY_AXE,
      CATEGORY_CLAWS,
      CATEGORY_CROSSBOW,
      CATEGORY_DAGGERS,
      CATEGORY_FISHING_POLE,
      CATEGORY_GREATSWORD,
      CATEGORY_LONGBOW,
      CATEGORY_MACE,
      CATEGORY_PISTOLS,
      CATEGORY_REAPER,
      CATEGORY_SLASHERS,
      CATEGORY_SPEAR,
      CATEGORY_SWORD,
      CATEGORY_TWINBLADES,
      CATEGORY_WHIP
  };

  public static readonly Dictionary<string, PrefabGUID> WeaponCategoryToDefaultEquipBuff = new Dictionary<string, PrefabGUID>
  {
      { CATEGORY_AXE, DEFAULT_AXE_EQUIPBUFF },
      { CATEGORY_CLAWS, DEFAULT_CLAWS_EQUIPBUFF },
      { CATEGORY_CROSSBOW, DEFAULT_CROSSBOW_EQUIPBUFF },
      { CATEGORY_DAGGERS, DEFAULT_DAGGERS_EQUIPBUFF },
      { CATEGORY_FISHING_POLE, DEFAULT_FISHING_POLE_EQUIPBUFF },
      { CATEGORY_GREATSWORD, DEFAULT_GREATSWORD_EQUIPBUFF },
      { CATEGORY_LONGBOW, DEFAULT_LONGBOW_EQUIPBUFF },
      { CATEGORY_MACE, DEFAULT_MACE_EQUIPBUFF },
      { CATEGORY_PISTOLS, DEFAULT_PISTOLS_EQUIPBUFF },
      { CATEGORY_REAPER, DEFAULT_REAPER_EQUIPBUFF },
      { CATEGORY_SLASHERS, DEFAULT_SLASHERS_EQUIPBUFF },
      { CATEGORY_SPEAR, DEFAULT_SPEAR_EQUIPBUFF },
      { CATEGORY_SWORD, DEFAULT_SWORD_EQUIPBUFF },
      { CATEGORY_TWINBLADES, DEFAULT_TWINBLADES_EQUIPBUFF },
      { CATEGORY_WHIP, DEFAULT_WHIP_EQUIPBUFF },
      { CATEGORY_UNARMED, DEFAULT_UNARMED_EQUIPBUFF }
  };

  public static readonly Dictionary<int, AbilityTypeEnum> PlacementIdToAbilityType = new Dictionary<int, AbilityTypeEnum>
  {
      { 0, AbilityTypeEnum.Primary },
      { 1, AbilityTypeEnum.Secondary },
      { 2, AbilityTypeEnum.Power },
      { 3, AbilityTypeEnum.Travel },
      { 4, AbilityTypeEnum.Offensive },
      { 5, AbilityTypeEnum.Defensive },
      { 6, AbilityTypeEnum.Ultimate }
  };

  public static readonly PrefabGUID[] jewelableAbilityPrefabs = new PrefabGUID[]
  {
      PrefabGUIDs.AB_Blood_BloodFountain_AbilityGroup,
      PrefabGUIDs.AB_Blood_BloodRage_AbilityGroup,
      PrefabGUIDs.AB_Blood_BloodRite_AbilityGroup,
      PrefabGUIDs.AB_Blood_CarrionSwarm_AbilityGroup,
      PrefabGUIDs.AB_Blood_SanguineCoil_AbilityGroup,
      PrefabGUIDs.AB_Blood_Shadowbolt_AbilityGroup,
      PrefabGUIDs.AB_Vampire_VeilOfBlood_Group,

      PrefabGUIDs.AB_Unholy_ChainsOfDeath_AbilityGroup,
      PrefabGUIDs.AB_Unholy_CorpseExplosion_AbilityGroup,
      PrefabGUIDs.AB_Unholy_CorruptedSkull_AbilityGroup,
      PrefabGUIDs.AB_Unholy_DeathKnight_AbilityGroup,
      PrefabGUIDs.AB_Unholy_Soulburn_AbilityGroup,
      PrefabGUIDs.AB_Unholy_WardOfTheDamned_AbilityGroup,
      PrefabGUIDs.AB_Vampire_VeilOfBones_AbilityGroup,

      PrefabGUIDs.AB_Storm_BallLightning_AbilityGroup,
      PrefabGUIDs.AB_Storm_Cyclone_AbilityGroup,
      PrefabGUIDs.AB_Storm_Discharge_AbilityGroup,
      PrefabGUIDs.AB_Storm_LightningTendrils_AbilityGroup,
      PrefabGUIDs.AB_Storm_LightningWall_AbilityGroup,
      PrefabGUIDs.AB_Storm_PolarityShift_AbilityGroup,
      PrefabGUIDs.AB_Vampire_VeilOfStorm_Group,

      PrefabGUIDs.AB_Chaos_Aftershock_Group,
      PrefabGUIDs.AB_Chaos_Barrier_AbilityGroup,
      PrefabGUIDs.AB_Chaos_Volley_AbilityGroup,
      PrefabGUIDs.AB_Chaos_PowerSurge_AbilityGroup,
      PrefabGUIDs.AB_Chaos_RainOfChaos_AbilityGroup,
      PrefabGUIDs.AB_Chaos_Void_AbilityGroup,
      PrefabGUIDs.AB_Vampire_VeilOfChaos_Group,

      PrefabGUIDs.AB_Illusion_Curse_Group,
      PrefabGUIDs.AB_Illusion_MistTrance_AbilityGroup,
      PrefabGUIDs.AB_Illusion_Mosquito_AbilityGroup,
      PrefabGUIDs.AB_Illusion_PhantomAegis_AbilityGroup,
      PrefabGUIDs.AB_Illusion_SpectralWolf_AbilityGroup,
      PrefabGUIDs.AB_Illusion_WraithSpear_AbilityGroup,
      PrefabGUIDs.AB_Vampire_VeilOfIllusion_AbilityGroup,

      PrefabGUIDs.AB_Frost_ColdSnap_AbilityGroup,
      PrefabGUIDs.AB_Frost_CrystalLance_AbilityGroup,
      PrefabGUIDs.AB_Frost_FrostBat_AbilityGroup,
      PrefabGUIDs.AB_Frost_IceNova_AbilityGroup,
      PrefabGUIDs.AB_FrostBarrier_AbilityGroup,
      PrefabGUIDs.AB_FrostCone_AbilityGroup,
      PrefabGUIDs.AB_Vampire_VeilOfFrost_Group
  };

  public static readonly PrefabGUID[] recastableAbilities = new PrefabGUID[]
  {
      PrefabGUIDs.AB_Blood_BloodFountain_AbilityGroup,
      PrefabGUIDs.AB_Blood_CarrionSwarm_AbilityGroup,
      PrefabGUIDs.AB_Chaos_Barrier_AbilityGroup,
      PrefabGUIDs.AB_Chaos_PowerSurge_AbilityGroup,
      PrefabGUIDs.AB_Frost_IceNova_AbilityGroup,
      PrefabGUIDs.AB_FrostBarrier_AbilityGroup,
      PrefabGUIDs.AB_Illusion_PhantomAegis_AbilityGroup,
      PrefabGUIDs.AB_Illusion_WispDance_AbilityGroup,
      PrefabGUIDs.AB_Storm_BallLightning_AbilityGroup,
      PrefabGUIDs.AB_Storm_Discharge_AbilityGroup,
      PrefabGUIDs.AB_Storm_EyeOfTheStorm_AbilityGroup,
      PrefabGUIDs.AB_Unholy_UnstableArachnid_AbilityGroup,
      PrefabGUIDs.AB_Unholy_WardOfTheDamned_AbilityGroup,
      PrefabGUIDs.AB_Vampire_VeilOfChaos_Group,
      PrefabGUIDs.AB_Vampire_VeilOfIllusion_AbilityGroup,
  };

  public static bool TryGetStandardizedWeaponCategory(string input, out string standardizedCategory)
  {
    if (string.IsNullOrEmpty(input))
    {
      standardizedCategory = null;
      return false;
    }

    string lowerInput = input.ToLower();

    if (lowerInput == "all")
    {
      standardizedCategory = "All";
      return true;
    }

    foreach (string category in weaponCategories)
    {
      if (category.ToLower() == lowerInput)
      {
        standardizedCategory = category;
        return true;
      }
    }

    standardizedCategory = null;
    return false;
  }

  public static string GetValidWeaponCategories()
  {
    return $"All, {string.Join(", ", weaponCategories)}";
  }

  public static void SetAbility(Entity playerEntity, AbilityTypeEnum targetSlot, PrefabGUID abilityPrefab, string currentWeaponCategory, out string warningMessage)
  {
    warningMessage = null;
    // Get player data
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    WeaponCategoryToDefaultEquipBuff.TryGetValue(currentWeaponCategory, out PrefabGUID defaultEquipBuff);
    int PrefabGUIDInt = defaultEquipBuff._Value;

    // we go to the hash and override its value with new data, but that means we gotta get the existing data if it exists first
    Dictionary<int, int[]> abilitySlotDefinitions = playerData.AbilitySlotDefinitions;

    // check to see if we have an entry for this equip buff already
    if (!abilitySlotDefinitions.ContainsKey(PrefabGUIDInt))
    {
      // if not, we create a new entry with empty slots
      abilitySlotDefinitions[PrefabGUIDInt] = new int[8];
    }

    // if they assign a recastable ability to a slot, we want to warn them that they may lose their jewel on it
    if (abilityPrefab.HasValue() && recastableAbilities.Contains(abilityPrefab))
    {
      warningMessage = $"Using a recastable ability ({abilityPrefab}) on a non-normal slot may cause you to lose your jewel. You have been warned.";
    }

    playerData.AbilitySlotDefinitions[PrefabGUIDInt][(int)targetSlot] = abilityPrefab.GuidHash;
    PlayerDataService.SaveData();
  }

  public static void ClearAbilitySlot(Entity playerEntity, AbilityTypeEnum targetSlot, string currentWeaponCategory)
  {
    // Get player data
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    WeaponCategoryToDefaultEquipBuff.TryGetValue(currentWeaponCategory, out PrefabGUID defaultEquipBuff);
    int PrefabGUIDInt = defaultEquipBuff._Value;

    // we go to the hash and override its value with new data, but that means we gotta get the existing data if it exists first
    Dictionary<int, int[]> abilitySlotDefinitions = playerData.AbilitySlotDefinitions;

    // check to see if we have an entry for this equip buff already
    if (!abilitySlotDefinitions.ContainsKey(PrefabGUIDInt))
    {
      // if not, we create a new entry with empty slots
      abilitySlotDefinitions[PrefabGUIDInt] = new int[8];
    }

    playerData.AbilitySlotDefinitions[PrefabGUIDInt][(int)targetSlot] = 0; // 0 means empty
    PlayerDataService.SaveData();
  }

  public static void ClearAllAbilitySlots(Entity playerEntity, string currentWeaponCategory)
  {
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    WeaponCategoryToDefaultEquipBuff.TryGetValue(currentWeaponCategory, out PrefabGUID defaultEquipBuff);
    int PrefabGUIDInt = defaultEquipBuff._Value;

    // we go to the hash and override its value with new data, but that means we gotta get the existing data if it exists first
    Dictionary<int, int[]> abilitySlotDefinitions = playerData.AbilitySlotDefinitions;

    // check to see if we have an entry for this equip buff already
    if (!abilitySlotDefinitions.ContainsKey(PrefabGUIDInt))
    {
      // if not, we create a new entry with empty slots
      abilitySlotDefinitions[PrefabGUIDInt] = new int[8];
    }

    for (int i = 0; i < 8; i++)
    {
      playerData.AbilitySlotDefinitions[PrefabGUIDInt][i] = 0; // 0 means empty
    }
    PlayerDataService.SaveData();
  }

  public static void SetAbilityForAllCategories(Entity playerEntity, AbilityTypeEnum targetSlot, PrefabGUID abilityPrefab, out string warningMessage)
  {
    foreach (string category in weaponCategories)
    {
      SetAbility(playerEntity, targetSlot, abilityPrefab, category, out string individualWarningMessage);
      if (individualWarningMessage != null)
      {
        warningMessage = individualWarningMessage;
        return;
      }
    }
    warningMessage = null;
  }

  public static void ClearAbilitySlotForAllCategories(Entity playerEntity, AbilityTypeEnum targetSlot)
  {
    foreach (string category in weaponCategories)
    {
      ClearAbilitySlot(playerEntity, targetSlot, category);
    }
  }

  public static void ClearAllAbilitySlotsForAllCategories(Entity playerEntity)
  {
    foreach (string category in weaponCategories)
    {
      ClearAllAbilitySlots(playerEntity, category);
    }
  }

  public static bool SetItemAbility(Entity itemEntity, AbilityTypeEnum targetSlot, PrefabGUID abilityPrefab, out string reason)
  {
    // if they are attempting to set a jewelable ability, we decline it based on config
    if (abilityPrefab.HasValue() && jewelableAbilityPrefabs.Contains(abilityPrefab) && Plugin.BlockJewelableAbilitiesOnItems)
    {
      Core.Log.LogInfo($"[AbilityService] - Attempted to set jewelable ability {abilityPrefab} on item {itemEntity}, but this is not allowed by config.");
      reason = "Setting jewelable abilities on items is not allowed.";
      return false;
    }

    // Get item data
    var itemData = ItemDataService.GetItemData(itemEntity);

    itemData.AbilityGUIDs[(int)targetSlot] = abilityPrefab.GuidHash;
    ItemDataService.MarkDirty();
    reason = null;
    return true;
  }

  public static void ClearItemAbility(Entity itemEntity, AbilityTypeEnum targetSlot)
  {
    // Get item data
    var itemData = ItemDataService.GetItemData(itemEntity);

    itemData.AbilityGUIDs[(int)targetSlot] = 0; // 0 means empty
    ItemDataService.MarkDirty();
  }

  public static void ClearAllItemAbilities(Entity itemEntity)
  {
    var itemData = ItemDataService.GetItemData(itemEntity);

    for (int i = 0; i < 8; i++)
    {
      itemData.AbilityGUIDs[i] = 0; // 0 means empty
    }
    ItemDataService.MarkDirty();
  }

  public static void ApplyAbilities(Entity playerEntity, Entity buffEntity, int[] abilitySlotPrefabGUIDs, int groupPriority = 1)
  {
    // first we want to get the abilities for the associated player
    if (abilitySlotPrefabGUIDs == null) return;

    var buffer = EntityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(buffEntity);

    for (int i = 0; i < buffer.Length; i++)
    {
      var existingBuff = buffer[i];
      if (existingBuff.Priority == 99)
      {
        existingBuff.Priority = 8;
        buffer[i] = existingBuff;
      }
    }

    for (int i = 0; i < abilitySlotPrefabGUIDs.Length; i++)
    {
      if (abilitySlotPrefabGUIDs[i] == 0) continue; // 0 means skip

      var abilityPrefab = new PrefabGUID(abilitySlotPrefabGUIDs[i]);
      if (abilitySlotPrefabGUIDs[i] == -1)
        abilityPrefab = PrefabGUIDs.Zero; // -1 means we actually want to set the slot to nothing

      if (abilityPrefab.HasValue())
      {
        // Apply the ability to the player
        ApplyAbilityBuff(buffEntity, (AbilityTypeEnum)i, abilityPrefab, groupPriority);
      }
    }
  }

  public static void ApplyAbilityBuff(Entity buffEntity, AbilityTypeEnum abilityType, PrefabGUID abilityGUID, int priority = 0)
  {
    var buffer = EntityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(buffEntity);

    ReplaceAbilityOnSlotBuff buff;

    if (abilityGUID.Equals(PrefabGUIDs.Zero))
    {
      buff = new()
      {
        Slot = (int)abilityType,
        CopyCooldown = false,
        Priority = 99
      };
    }
    else
    {
      buff = new()
      {
        Slot = (int)abilityType,
        NewGroupId = abilityGUID,
        CopyCooldown = true,
        Priority = priority
      };
    }

    buffer.Add(buff);
  }

  public static PrefabGUID GetEquippedAbilityPrefabGuid(Entity playerEntity, AbilityTypeEnum targetSlot)
  {
    // we get the actually equipped ability prefab GUID from the player game data, not our data
    var buffEntities = EntityService.GetEntitiesByComponentTypes<Buff, PrefabGUID>();
    foreach (var buffEntity in buffEntities)
    {
      if (buffEntity.Read<EntityOwner>().Owner == playerEntity)
      {
        PrefabGUID buff = EntityManager.GetComponentData<PrefabGUID>(buffEntity);
        if (buff == PrefabGUIDs.Buff_VBlood_Ability_Replace)
        {
          if (EntityManager.TryGetComponentData<VBloodAbilityReplaceBuff>(buffEntity, out var vBloodAbilityReplaceBuff))
          {
            if (vBloodAbilityReplaceBuff.AbilityType == targetSlot)
            {
              return vBloodAbilityReplaceBuff.AbilityGUID;
            }
          }
        }
      }
    }

    return PrefabGUID.Empty;
  }

  public static IEnumerator RefreshEquipBuff(Entity playerEntity)
  {
    PrefabGUID currentEquipBuff = PlayerService.GetEquipBuffPrefabGUID(playerEntity);
    Core.Log.LogInfo($"[AbilityService] - Refreshing equip buff {currentEquipBuff} for player {playerEntity}");
    // to refresh the equip buff, we remove it and re-apply it
    if (BuffService.HasBuff(playerEntity, currentEquipBuff))
    {
      BuffService.RemoveBuff(playerEntity, currentEquipBuff);
    }

    yield return new WaitForSeconds(0.3f); // wait a short moment to ensure the buff is removed

    BuffService.ApplyBuff(playerEntity, currentEquipBuff);

    yield return null;
  }

  public static void CastAbility(Entity casterEntity, PrefabGUID abilityGroupPrefab)
  {
    DebugEventsSystem debugEventsSystem = Core.DebugEventsSystem;

    int fromUserIndex = -1;

    NetworkId whoNetworkId = default;
    if (casterEntity.TryGetComponent(out NetworkId netId))
      whoNetworkId = netId;
    Nullable_Unboxed<float3> aimPosition = new Nullable_Unboxed<float3>();

    FromCharacter fromCharacter = new()
    {
      Character = casterEntity,
      User = EntityManager.HasComponent<PlayerCharacter>(casterEntity)
        ? EntityManager.GetComponentData<PlayerCharacter>(casterEntity).UserEntity
        : EntityManager.HasComponent<User>(casterEntity) ? casterEntity : casterEntity
    };

    CastAbilityServerDebugEvent castAbilityServerDebugEvent = new CastAbilityServerDebugEvent
    {
      Who = whoNetworkId,
      AimPosition = aimPosition,
      AbilityGroup = abilityGroupPrefab
    };

    debugEventsSystem.CastAbilityServerDebugEvent(fromUserIndex, ref castAbilityServerDebugEvent, ref fromCharacter);
  }

  public static int[] HandleSpecialCases(Entity playerEntity, PrefabGUID weaponEquipBuff, int[] abilitySlotPrefabGUIDs)
  {
    return AbilityAPI.RunSpecialCaseHandlers(playerEntity, weaponEquipBuff, abilitySlotPrefabGUIDs);
  }

  // command related functions

  public static CommandResult SetAbilityCommand(int placementId, int prefabGuid, string playerName = null, string weaponCategory = null)
  {
    PrefabGUID abilityPrefabGuid = new PrefabGUID(prefabGuid);

    if (!PlacementIdToAbilityType.TryGetValue(placementId, out var slot))
    {
      return new() { Success = false, Message = "Invalid slot (<color=white>0</color> for Attack, <color=white>1</color> for Q, <color=white>2</color> for E, <color=white>3</color> for Dash, <color=white>4</color> for Spell 1, <color=white>5</color> for Spell 2, <color=white>6</color> for Ultimate)" };
    }

    if (!TryGetStandardizedWeaponCategory(weaponCategory, out string resolvedCategory))
    {
      return new() { Success = false, Message = $"'{weaponCategory}' is not a valid weapon category.\nAvailable categories: {GetValidWeaponCategories()}" };
    }

    if (!EntityService.TryFindPlayer(playerName, out Entity playerEntity, out Entity userEntity))
    {
      return new() { Success = false, Message = $"Player '{playerName}' not found." };
    }

    if (resolvedCategory == "All")
    {
      SetAbilityForAllCategories(playerEntity, slot, abilityPrefabGuid, out string warningMessage);
      return new() { Success = true, Message = warningMessage != null ? $"{warningMessage} Set ability slot {slot} to prefab GUID {prefabGuid} for player '{playerEntity.GetUser().CharacterName}' for ALL weapon categories." : $"Set ability slot {slot} to prefab GUID {prefabGuid} for player '{playerEntity.GetUser().CharacterName}' for ALL weapon categories." };
    }
    else
    {
      string categoryToUse = resolvedCategory ?? PlayerService.GetEquippedWeaponCategory(playerEntity);
      SetAbility(playerEntity, slot, abilityPrefabGuid, categoryToUse, out string warningMessage);
      return new() { Success = true, Message = warningMessage != null ? $"{warningMessage} Set ability slot {slot} to prefab GUID {prefabGuid} for player '{playerEntity.GetUser().CharacterName}' for {categoryToUse} category." : $"Set ability slot {slot} to prefab GUID {prefabGuid} for player '{playerEntity.GetUser().CharacterName}' for {categoryToUse} category." };
    }
  }

  public static CommandResult ClearAbilitySlotCommand(int placementId, string playerName = null, string weaponCategory = null)
  {
    if (!PlacementIdToAbilityType.TryGetValue(placementId, out var slot))
    {
      return new() { Success = false, Message = "Invalid slot (<color=white>0</color> for Attack, <color=white>1</color> for Q, <color=white>2</color> for E, <color=white>3</color> for Dash, <color=white>4</color> for Spell 1, <color=white>5</color> for Spell 2, <color=white>6</color> for Ultimate)" };
    }

    if (!TryGetStandardizedWeaponCategory(weaponCategory, out string resolvedCategory))
    {
      return new() { Success = false, Message = $"'{weaponCategory}' is not a valid weapon category.\nAvailable categories: {GetValidWeaponCategories()}" };
    }

    if (!EntityService.TryFindPlayer(playerName, out Entity playerEntity, out Entity userEntity))
    {
      return new() { Success = false, Message = $"Player '{playerName}' not found." };
    }

    if (resolvedCategory == "All")
    {
      ClearAbilitySlotForAllCategories(playerEntity, slot);
      return new() { Success = true, Message = $"Cleared ability slot {slot} for player '{playerEntity.GetUser().CharacterName}' for ALL weapon categories." };
    }
    else
    {
      string categoryToUse = resolvedCategory ?? PlayerService.GetEquippedWeaponCategory(playerEntity);
      ClearAbilitySlot(playerEntity, slot, categoryToUse);
      return new() { Success = true, Message = $"Cleared ability slot {slot} for player '{playerEntity.GetUser().CharacterName}' for {categoryToUse} category." };
    }
  }

  public static CommandResult ClearAllAbilitySlotsCommand(string playerName = null, string weaponCategory = null)
  {
    if (!TryGetStandardizedWeaponCategory(weaponCategory, out string resolvedCategory))
    {
      return new() { Success = false, Message = $"'{weaponCategory}' is not a valid weapon category.\nAvailable categories: {GetValidWeaponCategories()}" };
    }

    if (!EntityService.TryFindPlayer(playerName, out Entity playerEntity, out Entity userEntity))
    {
      return new() { Success = false, Message = $"Player '{playerName}' not found." };
    }

    if (resolvedCategory == "All")
    {
      ClearAllAbilitySlotsForAllCategories(playerEntity);
      return new() { Success = true, Message = $"Cleared ALL ability slots for player '{playerEntity.GetUser().CharacterName}' for ALL weapon categories." };
    }
    else
    {
      string categoryToUse = resolvedCategory ?? PlayerService.GetEquippedWeaponCategory(playerEntity);
      ClearAllAbilitySlots(playerEntity, categoryToUse);
      return new() { Success = true, Message = $"Cleared ALL ability slots for player '{playerEntity.GetUser().CharacterName}' for {categoryToUse} category." };
    }
  }

  public static CommandResult SpellAssignCommand(int placementId, int prefabGuid, Entity playerEntity)
  {
    PrefabGUID abilityPrefabGuid = new PrefabGUID(prefabGuid);

    if (!AbilityAPI.HasAvailableSpell(playerEntity, prefabGuid))
    {
      return new() { Success = false, Message = $"You don't have access to spell {prefabGuid}. Check your available spells with .abilities spell list." };
    }

    if (!PlacementIdToAbilityType.TryGetValue(placementId, out var slot))
    {
      return new() { Success = false, Message = "Invalid slot (<color=white>0</color> for Attack, <color=white>1</color> for Q, <color=white>2</color> for E, <color=white>3</color> for Dash, <color=white>4</color> for Spell 1, <color=white>5</color> for Spell 2, <color=white>6</color> for Ultimate)" };
    }

    string equippedCategory = PlayerService.GetEquippedWeaponCategory(playerEntity);
    SetAbilityForAllCategories(playerEntity, slot, abilityPrefabGuid, out string warningMessage);
    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(abilityPrefabGuid);
    string playerName = playerEntity.GetUser().CharacterName.ToString();

    return new()
    {
      Success = true,
      Message = warningMessage != null
        ? $"{warningMessage} Assigned {prefabName} to slot {slot} for {playerName} on all weapon categories."
        : $"Assigned {prefabName} to slot {slot} for {playerName} on all weapon categories."
    };
  }

  public static CommandResult SpellUnassignCommand(int placementId, Entity playerEntity)
  {
    if (!PlacementIdToAbilityType.TryGetValue(placementId, out var slot))
    {
      return new() { Success = false, Message = "Invalid slot (<color=white>0</color> for Attack, <color=white>1</color> for Q, <color=white>2</color> for E, <color=white>3</color> for Dash, <color=white>4</color> for Spell 1, <color=white>5</color> for Spell 2, <color=white>6</color> for Ultimate)" };
    }

    string playerName = playerEntity.GetUser().CharacterName.ToString();
    ClearAbilitySlotForAllCategories(playerEntity, slot);

    return new() { Success = true, Message = $"Cleared ability slot {slot} for {playerName} on all weapon categories." };
  }
}
