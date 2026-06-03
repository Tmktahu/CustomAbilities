using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Stunlock.Core;
using CustomAbilities.Resources;

namespace CustomAbilities.Services;

internal class PlayerService
{
  internal PlayerService() { }

  public static PrefabGUID GetEquipBuffPrefabGUID(Entity playerEntity)
  {
    // we get all buffs on the character and loop through them
    var buffEntities = EntityService.GetEntitiesByComponentTypes<Buff, PrefabGUID>();

    foreach (var buffEntity in buffEntities)
    {
      PrefabGUID equippedWeaponPrefabGUID = default;
      if (buffEntity.Read<EntityOwner>().Owner == playerEntity)
      {
        // we need to get the name of the buff, so we need its prefab GUID
        PrefabGUID buff = Core.EntityManager.GetComponentData<PrefabGUID>(buffEntity);
        string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(buff);

        foreach (var category in AbilityService.weaponCategories)
        {
          if (prefabName.Contains(category, Il2CppSystem.StringComparison.OrdinalIgnoreCase))
          {
            equippedWeaponPrefabGUID = buff;
            break;
          }
        }
      }

      if (!equippedWeaponPrefabGUID.IsEmpty())
        return equippedWeaponPrefabGUID;
    }

    return default;
  }

  public static string GetEquippedWeaponCategory(Entity playerEntity)
  {
    PrefabGUID equipBuffPrefabGUID = GetEquipBuffPrefabGUID(playerEntity);
    if (!equipBuffPrefabGUID.IsEmpty())
    {
      string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(equipBuffPrefabGUID);
      foreach (var category in AbilityService.weaponCategories)
      {
        if (prefabName.Contains(category, Il2CppSystem.StringComparison.OrdinalIgnoreCase))
        {
          return category;
        }
      }
    }

    return "Unknown";
  }
}
