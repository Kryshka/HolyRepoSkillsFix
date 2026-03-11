using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using JP_RepoHolySkills.GlobalMananger;
using JP_RepoHolySkills.Player;
using JP_RepoHolySkills.SkillSelector;
using Photon.Pun;
using UnityEngine;

namespace JP_RepoHolySkills.Skills;

public class HealSkill : MonoBehaviour
{
	public int baseHealAmount = 10;

	public float healRange = 4.5f;

	public float healthRegenPercentage = 0.02f;

	public float regenDuration = 3f;

	public GameObject healIconInstance;

	public Color healUICooldownStartColor = new Color(1f, 1f, 1f, 0f);

	public Color healUICooldownEndColor = Color.white;

	public Color healUIColor = new Color(0f, 1f, 0f, 1f);

	public float cooldownDuration = 180f;

	private PhotonView pv;

	private bool isOnCooldown;

	private const string CANVAS_NAME = "Game Hud";

	private const int UI_LAYER = 5;

	private static readonly Vector3 UI_SCALE = new Vector3(13f, 13f, 13f);

	private const float UI_VERTICAL_OFFSET = 40f;

	private const float SOUND_VOLUME = 0.2f;

	private const int BASE_HEAL_UPGRADE_THRESHOLD = 500000;

	private const int DOUBLE_HEAL_RADIUS_THRESHOLD = 1000000;

	private const int HEAL_REGEN_INCREASE_THRESHOLD = 1500000;

	private const int REGEN_DURATION_INCREASE_THRESHOLD = 2000000;

	private const int REVIVAL_THRESHOLD = 2500000;



	private void Start()
	{
		pv = ((Component)this).GetComponent<PhotonView>();
		Plugin.Logger.LogInfo((object)"HealSkill: Started.");
		if (Plugin.Instance.isInDebugMode)
		{
			cooldownDuration = 1f;
		}
	}

	private void Update()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (ShouldProcessInput() && JPSkill_GlobalManager.Instance.selectedSkill == SelectableSkills.Heal)
		{
			SetupUIIfNeeded();
			KeyboardShortcut value = Plugin.ActivateSkillHotkey.Value;
			if (Input.GetKeyDown(((KeyboardShortcut)(value)).MainKey) && !isOnCooldown)
			{
				ActivateHealSkill();
			}
		}
	}

	private bool ShouldProcessInput()
	{
		if (!SemiFunc.RunIsLevel() || !pv.IsMine)
		{
			return false;
		}
		if (IsChatActive())
		{
			return false;
		}
		return true;
	}

	private bool IsChatActive()
	{
		FieldInfo field = typeof(ChatManager).GetField("chatActive", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null)
		{
			return false;
		}
		return (bool)field.GetValue(ChatManager.instance);
	}

	private void SetupUIIfNeeded()
	{
		if (JPSkill_GlobalManager.Instance.selectedSkill == SelectableSkills.Heal && (UnityEngine.Object)(object)healIconInstance == (UnityEngine.Object)null)
		{
			Plugin.Logger.LogInfo((object)"HealSkill: Rendering Heal UI for selected Heal skill.");
			RenderHealUI();
		}
	}

	private void ActivateHealSkill()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		Plugin.Logger.LogInfo((object)"HealSkill: Activated by local player.");
		isOnCooldown = true;
		Utility.TriggerWarCry(Plugin.Instance.enableWarCriesConfig, Plugin.Instance.healWarCriesConfig, "HealSkill", new Color(0.5f, 1f, 0.5f, 1f), ChatManager.instance);
		pv.RPC("PlayHealSkillSFX_RPC", (RpcTarget)0, new object[1] { ((Component)this).transform.position });
		ComputeEffectiveValues(out var effectiveBaseHeal, out var effectiveHealRange, out var effectiveRegenPercentage, out var effectiveRegenDuration, out var allowRevival, out var healParticleScaleMultiplier);
		pv.RPC("PlayHealSkillParticles_RPC", (RpcTarget)0, new object[2]
		{
			((Component)this).transform.position,
			healParticleScaleMultiplier
		});
		Plugin.Logger.LogInfo((object)"HealSkill: SFX and particle effects triggered.");
		int num = CountAdditionalPlayers(effectiveHealRange);
		ProcessRevival(effectiveHealRange, allowRevival);
		float num2 = 1f + (float)num * 0.03f;
		Plugin.Logger.LogInfo((object)$"HealSkill: Healing multiplier is {num2} ({num} additional players in range).");
		ProcessHealing(effectiveHealRange, effectiveBaseHeal, num2, effectiveRegenPercentage, effectiveRegenDuration);
		pv.RPC("PlayHealReviveSFX_RPC", (RpcTarget)0, Array.Empty<object>());
		if ((UnityEngine.Object)(object)healIconInstance != (UnityEngine.Object)null)
		{
			((MonoBehaviour)this).StartCoroutine(HealCooldownCount());
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HealSkill: healIconInstance is null; cannot start cooldown transparency effect.");
		}
	}

	private void ComputeEffectiveValues(out int effectiveBaseHeal, out float effectiveHealRange, out float effectiveRegenPercentage, out float effectiveRegenDuration, out bool allowRevival, out float healParticleScaleMultiplier)
	{
		int savedExtractionHaul = JPSkill_GlobalManager.Instance.savedExtractionHaul;
		effectiveBaseHeal = ((savedExtractionHaul >= 500000) ? 20 : baseHealAmount);
		effectiveHealRange = ((savedExtractionHaul >= 1000000) ? (healRange * 2f) : healRange);
		effectiveRegenPercentage = ((savedExtractionHaul >= 1500000) ? 0.04f : healthRegenPercentage);
		effectiveRegenDuration = ((savedExtractionHaul >= 2000000) ? 6f : regenDuration);
		allowRevival = savedExtractionHaul >= 2500000;
		healParticleScaleMultiplier = ((savedExtractionHaul >= 1000000) ? 2f : 1f);
	}

	private int CountAdditionalPlayers(float effectiveHealRange)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		List<PlayerAvatar> list = SemiFunc.PlayerGetAll();
		foreach (PlayerAvatar item in list)
		{
			if (!((UnityEngine.Object)(object)item == (UnityEngine.Object)null) && !((UnityEngine.Object)(object)((Component)item).gameObject == (UnityEngine.Object)(object)((Component)this).gameObject) && Vector3.Distance(((Component)this).transform.position, ((Component)item).transform.position) <= effectiveHealRange)
			{
				FieldInfo field = typeof(PlayerAvatar).GetField("deadSet", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && !(bool)field.GetValue(item))
				{
					num++;
				}
			}
		}
		return num;
	}

	private void ProcessRevival(float effectiveHealRange, bool allowRevival)
	{
		if (!allowRevival)
		{
			Plugin.Logger.LogInfo((object)"HealSkill ProcessRevival: Resurrection not allowed at this extraction haul level.");
			return;
		}

		Plugin.Logger.LogInfo((object)$"HealSkill ProcessRevival: Starting search for dead players within range {effectiveHealRange}.");

		PlayerDeathHead[] allDeathHeads = UnityEngine.Object.FindObjectsOfType<PlayerDeathHead>();
		Plugin.Logger.LogInfo((object)$"HealSkill ProcessRevival: Found {allDeathHeads.Length} PlayerDeathHead objects in total.");

		foreach (PlayerDeathHead deathHead in allDeathHeads)
		{
			if ((UnityEngine.Object)(object)deathHead == (UnityEngine.Object)null) continue;

			float distance = Vector3.Distance(((Component)this).transform.position, ((Component)deathHead).transform.position);
			PlayerAvatar playerAvatar = deathHead.playerAvatar;
			string playerName = ((UnityEngine.Object)(object)playerAvatar != (UnityEngine.Object)null) ? ((UnityEngine.Object)playerAvatar).name : "Unknown Player";

			Plugin.Logger.LogInfo((object)$"HealSkill ProcessRevival: Found death head for '{playerName}' at distance {distance:F2}.");

			if (distance <= effectiveHealRange)
			{
				if ((UnityEngine.Object)(object)playerAvatar == (UnityEngine.Object)null)
				{
					Plugin.Logger.LogWarning((object)"HealSkill ProcessRevival: deathHead.playerAvatar is null! Cannot revive.");
					continue;
				}

				FieldInfo deadSetField = typeof(PlayerAvatar).GetField("deadSet", BindingFlags.Instance | BindingFlags.NonPublic);
				bool isDead = (deadSetField != null) ? (bool)deadSetField.GetValue(playerAvatar) : false;

				Plugin.Logger.LogInfo((object)$"HealSkill ProcessRevival: Player '{playerName}' deadSet state: {isDead}.");

				if (isDead)
				{
					Plugin.Logger.LogInfo((object)$"HealSkill ProcessRevival: Reviving '{playerName}'...");
					playerAvatar.Revive(false);
					Plugin.Logger.LogInfo((object)$"HealSkill ProcessRevival: '{playerName}' revive method called.");
					pv.RPC("PlayHealReviveParticles_RPC", (RpcTarget)0, new object[1] { playerAvatar.photonView.ViewID });
				}
			}
			else
			{
				Plugin.Logger.LogInfo((object)$"HealSkill ProcessRevival: Death head for '{playerName}' is out of range.");
			}
		}
	}

	private void ProcessHealing(float effectiveHealRange, int effectiveBaseHeal, float healingMultiplier, float effectiveRegenPercentage, float effectiveRegenDuration)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		List<PlayerAvatar> list = SemiFunc.PlayerGetAll();
		foreach (PlayerAvatar item in list)
		{
			if ((UnityEngine.Object)(object)item == (UnityEngine.Object)null)
			{
				Plugin.Logger.LogWarning((object)"HealSkill: Encountered a null player avatar. Skipping.");
				continue;
			}
			float num = Vector3.Distance(((Component)this).transform.position, ((Component)item).transform.position);
			Plugin.Logger.LogInfo((object)$"HealSkill: Checking '{((UnityEngine.Object)item).name}' at distance {num:F2}.");
			if (num > effectiveHealRange)
			{
				Plugin.Logger.LogInfo((object)$"HealSkill: '{((UnityEngine.Object)item).name}' is outside the effective heal range ({effectiveHealRange}). Skipping.");
				continue;
			}
			PlayerHealth component = ((Component)item).GetComponent<PlayerHealth>();
			if ((UnityEngine.Object)(object)component == (UnityEngine.Object)null)
			{
				Plugin.Logger.LogWarning((object)("HealSkill: '" + ((UnityEngine.Object)item).name + "' has no PlayerHealth component. Skipping."));
				continue;
			}
			Plugin.Logger.LogInfo((object)("HealSkill: Found PlayerHealth for '" + ((UnityEngine.Object)item).name + "'."));
			FieldInfo field = typeof(PlayerHealth).GetField("maxHealth", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field == null)
			{
				Plugin.Logger.LogWarning((object)("HealSkill: 'maxHealth' field not found in PlayerHealth for '" + ((UnityEngine.Object)item).name + "'. Skipping."));
				continue;
			}
			int num2 = (int)field.GetValue(component);
			Plugin.Logger.LogInfo((object)$"HealSkill: '{((UnityEngine.Object)item).name}' max health is {num2}.");
			PlayerControllerCustom component2 = ((Component)item).GetComponent<PlayerControllerCustom>();
			if ((UnityEngine.Object)(object)component2 == (UnityEngine.Object)null)
			{
				Plugin.Logger.LogWarning((object)("HealSkill: No PlayerControllerCustom component found on '" + ((UnityEngine.Object)item).name + "'. Skipping."));
				continue;
			}
			FieldInfo field2 = typeof(PlayerAvatar).GetField("deadSet", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field2 == null)
			{
				Plugin.Logger.LogWarning((object)("HealSkill: 'deadSet' field not found in PlayerAvatar for '" + ((UnityEngine.Object)item).name + "'. Skipping."));
			}
			else if (!(bool)field2.GetValue(item))
			{
				int num3 = (int)((float)effectiveBaseHeal * healingMultiplier);
				Plugin.Logger.LogInfo((object)$"HealSkill: Healing '{((UnityEngine.Object)item).name}' with {num3} health (base: {effectiveBaseHeal}, multiplier: {healingMultiplier}).");
				component2.StartOwnerHealRPC(num3);
				int num4 = (int)((float)num2 * effectiveRegenPercentage);
				Plugin.Logger.LogInfo((object)$"HealSkill: Starting health regeneration for '{((UnityEngine.Object)item).name}' with {num4} health over {effectiveRegenDuration} seconds.");
				component2.StartOwnerRPCHealthRegen(num4, effectiveRegenDuration);
			}
		}
	}

	[PunRPC]
	public void PlayHealReviveParticles_RPC(int targetViewID)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		PhotonView val = PhotonView.Find(targetViewID);
		if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
		{
			Plugin.Logger.LogWarning((object)"HealSkill: Could not find player for heal revive particles.");
			return;
		}
		GameObject gameObject = ((Component)val).gameObject;
		Vector3 position = gameObject.transform.position;
		if (Plugin.AssetManager.TryGetValue("HealReviveSkill", out var value))
		{
			Plugin.Logger.LogInfo((object)$"HealSkill: Spawning revive particle effect at {position}.");
			GameObject val2 = UnityEngine.Object.Instantiate<GameObject>(value, position, Quaternion.identity);
			Plugin.Logger.LogInfo((object)$"HealSkill: Revive particles spawned for '{((UnityEngine.Object)gameObject).name}' at {val2.transform.position}.");
			val2.transform.SetParent(gameObject.transform, true);
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)val2, 5f);
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HealSkill: HealReviveSkill particle asset not found!");
		}
	}

	[PunRPC]
	public void PlayHealReviveSFX_RPC()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (Plugin.AssetManager.TryGetValue("HealReviveSkillSFX", out var value))
		{
			AudioSource component = value.GetComponent<AudioSource>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.volume = 0.2f;
			}
			UnityEngine.Object.Instantiate<GameObject>(value, ((Component)this).transform.position, Quaternion.identity);
			Plugin.Logger.LogInfo((object)"HealSkill: Heal Revive Skill SFX played.");
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HealSkill: HealReviveSkillSFX asset not found!");
		}
	}

	[PunRPC]
	public void PlayHealSkillSFX_RPC(Vector3 position)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		PlayHealSkillSFX(position);
	}

	public void PlayHealSkillSFX(Vector3 position)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (Plugin.AssetManager.TryGetValue("HealSkillSFX", out var value))
		{
			AudioSource component = value.GetComponent<AudioSource>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.volume = 0.2f;
			}
			UnityEngine.Object.Instantiate<GameObject>(value, position, Quaternion.identity);
			ManualLogSource logger = Plugin.Logger;
			Vector3 val = position;
			logger.LogInfo((object)("HealSkill: HealSkillSFX played at position " + ((object)(Vector3)(val)).ToString()));
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HealSkill: HealSkillSFX asset not found!");
		}
	}

	[PunRPC]
	public void PlayHealSkillParticles_RPC(Vector3 position, float scaleMultiplier)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		PlayHealSkillParticles(position, scaleMultiplier);
	}

	public void PlayHealSkillParticles(Vector3 position, float scaleMultiplier)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (Plugin.AssetManager.TryGetValue("HealSkill", out var value))
		{
			GameObject val = UnityEngine.Object.Instantiate<GameObject>(value, position, Quaternion.identity);
			Transform transform = val.transform;
			transform.localScale *= scaleMultiplier;
			ManualLogSource logger = Plugin.Logger;
			Vector3 val2 = position;
			logger.LogInfo((object)("HealSkill: HealSkillParticles played at position " + ((object)(Vector3)(val2)).ToString() + " with scale multiplier " + scaleMultiplier));
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HealSkill: HealSkillParticles asset not found!");
		}
	}

	public void RenderHealUI()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		if (Plugin.AssetManager.TryGetValue("HealSkillIcon", out var value))
		{
			GameObject val = GameObject.Find("Game Hud");
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				Plugin.Logger.LogWarning((object)"HealSkill: 'Game Hud' canvas not found!");
				return;
			}
			healIconInstance = UnityEngine.Object.Instantiate<GameObject>(value, ((Component)this).transform.position, Quaternion.identity);
			healIconInstance.layer = 5;
			healIconInstance.transform.localScale = UI_SCALE;
			healIconInstance.transform.SetParent(val.transform, false);
			Rect rect = val.GetComponent<RectTransform>().rect;
			float num = ((Rect)(rect)).height / 2f;
			healIconInstance.transform.localPosition = new Vector3(0f, 40f - num, 0f);
			ManualLogSource logger = Plugin.Logger;
			Vector3 localPosition = healIconInstance.transform.localPosition;
			logger.LogInfo((object)("HealSkill: Heal UI rendered on canvas at position " + ((object)(Vector3)(localPosition)).ToString()));
			SpriteRenderer component = healIconInstance.GetComponent<SpriteRenderer>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.color = healUIColor;
				Plugin.Logger.LogInfo((object)"SpawnHolyWallUI: UI color set to wallUIColor.");
			}
			else
			{
				Plugin.Logger.LogWarning((object)"SpawnHolyWallUI: No SpriteRenderer found on Holy Wall UI prefab.");
			}
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HealSkill: HealSkillIcon asset not found!");
		}
	}

	private IEnumerator HealCooldownCount()
	{
		Plugin.Logger.LogInfo((object)"HealSkill: Heal cooldown started.");
		float currentCooldown = 0f;
		SpriteRenderer sr = healIconInstance.GetComponent<SpriteRenderer>();
		if ((UnityEngine.Object)(object)sr == (UnityEngine.Object)null)
		{
			Plugin.Logger.LogWarning((object)"HealSkill: No SpriteRenderer found on healIconInstance.");
			yield break;
		}
		sr.color = healUICooldownStartColor;
		Plugin.Logger.LogInfo((object)"HealSkill: Initial UI color set.");
		while (currentCooldown < cooldownDuration)
		{
			float lerpRatio = currentCooldown / cooldownDuration;
			sr.color = Color.Lerp(healUICooldownStartColor, healUICooldownEndColor, lerpRatio);
			currentCooldown += Time.deltaTime;
			yield return null;
		}
		sr.color = healUIColor;
		isOnCooldown = false;
		Plugin.Logger.LogInfo((object)"HealSkill: Cooldown finished. UI color set to base color.");
	}
}
