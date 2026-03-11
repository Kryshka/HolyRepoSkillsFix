using System.Collections;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using JP_RepoHolySkills.GlobalMananger;
using JP_RepoHolySkills.SkillSelector;
using Photon.Pun;
using UnityEngine;

namespace JP_RepoHolySkills.Skills;

public class HolyWallSkill : MonoBehaviour
{
	public PhotonView photonView;

	public float skillDuration = 3f;

	public float cooldownDuration = 180f;

	public GameObject holyWallIconInstance;

	public Color wallUICooldownStartColor = new Color(1f, 1f, 1f, 0f);

	public Color wallUICooldownEndColor = Color.white;

	public Color wallUIColor = new Color(0f, 1f, 0f, 1f);

	private const string CANVAS_NAME = "Game Hud";

	private const int UI_LAYER = 5;

	private static readonly Vector3 UI_SCALE = new Vector3(13f, 13f, 13f);

	private const float UI_VERTICAL_OFFSET = 40f;

	private bool isWallOnCooldown;

	private float currentWallCooldown;

	private bool wallUISetup;

	private const float GRENADE_SPAWN_CHANCE = 0.2f;

	private const float MINE_SPAWN_CHANCE = 0.2f;

	private const float FORWARD_OFFSET = 2f;

	private const float UP_OFFSET = 1f;

	private const float ADDITIONAL_SPAWN_DISTANCE = 1f;

	private const float EXTRACTION_THRESHOLD_SCALE = 500000f;

	private const float EXTRACTION_THRESHOLD_DURATION = 1000000f;

	private const float EXTRACTION_THRESHOLD_GRENADE = 1500000f;

	private const float EXTRACTION_THRESHOLD_MINE = 2000000f;

	private void Start()
	{
		Plugin.Logger.LogInfo((object)"HolyWallSkill: Initializing skill.");
		photonView = ((Component)this).GetComponent<PhotonView>();
		isWallOnCooldown = false;
		wallUISetup = false;
		if (Plugin.Instance.isInDebugMode)
		{
			cooldownDuration = 1f;
		}
		Plugin.Logger.LogInfo((object)"HolyWallSkill: Initialization complete.");
	}

	private void Update()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!CanActivateSkill() || JPSkill_GlobalManager.Instance.selectedSkill != SelectableSkills.HolyWall)
		{
			return;
		}
		SetupUIIfNeeded();
		if (!isWallOnCooldown)
		{
			KeyboardShortcut value = Plugin.ActivateSkillHotkey.Value;
			if (Input.GetKeyDown(((KeyboardShortcut)(value)).MainKey))
			{
				ActivateHolyWallSkill();
			}
		}
	}

	private bool CanActivateSkill()
	{
		if (!SemiFunc.RunIsLevel() || !photonView.IsMine)
		{
			return false;
		}
		FieldInfo field = typeof(ChatManager).GetField("chatActive", BindingFlags.Instance | BindingFlags.NonPublic);
		bool flag = (bool)field.GetValue(ChatManager.instance);
		return !flag;
	}

	private void SetupUIIfNeeded()
	{
		if (JPSkill_GlobalManager.Instance.selectedSkill == SelectableSkills.HolyWall && !wallUISetup)
		{
			SpawnHolyWallUI();
		}
	}

	public void SpawnHolyWallUI()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		Plugin.Logger.LogInfo((object)"SpawnHolyWallUI: Setting up Holy Wall UI.");
		wallUISetup = true;
		if (Plugin.AssetManager.TryGetValue("HolyWallIcon", out var value))
		{
			GameObject val = GameObject.Find("Game Hud");
			if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null)
			{
				Plugin.Logger.LogWarning((object)"SpawnHolyWallUI: 'Game Hud' canvas not found!");
				return;
			}
			holyWallIconInstance = UnityEngine.Object.Instantiate<GameObject>(value, ((Component)this).transform.position, Quaternion.identity);
			holyWallIconInstance.layer = 5;
			holyWallIconInstance.transform.localScale = UI_SCALE;
			holyWallIconInstance.transform.SetParent(val.transform, false);
			Rect rect = val.GetComponent<RectTransform>().rect;
			float num = ((Rect)(rect)).height / 2f;
			holyWallIconInstance.transform.localPosition = new Vector3(0f, 40f - num, 0f);
			ManualLogSource logger = Plugin.Logger;
			Vector3 localPosition = holyWallIconInstance.transform.localPosition;
			logger.LogInfo((object)("SpawnHolyWallUI: Holy Wall UI added at position: " + ((object)(Vector3)(localPosition)).ToString()));
			SpriteRenderer component = holyWallIconInstance.GetComponent<SpriteRenderer>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.color = wallUIColor;
				Plugin.Logger.LogInfo((object)"SpawnHolyWallUI: UI color set to wallUIColor.");
			}
			else
			{
				Plugin.Logger.LogWarning((object)"SpawnHolyWallUI: No SpriteRenderer found on Holy Wall UI prefab.");
			}
		}
		else
		{
			Plugin.Logger.LogWarning((object)"SpawnHolyWallUI: Holy Wall icon asset not found!");
		}
	}

	private void ActivateHolyWallSkill()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		Plugin.Logger.LogInfo((object)"HolyWallSkill: F key pressed, activating Holy Wall skill.");
		if ((UnityEngine.Object)(object)JPSkill_GlobalManager.Instance != (UnityEngine.Object)null)
		{
			JPSkill_GlobalManager.Instance.EnsurePrefabPool();
		}
		isWallOnCooldown = true;
		((MonoBehaviour)this).StartCoroutine(WallCooldownCount());
		PlayHolyWallSFX();
		if (!Plugin.AssetManager.TryGetValue("HolyWall", out var _))
		{
			Plugin.Logger.LogWarning((object)"HolyWallSkill: Holy Wall asset not found!");
			return;
		}
		Plugin.Logger.LogInfo((object)"HolyWallSkill: Holy Wall asset found. Instantiating...");
		Vector3 val = ((Component)this).transform.position + ((Component)this).transform.forward * 2f + Vector3.up * 1f;
		Quaternion rotation = ((Component)this).transform.rotation;
		GameObject val2 = PhotonNetwork.Instantiate("HolyWall", val, rotation, (byte)0, (object[])null);
		AdjustHolyWallScale(val2);
		float effectiveDuration = GetEffectiveDuration();
		TimedDestroyer timedDestroyer = val2.AddComponent<TimedDestroyer>();
		timedDestroyer.lifeTime = effectiveDuration;
		TryRequestGrenadeSpawn();
		TryRequestMineSpawn();
		Utility.TriggerWarCry(Plugin.Instance.enableWarCriesConfig, Plugin.Instance.holyWallWarCriesConfig, "HolyWallSkill", new Color(0.5f, 0.7f, 1f, 1f), ChatManager.instance);
	}

	private IEnumerator WallCooldownCount()
	{
		Plugin.Logger.LogInfo((object)"WallCooldownCount: Cooldown started.");
		currentWallCooldown = 0f;
		if ((UnityEngine.Object)(object)holyWallIconInstance == (UnityEngine.Object)null)
		{
			Plugin.Logger.LogWarning((object)"WallCooldownCount: holyWallIconInstance is null. Exiting cooldown coroutine.");
			yield break;
		}
		SpriteRenderer wallSprite = holyWallIconInstance.GetComponent<SpriteRenderer>();
		if ((UnityEngine.Object)(object)wallSprite == (UnityEngine.Object)null)
		{
			Plugin.Logger.LogWarning((object)"WallCooldownCount: SpriteRenderer not found on holyWallIconInstance. Exiting cooldown coroutine.");
			yield break;
		}
		wallSprite.color = Color.white;
		while (currentWallCooldown < cooldownDuration)
		{
			Color lerpedColor = Color.Lerp(wallUICooldownStartColor, wallUICooldownEndColor, currentWallCooldown / cooldownDuration);
			wallSprite.color = lerpedColor;
			currentWallCooldown += Time.deltaTime;
			yield return null;
		}
		wallSprite.color = wallUIColor;
		isWallOnCooldown = false;
		Plugin.Logger.LogInfo((object)"WallCooldownCount: Cooldown finished. UI color reset.");
	}

	private void PlayHolyWallSFX()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (Plugin.AssetManager.TryGetValue("HolyWallSFX", out var value))
		{
			AudioSource component = value.GetComponent<AudioSource>();
			if ((UnityEngine.Object)(object)component != (UnityEngine.Object)null)
			{
				component.volume = 0.2f;
			}
			UnityEngine.Object.Instantiate<GameObject>(value, ((Component)this).transform.position, Quaternion.identity);
			Plugin.Logger.LogInfo((object)"HolyWallSkill: Holy Wall SFX played.");
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HolyWallSkill: HolyWallSFX asset not found!");
		}
	}

	private void AdjustHolyWallScale(GameObject spawnedWall)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		float scaleMultiplier = GetScaleMultiplier();
		Vector3 localScale = spawnedWall.transform.localScale;
		localScale.x *= scaleMultiplier;
		localScale.y *= scaleMultiplier;
		int viewID = spawnedWall.GetComponent<PhotonView>().ViewID;
		photonView.RPC("SetHolyWallScale_RPC", (RpcTarget)0, new object[2] { viewID, localScale });
		Plugin.Logger.LogInfo((object)$"HolyWallSkill: Holy Wall instantiated at {spawnedWall.transform.position} with scale multiplier: {scaleMultiplier}.");
	}

	private float GetScaleMultiplier()
	{
		float playerExtractionHaul = GetPlayerExtractionHaul();
		return (playerExtractionHaul >= 500000f) ? 2f : 1.5f;
	}

	private float GetPlayerExtractionHaul()
	{
		return JPSkill_GlobalManager.Instance.savedExtractionHaul;
	}

	private float GetEffectiveDuration()
	{
		float playerExtractionHaul = GetPlayerExtractionHaul();
		return (playerExtractionHaul >= 1000000f) ? 6f : skillDuration;
	}

	private void TryRequestGrenadeSpawn()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		float playerExtractionHaul = GetPlayerExtractionHaul();
		if (playerExtractionHaul >= 1500000f)
		{
			if (Random.value <= 0.2f)
			{
				Vector3 val = ((Component)this).transform.position + ((Component)this).transform.forward * 1f + Vector3.up * 1f;
				Quaternion rotation = ((Component)this).transform.rotation;
				photonView.RPC("RequestGrenadeSpawn", (RpcTarget)2, new object[2] { val, rotation });
				Plugin.Logger.LogInfo((object)"HolyWallSkill: Grenade spawn requested.");
			}
			else
			{
				Plugin.Logger.LogInfo((object)"HolyWallSkill: Grenade spawn chance failed.");
			}
		}
		else
		{
			Plugin.Logger.LogInfo((object)"HolyWallSkill: Extraction haul below threshold for grenade spawn.");
		}
	}

	private void TryRequestMineSpawn()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		float playerExtractionHaul = GetPlayerExtractionHaul();
		if (playerExtractionHaul >= 2000000f)
		{
			if (Random.value <= 0.2f)
			{
				Vector3 val = ((Component)this).transform.position + ((Component)this).transform.forward * 1f + Vector3.up * 1f;
				Quaternion rotation = ((Component)this).transform.rotation;
				photonView.RPC("RequestMineSpawn", (RpcTarget)2, new object[2] { val, rotation });
				Plugin.Logger.LogInfo((object)"HolyWallSkill: Mine spawn requested.");
			}
			else
			{
				Plugin.Logger.LogInfo((object)"HolyWallSkill: Mine spawn chance failed.");
			}
		}
		else
		{
			Plugin.Logger.LogInfo((object)"HolyWallSkill: Extraction haul below threshold for mine spawn.");
		}
	}

	[PunRPC]
	public void RequestGrenadeSpawn(Vector3 spawnPos, Quaternion spawnRot)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (PhotonNetwork.IsMasterClient)
		{
			GameObject val = PhotonNetwork.Instantiate("Items/Item Grenade Stun", spawnPos, spawnRot, (byte)0, (object[])null);
			Plugin.Logger.LogInfo((object)$"HolyWallSkill: Grenade spawned at {spawnPos} with rotation {((Quaternion)(spawnRot)).eulerAngles}");
		}
	}

	[PunRPC]
	public void RequestMineSpawn(Vector3 spawnPos, Quaternion spawnRot)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (PhotonNetwork.IsMasterClient)
		{
			GameObject val = PhotonNetwork.Instantiate("Items/Item Mine Stun", spawnPos, spawnRot, (byte)0, (object[])null);
			Plugin.Logger.LogInfo((object)$"HolyWallSkill: Mine spawned at {spawnPos} with rotation {((Quaternion)(spawnRot)).eulerAngles}");
		}
	}

	[PunRPC]
	private void SetHolyWallScale_RPC(int spawnedWallViewID, Vector3 newScale)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		PhotonView val = PhotonView.Find(spawnedWallViewID);
		if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
		{
			GameObject gameObject = ((Component)val).gameObject;
			gameObject.transform.localScale = newScale;
			Plugin.Logger.LogInfo((object)"HolyWallSkill: Scale updated for Holy Wall.");
		}
		else
		{
			Plugin.Logger.LogWarning((object)"HolyWallSkill: Holy Wall not found for scaling update!");
		}
	}
}
