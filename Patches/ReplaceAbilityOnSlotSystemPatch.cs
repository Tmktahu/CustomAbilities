using CustomAbilities.Services;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Stunlock.Core;
using CustomAbilities.Resources;
using CustomAbilities.Models;

namespace CustomAbilities.Patches;

[HarmonyPatch]
internal static class ReplaceAbilityOnSlotSystemPatch
{
    static EntityManager EntityManager => Core.EntityManager;

    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), nameof(ReplaceAbilityOnSlotSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(ReplaceAbilityOnSlotSystem __instance)
    {
        if (!Core._initialized) return;

        NativeArray<Entity> entities = __instance.__query_1482480545_0.ToEntityArray(Allocator.Temp);

        try
        {
            foreach (Entity buffEntity in entities)
            {
                try
                {
                    PrefabGUID prefabGUID = EntityManager.GetComponentData<PrefabGUID>(buffEntity);
                    if (prefabGUID.IsEmpty()) continue;

                    if (prefabGUID.Equals(PrefabGUIDs.AB_Feed_01_EnemyTarget_Debuff))
                        continue; // Skip this specific buff

                    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(prefabGUID);
                    if (string.IsNullOrEmpty(prefabName)) continue; // Skip if prefabName is invalid
                    // Core.Log.LogInfo($"[ReplaceAbilityOnSlotSystemPatch Prefix] - Processing Buff Entity: {buffEntity} with Prefab: {prefabName}");

                    if (!buffEntity.TryGetComponent(out EntityOwner entityOwner) || entityOwner.Owner == Entity.Null || !EntityManager.Exists(entityOwner.Owner)) continue;
                    else if (EntityManager.HasComponent<PlayerCharacter>(entityOwner.Owner))
                    {
                        Entity character = entityOwner.Owner;
                        PlayerData playerData = PlayerDataService.GetPlayerData(character);
                        int[] abilitySlotPrefabGUIDs = null;

                        int[] formOverride = AbilityAPI.RunFormAbilityOverrides(character, prefabGUID);
                        if (formOverride != null)
                        {
                            AbilityService.ApplyAbilities(character, buffEntity, formOverride, 9);
                            continue;
                        }

                        if (prefabName.Contains("unarmed", Il2CppSystem.StringComparison.OrdinalIgnoreCase) ||
                            prefabName.Contains("weapon", Il2CppSystem.StringComparison.OrdinalIgnoreCase))
                        {
                            // item abilities take priority over categorical abilities
                            // get the player's equipment component
                            Equipment equipment = EntityManager.GetComponentData<Equipment>(character);
                            EquipmentSlot weaponSlot = equipment.WeaponSlot;
                            Entity itemEntity = weaponSlot.SlotEntity._Entity;

                            // first we get the categorical abilities
                            foreach (var category in AbilityService.weaponCategories)
                            {
                                if (prefabName.Contains(category, Il2CppSystem.StringComparison.OrdinalIgnoreCase) && abilitySlotPrefabGUIDs == null)
                                {
                                    AbilityService.WeaponCategoryToDefaultEquipBuff.TryGetValue(category, out var defaultEquipBuff);
                                    playerData.AbilitySlotDefinitions.TryGetValue(defaultEquipBuff._Value, out abilitySlotPrefabGUIDs);
                                    break;
                                }
                            }

                            // Clone to prevent merge logic from mutating persisted category data
                            if (abilitySlotPrefabGUIDs != null)
                            {
                                abilitySlotPrefabGUIDs = (int[])abilitySlotPrefabGUIDs.Clone();
                            }

                            // then we check if there are item abilities
                            if (EntityManager.Exists(itemEntity) && ItemDataService.HasItemData(itemEntity))
                            {
                                Services.ItemData itemData = ItemDataService.GetItemData(itemEntity);
                                int[] itemAbilitySlotPrefabGUIDs = itemData.AbilityGUIDs;

                                // now we want to merge them.
                                // item ones take priority, so we overwrite the categorical ones if the
                                // item ones are not 0
                                if (abilitySlotPrefabGUIDs == null)
                                {
                                    abilitySlotPrefabGUIDs = itemAbilitySlotPrefabGUIDs;
                                }
                                else if (itemAbilitySlotPrefabGUIDs != null)
                                {
                                    for (int i = 0; i < abilitySlotPrefabGUIDs.Length; i++)
                                    {
                                        if (itemAbilitySlotPrefabGUIDs.Length > i && itemAbilitySlotPrefabGUIDs[i] != 0)
                                        {
                                            abilitySlotPrefabGUIDs[i] = itemAbilitySlotPrefabGUIDs[i];
                                        }
                                    }
                                }
                            }

                            // Clone to prevent HandleSpecialCases from mutating any persisted data
                            if (abilitySlotPrefabGUIDs != null)
                            {
                                abilitySlotPrefabGUIDs = (int[])abilitySlotPrefabGUIDs.Clone();
                            }
                            else
                            {
                                abilitySlotPrefabGUIDs = new int[8]; // default size for ability slots
                            }

                            abilitySlotPrefabGUIDs = AbilityService.HandleSpecialCases(character, prefabGUID, abilitySlotPrefabGUIDs);

                            AbilityService.ApplyAbilities(character, buffEntity, abilitySlotPrefabGUIDs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.LogError($"ReplaceAbilityOnSlotSystemPatch Prefix: Exception processing entity {buffEntity.Index}: {ex.Message}");
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
}