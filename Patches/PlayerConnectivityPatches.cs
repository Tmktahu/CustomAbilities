using System;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Stunlock.Network;
using Unity.Collections;
using Unity.Entities;
using CustomAbilities.Services;
using CustomAbilities.Resources;

namespace CustomAbilities.Patches;

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
public static class OnUserDisconnected_Patch
{
	private static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason, string extraData)
	{
		if (Core.PlayerService == null) Core.Initialize();
		try
		{
			if (!__instance._NetEndPointToApprovedUserIndex.ContainsKey(netConnectionId)) return;
		int userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
			var serverClient = __instance._ApprovedUsersLookup[userIndex];
			var userEntity = serverClient.UserEntity;
			var userData = __instance.EntityManager.GetComponentData<User>(userEntity);
			bool isNewVampire = userData.CharacterName.IsEmpty;

			if (!isNewVampire)
			{
				PlayerDataService.FlushSaveToDisk();
			}
		}
		catch { }
	}
}