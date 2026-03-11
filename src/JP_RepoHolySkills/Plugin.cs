using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JP_RepoHolySkills.Patches;
using UnityEngine;

namespace JP_RepoHolySkills;

[BepInPlugin("JP_RepoHolySkills", "RepoHolySkills", "1.1.7")]
public class Plugin : BaseUnityPlugin
{
	public const string VERSION = "v1.1.7";
	private readonly Harmony harmony = new Harmony("JP_RepoHolySkills");

	public static Plugin Instance;

	public static ManualLogSource Logger;

	public static Dictionary<string, GameObject> AssetManager;

	public static ConfigEntry<KeyboardShortcut> SkillPageHotkey;

	public static ConfigEntry<KeyboardShortcut> ActivateSkillHotkey;

	public ConfigEntry<string> holyWarCriesConfig;

	public ConfigEntry<string> healWarCriesConfig;

	public ConfigEntry<string> holyWallWarCriesConfig;

	public ConfigEntry<bool> enableWarCriesConfig;

	public GameObject stunGrenadePrefab;

	public GameObject shockwaveGrenadePrefab;

	public bool isInDebugMode = false;

	private void Awake()
	{
		Logger = base.Logger;
		Logger.LogInfo((object)$"!!! RepoHolySkills VERSION {VERSION} IS LOADING !!!");
		Logger.LogInfo((object)"Plugin JP_RepoHolySkills is loaded!");
		if ((UnityEngine.Object)(object)Instance == (UnityEngine.Object)null)
		{
			Instance = this;
			Logger.LogInfo((object)"Plugin: Instance set successfully.");
		}
		else
		{
			Logger.LogWarning((object)"Plugin: Multiple Plugin instances detected.");
		}
		SetupUserConfig();
		string directoryName = Path.GetDirectoryName(((BaseUnityPlugin)this).Info.Location);
		Logger.LogInfo((object)("Plugin: DLL directory is " + directoryName));
		string text = Path.Combine(directoryName, "jp_repoholyskillsprefabs");
		AssetBundle val = AssetBundle.LoadFromFile(text);
		AssetManager = new Dictionary<string, GameObject>();
		if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
		{
			Logger.LogError((object)("Plugin: Unable to load asset bundle from " + text));
		}
		else
		{
			Logger.LogInfo((object)"Plugin: Asset bundle loaded successfully.");
		}
		GameObject value = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HolyAura");
		GameObject value2 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HolyAuraBuff");
		GameObject value3 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HolyAuraIcon");
		GameObject value4 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HolyAuraSFX");
		GameObject value5 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HealSkillIcon");
		GameObject value6 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HealSkillSFX");
		GameObject value7 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HealSkill");
		GameObject value8 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HealReviveSkill");
		GameObject value9 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HealReviveSkillSFX");
		GameObject value10 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HolyWall");
		GameObject value11 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HolyWallSFX");
		GameObject value12 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "HolyWallIcon");
		GameObject value13 = LoadAssetFromAssetBundleAndLogInfo<GameObject>(val, "SelectSkillUI");
		AssetManager.Add("HolyAura", value);
		AssetManager.Add("HolyAuraBuff", value2);
		AssetManager.Add("HolyAuraIcon", value3);
		AssetManager.Add("HolyAuraSFX", value4);
		AssetManager.Add("HealSkill", value7);
		AssetManager.Add("HealSkillIcon", value5);
		AssetManager.Add("HealSkillSFX", value6);
		AssetManager.Add("HealReviveSkill", value8);
		AssetManager.Add("HealReviveSkillSFX", value9);
		AssetManager.Add("HolyWall", value10);
		AssetManager.Add("HolyWallIcon", value12);
		AssetManager.Add("HolyWallSFX", value11);
		AssetManager.Add("SelectSkillUI", value13);
		Logger.LogInfo((object)"Plugin: AssetManager populated successfully.");
		harmony.PatchAll(typeof(PlayerControllerPatch));
		harmony.PatchAll(typeof(MapToolControllerPatch));
		harmony.PatchAll(typeof(ExtractionPointPatch));
		harmony.PatchAll(typeof(PlayerAvatarPatch));
		harmony.PatchAll(typeof(GameManagerPatch));
		Logger.LogInfo((object)"Plugin: All Harmony patches applied successfully.");
		Logger.LogInfo((object)"Plugin: Awake completed.");
	}

	private void SetupUserConfig()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		SkillPageHotkey = ((BaseUnityPlugin)this).Config.Bind<KeyboardShortcut>("Keybinds", "OpenSkillSelectionPage", new KeyboardShortcut((KeyCode)112, Array.Empty<KeyCode>()), "The key used to open the skill selection page. Use Unity KeyCode names: https://docs.unity3d.com/ScriptReference/KeyCode.html");
		Logger.LogInfo((object)$"Config: SkillPageHotkey bound to {SkillPageHotkey.Value}");
		ActivateSkillHotkey = ((BaseUnityPlugin)this).Config.Bind<KeyboardShortcut>("Keybinds", "ActivateSkill", new KeyboardShortcut((KeyCode)114, Array.Empty<KeyCode>()), "The key used to activate the currently selected skill. Use Unity KeyCode names: https://docs.unity3d.com/ScriptReference/KeyCode.html");
		Logger.LogInfo((object)$"Config: ActivateSkillHotkey bound to {ActivateSkillHotkey.Value}");
		enableWarCriesConfig = ((BaseUnityPlugin)this).Config.Bind<bool>("WarCries", "EnableWarCries", true, "Set to false to disable war cries entirely.");
		Logger.LogInfo((object)$"Config: WarCries enabled = {enableWarCriesConfig.Value}");
		holyWarCriesConfig = ((BaseUnityPlugin)this).Config.Bind<string>("WarCries", "HolyWarCries", JoinDefaultsWarcries(ClassModConstants.HOLY_WAR_CRIES), "List of war cries shouted when casting holy aura (comma-separated)");
		Logger.LogInfo((object)("Config: Loaded HolyWarCries = [" + holyWarCriesConfig.Value + "]"));
		healWarCriesConfig = ((BaseUnityPlugin)this).Config.Bind<string>("WarCries", "HealWarCries", JoinDefaultsWarcries(ClassModConstants.HEAL_WAR_CRIES), "List of war cries shouted when casting healing (comma-separated)");
		Logger.LogInfo((object)("Config: Loaded HealWarCries = [" + healWarCriesConfig.Value + "]"));
		holyWallWarCriesConfig = ((BaseUnityPlugin)this).Config.Bind<string>("WarCries", "HolyWallWarCries", JoinDefaultsWarcries(ClassModConstants.HOLY_WALL_WAR_CRIES), "List of war cries shouted when casting holy wall (comma-separated)");
		Logger.LogInfo((object)("Config: Loaded HolyWallWarCries = [" + holyWallWarCriesConfig.Value + "]"));
	}

	private static string JoinDefaultsWarcries(string[] array)
	{
		return string.Join(",", array);
	}

	private static void NetcodeWeaver()
	{
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		Type[] array = types;
		foreach (Type type in array)
		{
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo[] array2 = methods;
			foreach (MethodInfo methodInfo in array2)
			{
				object[] customAttributes = methodInfo.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), inherit: false);
				if (customAttributes.Length != 0)
				{
					methodInfo.Invoke(null, null);
				}
			}
		}
		Logger.LogInfo((object)"Plugin: NetcodeWeaver executed.");
	}

	private T LoadAssetFromAssetBundleAndLogInfo<T>(AssetBundle bundle, string assetName) where T : UnityEngine.Object
	{
		T val = bundle.LoadAsset<T>(assetName);
		if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
		{
			Logger.LogError((object)("Plugin: " + assetName + " asset failed to load."));
		}
		else
		{
			Logger.LogInfo((object)("Plugin: " + assetName + " asset successfully loaded."));
		}
		return val;
	}
}
