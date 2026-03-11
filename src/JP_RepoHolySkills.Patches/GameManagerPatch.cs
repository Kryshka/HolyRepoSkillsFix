using HarmonyLib;
using JP_RepoHolySkills.GlobalMananger;
using JP_RepoHolySkills.SkillSelector;
using UnityEngine;

namespace JP_RepoHolySkills.Patches;

[HarmonyPatch(typeof(GameManager))]
internal class GameManagerPatch
{
	[HarmonyPatch("Awake")]
	[HarmonyPostfix]
	public static void Awake(GameManager __instance)
	{
		if ((UnityEngine.Object)(object)__instance.GetComponent<SkillSelectorController>() == (UnityEngine.Object)null)
		{
			Plugin.Logger.LogInfo((object)"GameManagerPatch Awake: Adding SkillSelectorController to GameManager.");
			((Component)__instance).gameObject.AddComponent<SkillSelectorController>();
		}

		if ((UnityEngine.Object)(object)JPSkill_GlobalManager.Instance == (UnityEngine.Object)null)
		{
			Plugin.Logger.LogInfo((object)"GameManagerPatch Awake: JPSkill_GlobalManager instance not found. Creating JPSkill_GlobalManagerGameObject...");
			GameObject val = new GameObject("JPSkill_GlobalManagerGameObject");
			val.AddComponent<JPSkill_GlobalManager>();
		}
	}
}
