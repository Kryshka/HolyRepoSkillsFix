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

public class HolyAuraSkill : MonoBehaviour
{
	public float sprintBoostMultiplier = 1.4f;

	public int auraFinalScaleValue = 8;

	public float auraExpansionTime = 0.3f;

	public float sprintBoostDuration = 4f;

	public float staminaRegenDuration = 4f;

	public float staminaRegenPercentage = 0.02f;

	public float auraCooldownDuration = 90f;

	public float baseAuraRange = 6.6f;

	public GameObject auraIconInstance;

	public Color auraUICooldownStartColor = new Color(1f, 1f, 1f, 0f);

	public Color auraUICooldownEndColor = Color.white;

	public Color auraUIColor = new Color(0f, 1f, 0f, 1f);

	private PhotonView pv;

	private const string CANVAS_NAME = "Game Hud";

	private const int UI_LAYER = 5;

	private static readonly Vector3 UI_SCALE = new Vector3(13f, 13f, 13f);

	private const float UI_VERTICAL_OFFSET = 40f;

	private static readonly Vector3 BUFF_PARTICLES_OFFSET = new Vector3(0f, 1f, 0f);

	private const float SOUND_VOLUME = 0.2f;

	private bool isAuraOnCooldown;

	private float currentAuraCooldown;

	private bool hasUIBeenSetup;

	private void Start()
	{
		Plugin.Logger.LogInfo((object)"HolyAuraSkill Start: Initializing skill.");
		isAuraOnCooldown = false;
		pv = ((Component)this).GetComponent<PhotonView>();
		if (Plugin.Instance.isInDebugMode)
		{
			auraCooldownDuration = 1f;
		}
		Plugin.Logger.LogInfo((object)"HolyAuraSkill Start: Initialization complete.");
	}

	private void Update()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (ShouldProcessInput() && JPSkill_GlobalManager.Instance.selectedSkill == SelectableSkills.HolyAura)
		{
			SetupUIIfNeeded();
			KeyboardShortcut value = Plugin.ActivateSkillHotkey.Value;
			if (Input.GetKeyDown(((KeyboardShortcut)(value)).MainKey) && !isAuraOnCooldown)
			{
				ActivateAuraSkill();
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
		if (JPSkill_GlobalManager.Instance.selectedSkill == SelectableSkills.HolyAura && !hasUIBeenSetup)
		{
			Plugin.Logger.LogInfo((object)"HolyAuraSkill Update: HolyAura skill selected; setting up UI.");
			SpawnAuraUI();
		}
	}

	private void ActivateAuraSkill()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		Plugin.Logger.LogInfo((object)"HolyAuraSkill Update: R key pressed, activating Holy Aura skill.");
		isAuraOnCooldown = true;
		pv.RPC("ActivateAuraSFX_RPC", (RpcTarget)0, new object[1] { ((Component)this).transform.position });
		((MonoBehaviour)this).StartCoroutine(AuraCooldownCount());
		Utility.TriggerWarCry(Plugin.Instance.enableWarCriesConfig, Plugin.Instance.holyWarCriesConfig, "HolyAura", new Color(1f, 0.85f, 0.45f, 1f), ChatManager.instance);
		float num = JPSkill_GlobalManager.Instance.savedExtractionHaul;
		float num2 = ((num >= 500000f) ? 2f : 1f);
		pv.RPC("ExpandAura_RPC", (RpcTarget)0, new object[2]
		{
			((Component)this).transform.position,
			num2
		});
		Plugin.Logger.LogInfo((object)("HolyAuraSkill Update: ExpandAura_RPC called with scale multiplier " + num2 + "."));
		ApplyBuffsToPlayers();
	}

	private void ApplyBuffsToPlayers()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		float num = JPSkill_GlobalManager.Instance.savedExtractionHaul;
		float num2 = baseAuraRange;
		if (num >= 500000f)
		{
			num2 *= 2f;
		}
		int num3 = 0;
		List<PlayerAvatar> list = SemiFunc.PlayerGetAll();
		foreach (PlayerAvatar item in list)
		{
			if (!((UnityEngine.Object)(object)item == (UnityEngine.Object)null) && !((UnityEngine.Object)(object)((Component)item).gameObject == (UnityEngine.Object)(object)((Component)this).gameObject) && Vector3.Distance(((Component)this).transform.position, ((Component)item).transform.position) <= num2)
			{
				FieldInfo field = typeof(PlayerAvatar).GetField("deadSet", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && !(bool)field.GetValue(item))
				{
					num3++;
				}
			}
		}
		float num4 = ((num >= 1000000f) ? 0.04f : 0.02f);
		float num5 = num4 + (float)num3 * 0.02f;
		float duration = ((num >= 1500000f) ? 8f : sprintBoostDuration);
		float duration2 = ((num >= 2000000f) ? 8f : staminaRegenDuration);
		Plugin.Logger.LogInfo((object)$"HolyAuraSkill: {num3} additional players in range; effective regen percentage is {num5 * 100f}%.");
		foreach (PlayerAvatar item2 in list)
		{
			if ((UnityEngine.Object)(object)item2 == (UnityEngine.Object)null)
			{
				Plugin.Logger.LogWarning((object)"HolyAuraSkill: Encountered null player avatar. Skipping.");
				continue;
			}
			float num6 = Vector3.Distance(((Component)this).transform.position, ((Component)item2).transform.position);
			if (num6 > num2)
			{
				Plugin.Logger.LogInfo((object)$"HolyAuraSkill: '{((UnityEngine.Object)item2).name}' is outside the effective aura range ({num2}). Skipping.");
				continue;
			}
			Plugin.Logger.LogInfo((object)("HolyAuraSkill: Applying buffs to '" + ((UnityEngine.Object)item2).name + "'."));
			PlayerControllerCustom component = ((Component)item2).GetComponent<PlayerControllerCustom>();
			int viewID = ((Component)item2).GetComponent<PhotonView>().ViewID;
			component.StartOwnerRPCSprintBoost(sprintBoostMultiplier, duration);
			component.StartOwnerRPCRegenStamina(PlayerController.instance.EnergyStart * num5, duration2);
		}
	}

	public void SpawnAuraUI()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		Plugin.Logger.LogInfo((object)"SpawnAuraUI: Setting up Holy Aura UI.");
		hasUIBeenSetup = true;
		if (Plugin.AssetManager.TryGetValue("HolyAuraIcon", out var value))
		{
			GameObject val = GameObject.Find("Game Hud");
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				Plugin.Logger.LogWarning((object)"SpawnAuraUI: 'Game Hud' canvas not found!");
				return;
			}
			auraIconInstance = UnityEngine.Object.Instantiate<GameObject>(value, ((Component)this).transform.position, Quaternion.identity);
			auraIconInstance.layer = 5;
			auraIconInstance.transform.localScale = UI_SCALE;
			auraIconInstance.transform.SetParent(val.transform, false);
			Rect rect = val.GetComponent<RectTransform>().rect;
			float num = ((Rect)(rect)).height / 2f;
			auraIconInstance.transform.localPosition = new Vector3(0f, 40f - num, 0f);
			ManualLogSource logger = Plugin.Logger;
			Vector3 localPosition = auraIconInstance.transform.localPosition;
			logger.LogInfo((object)("SpawnAuraUI: Holy Aura UI added to canvas at position: " + ((object)(Vector3)(localPosition)).ToString()));
			if (!isAuraOnCooldown)
			{
				SpriteRenderer component = auraIconInstance.GetComponent<SpriteRenderer>();
				if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
				{
					component.color = auraUIColor;
					Plugin.Logger.LogInfo((object)"SpawnAuraUI: UI color set to auraUIColor.");
				}
				else
				{
					Plugin.Logger.LogWarning((object)"SpawnAuraUI: No SpriteRenderer found on Holy Aura UI prefab.");
				}
			}
		}
		else
		{
			Plugin.Logger.LogWarning((object)"SpawnAuraUI: Holy Aura icon asset not found!");
		}
	}

	private IEnumerator AuraCooldownCount()
	{
		Plugin.Logger.LogInfo((object)"AuraCooldownCount: Cooldown started.");
		currentAuraCooldown = 0f;
		SpriteRenderer auraSprite = auraIconInstance.GetComponent<SpriteRenderer>();
		auraSprite.color = Color.white;
		while (currentAuraCooldown < auraCooldownDuration)
		{
			Color lerpedColor = Color.Lerp(auraUICooldownStartColor, auraUICooldownEndColor, currentAuraCooldown / auraCooldownDuration);
			auraSprite.color = lerpedColor;
			currentAuraCooldown += Time.deltaTime;
			yield return null;
		}
		auraSprite.color = auraUIColor;
		isAuraOnCooldown = false;
		Plugin.Logger.LogInfo((object)"AuraCooldownCount: Cooldown finished. UI color reset.");
	}

	[PunRPC]
	private void ActivateAuraSFX_RPC(Vector3 position)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		ManualLogSource logger = Plugin.Logger;
		Vector3 val = position;
		logger.LogInfo((object)("ActivateAuraSFX_RPC: Called at position " + ((object)(Vector3)(val)).ToString()));
		ActivateAuraSFX(position);
	}

	private void ActivateAuraSFX(Vector3 position)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (Plugin.AssetManager.TryGetValue("HolyAuraSFX", out var value))
		{
			AudioSource component = value.GetComponent<AudioSource>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.volume = 0.2f;
			}
			UnityEngine.Object.Instantiate<GameObject>(value, position, Quaternion.identity);
			ManualLogSource logger = Plugin.Logger;
			Vector3 val = position;
			logger.LogInfo((object)("ActivateAuraSFX: Sound effect instantiated at " + ((object)(Vector3)(val)).ToString()));
		}
		else
		{
			Plugin.Logger.LogWarning((object)"ActivateAuraSFX: Holy Aura SFX asset not found!");
		}
	}

	[PunRPC]
	public void ExpandAura_RPC(Vector3 position, float scaleMultiplier)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		ManualLogSource logger = Plugin.Logger;
		Vector3 val = position;
		logger.LogInfo((object)("ExpandAura_RPC: Spawning Holy Aura effect at " + ((object)(Vector3)(val)).ToString() + " with scale multiplier " + scaleMultiplier));
		if (Plugin.AssetManager.TryGetValue("HolyAura", out var value))
		{
			((MonoBehaviour)this).StartCoroutine(ExpandAuraEffect(value, position, scaleMultiplier));
		}
		else
		{
			Plugin.Logger.LogWarning((object)"ExpandAura_RPC: Holy Aura asset not found!");
		}
	}

	private IEnumerator ExpandAuraEffect(GameObject auraPrefab, Vector3 position, float scaleMultiplier)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Plugin.Logger.LogInfo((object)"ExpandAuraEffect: Instantiating Holy Aura effect.");
		GameObject spawnedAura = UnityEngine.Object.Instantiate<GameObject>(auraPrefab, position, Quaternion.identity);
		float elapsed = 0f;
		Vector3 initialScale = Vector3.zero;
		Vector3 targetScale = new Vector3((float)auraFinalScaleValue, (float)auraFinalScaleValue, (float)auraFinalScaleValue) * scaleMultiplier;
		while (elapsed < auraExpansionTime)
		{
			spawnedAura.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / auraExpansionTime);
			elapsed += Time.deltaTime;
			yield return null;
		}
		spawnedAura.transform.localScale = targetScale;
		ManualLogSource logger = Plugin.Logger;
		Vector3 val = targetScale;
		logger.LogInfo((object)("ExpandAuraEffect: Final scale set to " + ((object)(Vector3)(val)).ToString()));
		ParticleSystem ps = spawnedAura.GetComponent<ParticleSystem>();
		float num;
		if (!((UnityEngine.Object)(object)ps != (UnityEngine.Object)null))
		{
			num = 1.5f;
		}
		else
		{
			ParticleSystem.MainModule main = ps.main;
			num = ((ParticleSystem.MainModule)(main)).duration + 1.5f;
		}
		float effectDuration = num;
		Plugin.Logger.LogInfo((object)("ExpandAuraEffect: Waiting " + effectDuration + " seconds before destroying the aura effect."));
		yield return (object)new WaitForSeconds(effectDuration);
		Plugin.Logger.LogInfo((object)"ExpandAuraEffect: Destroying Holy Aura effect.");
		if ((UnityEngine.Object)(object)spawnedAura != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)spawnedAura);
		}
	}

	[PunRPC]
	public void SpawnAuraBuffParticles_RPC(int avatarViewID)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		PhotonView val = PhotonView.Find(avatarViewID);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			GameObject gameObject = ((Component)val).gameObject;
			if (Plugin.AssetManager.TryGetValue("HolyAuraBuff", out var value))
			{
				Vector3 val2 = gameObject.transform.position + BUFF_PARTICLES_OFFSET;
				Quaternion identity = Quaternion.identity;
				UnityEngine.Object.Instantiate<GameObject>(value, val2, identity);
				GameObject val3 = UnityEngine.Object.Instantiate<GameObject>(value, val2, identity, gameObject.transform);
				Plugin.Logger.LogInfo((object)$"SpawnAuraBuffParticles_RPC: Buff particles spawned and attached to '{((UnityEngine.Object)gameObject).name}' (ViewID: {avatarViewID}).");
			}
			else
			{
				Plugin.Logger.LogWarning((object)"SpawnAuraBuffParticles_RPC: Buff particles asset not found!");
			}
		}
		else
		{
			Plugin.Logger.LogWarning((object)("SpawnAuraBuffParticles_RPC: Avatar with ViewID " + avatarViewID + " not found."));
		}
	}
}
