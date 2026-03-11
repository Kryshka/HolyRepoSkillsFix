using System;
using System.Reflection;
using HarmonyLib;
using JP_RepoHolySkills.GlobalMananger;

namespace JP_RepoHolySkills.Patches;

[HarmonyPatch(typeof(ExtractionPoint))]
internal class ExtractionPointPatch
{
	[HarmonyPatch("StateComplete")]
	[HarmonyPrefix]
	public static void StateComplete(ExtractionPoint __instance)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		if (SemiFunc.RunIsShop())
		{
			return;
		}
		FieldInfo field = typeof(ExtractionPoint).GetField("stateStart", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null || !(bool)field.GetValue(__instance))
		{
			return;
		}
		FieldInfo field2 = typeof(ExtractionPoint).GetField("extractionHaul", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field2 == null)
		{
			return;
		}
		int num = (int)field2.GetValue(__instance);
		Plugin.Logger.LogInfo((object)("ExtractionPointPatch: extractionHaul value = " + num));
		ES3Settings val = new ES3Settings("JPSkillRepo.es3", new Enum[1] { (Enum)(object)(ES3.Location)0 });
		try
		{
			int num2 = ES3.Load<int>("accumulatedExtractionHaul", 0, val);
			Plugin.Logger.LogInfo((object)("Loaded previous accumulated extraction haul: " + num2));
			int num3 = checked(num2 + num);
			Plugin.Logger.LogInfo((object)("New accumulated extraction haul = " + num3));
			ES3.Save<int>("accumulatedExtractionHaul", num3, val);
			Plugin.Logger.LogInfo((object)"Accumulated extraction haul saved successfully.");
			JPSkill_GlobalManager.Instance.savedExtractionHaul = num3;
		}
		catch (OverflowException ex)
		{
			Plugin.Logger.LogWarning((object)("Overflow occurred while saving extraction haul: " + ex.Message));
		}
		catch (Exception ex2)
		{
			Plugin.Logger.LogError((object)("Failed to load & save new extraction haul value: " + ex2.Message));
		}
	}
}
