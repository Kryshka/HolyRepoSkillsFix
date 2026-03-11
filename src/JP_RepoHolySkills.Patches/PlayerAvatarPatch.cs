using HarmonyLib;
using JP_RepoHolySkills.Player;
using UnityEngine;

namespace JP_RepoHolySkills.Patches;

[HarmonyPatch(typeof(PlayerAvatar))]
internal class PlayerAvatarPatch
{
	[HarmonyPatch("Awake")]
	[HarmonyPrefix]
	public static void Awake(PlayerAvatar __instance)
	{
		Plugin.Logger.LogInfo((object)"PlayerAvatarPatch awake");
		((Component)__instance).gameObject.AddComponent<PlayerControllerCustom>();
	}

	[HarmonyPatch("Update")]
	[HarmonyPrefix]
	public static void Update(PlayerAvatar __instance)
	{
	}
}
