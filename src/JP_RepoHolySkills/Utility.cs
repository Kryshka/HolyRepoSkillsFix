using System.Linq;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace JP_RepoHolySkills;

public class Utility
{
	public static GameObject FindSkillTierContainer(int index)
	{
		string text = $"SkillTier{index}Container";
		return GameObject.Find(text);
	}

	public static GameObject FindSkillTierDescription(int index)
	{
		string text = $"SkillTier{index}Description";
		return GameObject.Find(text);
	}

	public static void SetSkillTierContainerColor(int index, Color newColor)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = FindSkillTierContainer(index);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			Image component = val.GetComponent<Image>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				((Graphic)component).color = newColor;
			}
			else
			{
				Debug.LogWarning((object)$"SkillTier{index}Container does not have an Image component.");
			}
		}
		else
		{
			Debug.LogWarning((object)$"SkillTier{index}Container not found in the scene.");
		}
	}

	public static void SetSkillTierContainerText(int index, string newText)
	{
		GameObject val = FindSkillTierContainer(index);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			TextMeshProUGUI component = val.GetComponent<TextMeshProUGUI>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				((TMP_Text)component).text = newText;
			}
			else
			{
				Debug.LogWarning((object)$"SkillTier{index}Container does not have a TextMeshProUGUI component.");
			}
		}
		else
		{
			Debug.LogWarning((object)$"SkillTier{index}Container not found in the scene.");
		}
	}

	public static string FormatTotalExtractedHaul(int haulAmount)
	{
		string text = haulAmount.ToString("N0");
		return "<b><color=#FFFFFF>Total Extracted Haul:</color> <color=#00FFFF>" + text + "</color></b>";
	}

	public static void UpdateTotalExtractedHaulText(int extractedHaul)
	{
		GameObject val = GameObject.Find("SkillTotalHaulText");
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			TextMeshProUGUI component = val.GetComponent<TextMeshProUGUI>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				((TMP_Text)component).text = FormatTotalExtractedHaul(extractedHaul);
			}
		}
	}

	public static void TriggerWarCry(ConfigEntry<bool> enableWarCries, ConfigEntry<string> warCriesConfig, string context, Color textColor, ChatManager chatManager)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if (!enableWarCries.Value)
		{
			Plugin.Logger.LogInfo((object)(context + ": War cries are disabled via config. Skipping trigger."));
			return;
		}
		string[] array = (from s in warCriesConfig.Value.Split(',')
			select s.Trim() into s
			where !string.IsNullOrEmpty(s)
			select s).ToArray();
		if (array.Length == 0)
		{
			Plugin.Logger.LogWarning((object)(context + ": No war cries found in config. Aborting trigger."));
			return;
		}
		int num = Random.Range(0, array.Length);
		string text = array[num];
		Plugin.Logger.LogInfo((object)(context + ": Selected war cry: " + text));
		chatManager.PossessChatScheduleStart(10);
		chatManager.PossessChat((ChatManager.PossessChatID)3, text, 1.5f, textColor, 0f, false, 0, (UnityEvent)null);
		chatManager.PossessChatScheduleEnd();
	}
}
