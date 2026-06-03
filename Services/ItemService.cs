using ProjectM;
using Unity.Entities;

namespace CustomAbilities.Services;

internal static class ItemService
{
  static EntityManager EntityManager => Core.EntityManager;

  public static Entity GetHeldWeaponEntity(Entity playerEntity)
  {
    if (EntityManager.TryGetComponentData<Equipment>(playerEntity, out var equipment))
    {
      EquipmentSlot weaponSlot = equipment.WeaponSlot;
      Entity weaponEntity = weaponSlot.SlotEntity._Entity;

      if (EntityManager.Exists(weaponEntity))
      {
        return weaponEntity;
      }
    }

    return Entity.Null;
  }

  public static int GetItemUniqueId(Entity itemEntity)
  {
    int guidHash = 0;
    if (EntityManager.TryGetComponentData(itemEntity, out SequenceGUID guid))
    {
      guidHash = guid.GuidHash;
      return guidHash;
    }
    else
    {
      guidHash = Guid.NewGuid().GetHashCode();

      guid = new SequenceGUID(guidHash);
      EntityManager.AddComponentData(itemEntity, guid);
    }

    return guidHash;
  }
}
