using Unity.Entities;
using ProjectM.Network;
using Unity.Collections;
using Il2CppInterop.Runtime;

namespace CustomAbilities.Services;

public static class EntityService
{
  static EntityManager EntityManager => Core.EntityManager;

  public static bool TryFindPlayer(string playerName, out Entity playerEntity, out Entity userEntity)
  {
    // if the playerName is null or empty, we return false
    if (string.IsNullOrEmpty(playerName))
    {
      playerEntity = Entity.Null;
      userEntity = Entity.Null;
      return false;
    }

    playerEntity = Entity.Null;
    userEntity = Entity.Null;

    var userEntities = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>())
                       .ToEntityArray(Allocator.Temp);

    foreach (var entity in userEntities)
    {
      var userData = EntityManager.GetComponentData<User>(entity);
      if (userData.CharacterName.ToString().Equals(playerName, StringComparison.OrdinalIgnoreCase))
      {
        userEntity = entity;
        playerEntity = userData.LocalCharacter._Entity;
        return true;
      }
    }

    return false;
  }

  public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
  {
    EntityQueryOptions options = EntityQueryOptions.Default;
    if (includeAll) options |= EntityQueryOptions.IncludeAll;
    if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
    if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
    if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
    if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

    var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
      .AddAll(new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite))
      .AddAll(new(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite))
      .WithOptions(options);

    var query = Core.EntityManager.CreateEntityQuery(ref entityQueryBuilder);

    var entities = query.ToEntityArray(Allocator.Temp);
    return entities;
  }
}
