using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using JP_RepoHolySkills.GlobalMananger;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JP_RepoHolySkills.SkillSelector;

public class SkillSelectorController : MonoBehaviour
{
	public string currentSkillDescription = "";

	public bool hasSetupSkillUI = false;

	public bool isSelectSkillUIShowing = false;

	private GameObject spawnedSelectSkillUI;

	public static SkillSelectorController Instance;

	private void Awake()
	{
		if ((UnityEngine.Object)(object)Instance == (UnityEngine.Object)null)
		{
			Instance = this;
			Plugin.Logger.LogInfo((object)"SkillSelectorController Awake: Instance set successfully.");
		}
		else
		{
			Plugin.Logger.LogWarning((object)"SkillSelectorController Awake: Duplicate instance detected!");
		}
	}

	private void Start()
	{
		Plugin.Logger.LogInfo((object)"SkillSelectorController Start: Initializing Skill Selector UI.");
		SetupSelectSkillUI();
	}

	private void Update()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		FieldInfo field = typeof(ChatManager).GetField("chatActive", BindingFlags.Instance | BindingFlags.NonPublic);
		if ((bool)field.GetValue(ChatManager.instance) || SemiFunc.RunIsLevel() || SemiFunc.RunIsShop())
		{
			return;
		}
		Utility.UpdateTotalExtractedHaulText(JPSkill_GlobalManager.Instance.savedExtractionHaul);
		KeyboardShortcut value = Plugin.SkillPageHotkey.Value;
		if (Input.GetKeyDown(((KeyboardShortcut)(value)).MainKey))
		{
			Plugin.Logger.LogInfo((object)"SkillSelectorController Update: P key pressed to toggle Skill Selector UI.");
			if ((UnityEngine.Object)(object)spawnedSelectSkillUI == (UnityEngine.Object)null)
			{
				Plugin.Logger.LogWarning((object)"SkillSelectorController Update: spawnedSelectSkillUI is null. Attempting to reinitialize UI.");
				hasSetupSkillUI = false;
				SetupSelectSkillUI();
			}
			if ((UnityEngine.Object)(object)spawnedSelectSkillUI != (UnityEngine.Object)null)
			{
				if (isSelectSkillUIShowing)
				{
					Plugin.Logger.LogInfo((object)"SkillSelectorController Update: Hiding Skill Selector UI.");
					spawnedSelectSkillUI.SetActive(false);
				}
				else
				{
					Plugin.Logger.LogInfo((object)"SkillSelectorController Update: Showing Skill Selector UI.");
					spawnedSelectSkillUI.SetActive(true);
					AutoSelectSkillUI();
				}
				isSelectSkillUIShowing = !isSelectSkillUIShowing;
			}
			else
			{
				Plugin.Logger.LogError((object)"SkillSelectorController Update: Failed to initialize Skill Selector UI; spawnedSelectSkillUI is still null.");
			}
		}
		if (isSelectSkillUIShowing)
		{
			HandleSkillSeletorMenuInput();
		}
	}

	public void HandleSkillSeletorMenuInput()
	{
		if (Input.GetKeyDown((KeyCode)49))
		{
			SelectHolyAuraSkill();
		}
		else if (Input.GetKeyDown((KeyCode)50))
		{
			SelectHealSkill();
		}
		else if (Input.GetKeyDown((KeyCode)51))
		{
			SelectHolyWallSkill();
		}
	}

	private void AutoSelectSkillUI()
	{
		if (JPSkill_GlobalManager.Instance.selectedSkill == SelectableSkills.HolyAura)
		{
			SelectHolyAuraSkill();
		}
		else if (JPSkill_GlobalManager.Instance.selectedSkill == SelectableSkills.Heal)
		{
			SelectHealSkill();
		}
		else if (JPSkill_GlobalManager.Instance.selectedSkill == SelectableSkills.HolyWall)
		{
			SelectHolyWallSkill();
		}
	}

	private void SelectHolyAuraSkill()
	{
		Plugin.Logger.LogInfo((object)"SkillSelectorController: Alpha1 pressed - Selecting HolyAura skill.");
		JPSkill_GlobalManager.Instance.selectedSkill = SelectableSkills.HolyAura;
		currentSkillDescription = ClassModConstants.HolyAuraDescription;
		SetSkillPointers(auraEnabled: true, healEnabled: false, wallEnabled: false);
		UpdateSkillDescriptionText(currentSkillDescription);
		ClearSkillTierDescriptions(5);
		UpdateSkillTierContainers(5);
		UpdateSkillTierDescriptions(ClassModConstants.HolyAuraTierDescriptions);
	}

	private void SelectHealSkill()
	{
		Plugin.Logger.LogInfo((object)"SkillSelectorController: Alpha2 pressed - Selecting Heal skill.");
		JPSkill_GlobalManager.Instance.selectedSkill = SelectableSkills.Heal;
		currentSkillDescription = ClassModConstants.HealDescription;
		SetSkillPointers(auraEnabled: false, healEnabled: true, wallEnabled: false);
		UpdateSkillDescriptionText(currentSkillDescription);
		ClearSkillTierDescriptions(5);
		UpdateSkillTierContainers(5);
		UpdateSkillTierDescriptions(ClassModConstants.HealSkillTierDescriptions);
	}

	private void SelectHolyWallSkill()
	{
		Plugin.Logger.LogInfo((object)"SkillSelectorController: Alpha3 pressed - Selecting Holy Wall skill.");
		JPSkill_GlobalManager.Instance.selectedSkill = SelectableSkills.HolyWall;
		currentSkillDescription = ClassModConstants.HolyWallDescription;
		SetSkillPointers(auraEnabled: false, healEnabled: false, wallEnabled: true);
		UpdateSkillDescriptionText(currentSkillDescription);
		ClearSkillTierDescriptions(5);
		UpdateSkillTierContainers(5);
		UpdateSkillTierDescriptions(ClassModConstants.HolyWallSkillTierDescriptions);
	}

	private void SetSkillPointers(bool auraEnabled, bool healEnabled, bool wallEnabled)
	{
		GameObject val = GameObject.Find("HolyAuraPointer");
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			RawImage component = val.GetComponent<RawImage>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				((Behaviour)component).enabled = auraEnabled;
			}
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HolyAuraPointer not found.");
		}
		GameObject val2 = GameObject.Find("HealPointer");
		if ((UnityEngine.Object)(object)val2 != (UnityEngine.Object)null)
		{
			RawImage component2 = val2.GetComponent<RawImage>();
			if ((UnityEngine.Object)(object)component2 != (UnityEngine.Object)null)
			{
				((Behaviour)component2).enabled = healEnabled;
			}
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HealPointer not found.");
		}
		GameObject val3 = GameObject.Find("HolyWallPointer");
		if ((UnityEngine.Object)(object)val3 != (UnityEngine.Object)null)
		{
			RawImage component3 = val3.GetComponent<RawImage>();
			if ((UnityEngine.Object)(object)component3 != (UnityEngine.Object)null)
			{
				((Behaviour)component3).enabled = wallEnabled;
			}
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HolyWallPointer not found.");
		}
	}

	private void UpdateSkillDescriptionText(string description)
	{
		GameObject val = GameObject.Find("JP_SkillDescriptionText");
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			TextMeshProUGUI component = val.GetComponent<TextMeshProUGUI>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				((TMP_Text)component).text = description;
			}
			else
			{
				Plugin.Logger.LogWarning((object)"JP_SkillDescriptionText does not have a TextMeshProUGUI component.");
			}
		}
		else
		{
			Plugin.Logger.LogWarning((object)"JP_SkillDescriptionText not found.");
		}
	}

	private void ClearSkillTierDescriptions(int numberOfTiers)
	{
		for (int i = 1; i <= numberOfTiers; i++)
		{
			GameObject val = Utility.FindSkillTierDescription(i);
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				TextMeshProUGUI component = val.GetComponent<TextMeshProUGUI>();
				if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
				{
					((TMP_Text)component).text = "";
					Plugin.Logger.LogInfo((object)$"SkillTier{i}Description text cleared.");
				}
				else
				{
					Plugin.Logger.LogWarning((object)$"SkillTier{i}Description GameObject does not have a TextMeshProUGUI component.");
				}
			}
			else
			{
				Plugin.Logger.LogWarning((object)$"SkillTier{i}Description not found in the scene.");
			}
		}
	}

	private void UpdateSkillTierContainers(int numberOfTiers)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i <= numberOfTiers; i++)
		{
			if (JPSkill_GlobalManager.Instance.savedExtractionHaul >= i * 500000)
			{
				Utility.SetSkillTierContainerColor(i, new Color(0.5235849f, 1f, 0.5453033f, 0.3921569f));
			}
			else
			{
				Utility.SetSkillTierContainerColor(i, new Color(1f, 1f, 1f, 0.4f));
			}
		}
	}

	private void UpdateSkillTierDescriptions(string[] descriptions)
	{
		for (int i = 1; i <= descriptions.Length; i++)
		{
			GameObject val = Utility.FindSkillTierDescription(i);
			if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
			{
				TextMeshProUGUI component = val.GetComponent<TextMeshProUGUI>();
				if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
				{
					((TMP_Text)component).text = "";
					((TMP_Text)component).text = descriptions[i - 1];
					Plugin.Logger.LogInfo((object)$"SkillTier{i}Description text set successfully.");
				}
				else
				{
					Plugin.Logger.LogWarning((object)$"SkillTier{i}Description GameObject does not have a TextMeshProUGUI component.");
				}
			}
			else
			{
				Plugin.Logger.LogWarning((object)$"SkillTier{i}Description not found in the scene.");
			}
		}
	}

	public void SetupSelectSkillUI()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		if (hasSetupSkillUI && (UnityEngine.Object)(object)spawnedSelectSkillUI != (UnityEngine.Object)null)
		{
			Plugin.Logger.LogInfo((object)"SkillSelectorController: SetupSelectSkillUI already executed. Skipping.");
			return;
		}
		hasSetupSkillUI = true;
		Plugin.Logger.LogInfo((object)"SkillSelectorController: Setting up Skill Selector UI.");
		if (Plugin.AssetManager.TryGetValue("SelectSkillUI", out var value))
		{
			GameObject val = GameObject.Find("Game Hud");
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				Plugin.Logger.LogWarning((object)"SkillSelectorController: 'Game Hud' canvas not found.");
				return;
			}
			spawnedSelectSkillUI = UnityEngine.Object.Instantiate<GameObject>(value, ((Component)this).transform.position, Quaternion.identity);
			spawnedSelectSkillUI.SetActive(false);
			spawnedSelectSkillUI.layer = 5;
			spawnedSelectSkillUI.transform.SetParent(val.transform, false);
			RectTransform component = val.GetComponent<RectTransform>();
			Rect rect = component.rect;
			float width = ((Rect)(rect)).width;
			Vector3 val2 = new Vector3(width - width / 2f - 150f, -30f, 0f);
			spawnedSelectSkillUI.transform.localPosition = val2;
			ManualLogSource logger = Plugin.Logger;
			Vector3 val3 = val2;
			logger.LogInfo((object)("SkillSelectorController: UI setup complete, added to canvas at local position " + ((object)(UnityEngine.Vector3)(val3)).ToString()));
		}
		else
		{
			Plugin.Logger.LogWarning((object)"SkillSelectorController: SELECT_SKILL_UI asset not found in AssetManager!");
		}
	}
}
