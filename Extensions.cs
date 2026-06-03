using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;

namespace CustomAbilities;

public static class VExtensions
{
  static EntityManager EntityManager => Core.EntityManager;

  public static bool TryGetComponent<T>(this Entity entity, out T componentData) where T : struct
  {
    componentData = default;

    if (EntityManager.HasComponent(entity, new(Il2CppType.Of<T>())))
    {
      componentData = EntityManager.GetComponentData<T>(entity);
      return true;
    }

    return false;
  }

  public static T Read<T>(this Entity entity) where T : struct
  {
    return EntityManager.GetComponentData<T>(entity);
  }

  public static User GetUser(this Entity entity)
  {
    if (entity.TryGetComponent(out User user)) return user;

    if (entity.TryGetComponent(out PlayerCharacter playerCharacter))
      return playerCharacter.UserEntity.Read<User>();

    return User.Empty;
  }

}
