using HarmonyLib;

namespace JP_RepoHolySkills.Patches;

[HarmonyPatch(typeof(PlayerController))]
internal class PlayerControllerPatch
{
	[HarmonyPatch("Awake")]
	[HarmonyPrefix]
	public static void Awake(PlayerController __instance)
	{
	}

	[HarmonyPatch("Update")]
	[HarmonyPrefix]
	public static void Update(PlayerController __instance)
	{
	}
}
