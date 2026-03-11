using HarmonyLib;
using JP_RepoHolySkills.MapToolControllerCustoms;
using UnityEngine;

namespace JP_RepoHolySkills.Patches;

[HarmonyPatch(typeof(MapToolController))]
internal class MapToolControllerPatch
{
	[HarmonyPatch("Start")]
	[HarmonyPrefix]
	public static void Start(MapToolController __instance)
	{
		Plugin.Logger.LogInfo((object)"MapToolControllerPatch start");
		((Component)__instance).gameObject.AddComponent<MapToolControllerCustom>();
	}
}
