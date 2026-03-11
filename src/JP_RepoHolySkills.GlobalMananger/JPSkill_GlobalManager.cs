using System;
using JP_RepoHolySkills.SkillSelector;
using Photon.Pun;
using UnityEngine;

namespace JP_RepoHolySkills.GlobalMananger;

public class JPSkill_GlobalManager : MonoBehaviour
{
	public int savedExtractionHaul = 0;

	public SelectableSkills selectedSkill = SelectableSkills.None;

	public static JPSkill_GlobalManager Instance;

	private void Awake()
	{
		if ((UnityEngine.Object)(object)Instance == (UnityEngine.Object)null)
		{
			Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(((Component)this).gameObject);
			Plugin.Logger.LogInfo((object)"JPSkill_GlobalManager Awake: Instance set successfully.");
		}
		else
		{
			UnityEngine.Object.Destroy(((Component)this).gameObject);
			Plugin.Logger.LogWarning((object)"JPSkill_GlobalManager Awake: An instance already exists! Destroying duplicate.");
		}
	}

	private void Start()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Expected O, but got Unknown
		try
		{
			ES3Settings val = new ES3Settings("JPSkillRepo.es3", new Enum[1] { (Enum)(object)(ES3.Location)0 });
			if (!ES3.FileExists(val) || !ES3.KeyExists("accumulatedExtractionHaul", val))
			{
				Plugin.Logger.LogInfo((object)"No saved extraction haul found. Creating file with default value 0.");
				ES3.Save<int>("accumulatedExtractionHaul", 0, val);
				savedExtractionHaul = 0;
			}
			else
			{
				savedExtractionHaul = (Plugin.Instance.isInDebugMode ? 3000000 : ES3.Load<int>("accumulatedExtractionHaul", val));
			}
			Plugin.Logger.LogInfo((object)$"JPSkill_GlobalManager Start: Loaded savedExtractionHaul = {savedExtractionHaul}.");
		}
		catch (Exception ex)
		{
			Plugin.Logger.LogError((object)("JPSkill_GlobalManager Start: Failed to load savedExtractionHaul: " + ex.Message));
		}
		if (Plugin.AssetManager.TryGetValue("HolyWall", out var value) && (UnityEngine.Object)(object)value != (UnityEngine.Object)null)
		{
			Plugin.Logger.LogInfo((object)("JPSkill_GlobalManager Start: Found Holy Wall prefab: " + ((UnityEngine.Object)value).name + "."));
		}
		else
		{
			Plugin.Logger.LogWarning((object)"JPSkill_GlobalManager Start: Holy Wall prefab not found in AssetManager.");
		}
		IPunPrefabPool val2 = PhotonNetwork.PrefabPool;
		if (val2 == null)
		{
			Plugin.Logger.LogInfo((object)"JPSkill_GlobalManager Start: No existing Photon prefab pool found, using DefaultPool.");
			val2 = (IPunPrefabPool)new DefaultPool();
		}
		else
		{
			Plugin.Logger.LogInfo((object)("JPSkill_GlobalManager Start: Existing Photon prefab pool found: " + ((object)val2).GetType().Name));
		}
		CombinedPrefabPool combinedPrefabPool = new CombinedPrefabPool(val2);
		Plugin.Logger.LogInfo((object)"JPSkill_GlobalManager Start: Created CombinedPrefabPool.");
		combinedPrefabPool.AddModdedPrefab("HolyWall", value);
		Plugin.Logger.LogInfo((object)"JPSkill_GlobalManager Start: Added modded Holy Wall prefab with key 'HolyWall'.");
		PhotonNetwork.PrefabPool = (IPunPrefabPool)(object)combinedPrefabPool;
		Plugin.Logger.LogInfo((object)"JPSkill_GlobalManager Start: PhotonNetwork.PrefabPool has been set to the CombinedPrefabPool.");
	}

	private void Update()
	{
		EnsurePrefabPool();
	}

	public void EnsurePrefabPool()
	{
		if (!(PhotonNetwork.PrefabPool is CombinedPrefabPool))
		{
			Plugin.Logger.LogWarning((object)("JPSkill_GlobalManager: PrefabPool was reset or not set. Restoring CombinedPrefabPool. Current: " + ((object)PhotonNetwork.PrefabPool)?.GetType().Name));
			IPunPrefabPool currentPool = PhotonNetwork.PrefabPool;
			if (currentPool == null)
			{
				currentPool = new DefaultPool();
			}
			CombinedPrefabPool newPool = new CombinedPrefabPool(currentPool);
			if (Plugin.AssetManager != null)
			{
				if (Plugin.AssetManager.TryGetValue("HolyWall", out var value))
				{
					newPool.AddModdedPrefab("HolyWall", value);
				}
				if (Plugin.AssetManager.TryGetValue("HealSkill", out var value2))
				{
					newPool.AddModdedPrefab("HealSkill", value2);
				}
				if (Plugin.AssetManager.TryGetValue("HolyAura", out var value3))
				{
					newPool.AddModdedPrefab("HolyAura", value3);
				}
			}
			PhotonNetwork.PrefabPool = (IPunPrefabPool)(object)newPool;
			Plugin.Logger.LogInfo((object)"JPSkill_GlobalManager: PhotonNetwork.PrefabPool has been successfully restored.");
		}
	}
}
