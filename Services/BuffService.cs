using Stunlock.Core;
using ProjectM.Network;
using Unity.Entities;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.Shared;

namespace CustomAbilities.Services;

internal static class BuffService
{
  static DebugEventsSystem DebugEventsSystem => Core.DebugEventsSystem;
  static ServerGameManager ServerGameManager => Core.ServerGameManager;
  static EntityManager EntityManager => Core.EntityManager;

  public static void ApplyBuff(Entity entity, PrefabGUID buffPrefabGuid)
  {
    // Create buff application event
    ApplyBuffDebugEvent applyBuffDebugEvent = new()
    {
      BuffPrefabGUID = buffPrefabGuid
    };

    // Create character reference
    FromCharacter fromCharacter = new()
    {
      Character = entity,
      User = EntityManager.HasComponent<PlayerCharacter>(entity)
        ? EntityManager.GetComponentData<PlayerCharacter>(entity).UserEntity
        : EntityManager.HasComponent<User>(entity) ? entity : entity
    };

    // Apply the buff
    DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);
  }

  public static bool TryGetBuff(Entity entity, PrefabGUID buffPrefabGUID, out Entity buffEntity)
  {
    if (ServerGameManager.TryGetBuff(entity, buffPrefabGUID.ToIdentifier(), out buffEntity))
    {
      return true;
    }

    return false;
  }

  public static void TryRemoveBuff(Entity entity, PrefabGUID buffPrefabGuid)
  {
    if (TryGetBuff(entity, buffPrefabGuid, out Entity buffEntity))
    {
      DestroyBuff(buffEntity);
    }
  }

  public static void DestroyBuff(Entity buffEntity)
  {
    if (buffEntity != Entity.Null && EntityManager.Exists(buffEntity)) DestroyUtility.Destroy(EntityManager, buffEntity, DestroyDebugReason.TryRemoveBuff);
  }

  public static bool RemoveBuff(Entity entity, PrefabGUID buffPrefabGuid)
  {
    TryRemoveBuff(entity, buffPrefabGuid);
    return true;
  }

  public static bool HasBuff(Entity entity, PrefabGUID buffPrefabGuid)
  {
    return ServerGameManager.HasBuff(entity, buffPrefabGuid.ToIdentifier());
  }
}
