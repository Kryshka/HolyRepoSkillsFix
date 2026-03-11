using System.Collections;
using System.Reflection;
using JP_RepoHolySkills.Skills;
using Photon.Pun;
using UnityEngine;

namespace JP_RepoHolySkills.Player;

internal class PlayerControllerCustom : MonoBehaviour
{
	public PhotonView photonView;

	public HealSkill healSkill;

	public HolyAuraSkill holyAuraSkill;

	public HolyWallSkill holyWallSkill;

	public Coroutine boostSprintCoroutine;

	public Coroutine staminaRegenCoroutine;

	public Coroutine healthRegenCoroutine;

	public PlayerHealth playerHealth;

	private void Start()
	{
		Plugin.Logger.LogInfo((object)"PlayerControllerCustom: Start called.");
		photonView = ((Component)this).GetComponent<PhotonView>();
		playerHealth = ((Component)this).GetComponent<PlayerHealth>();
		healSkill = ((Component)this).gameObject.AddComponent<HealSkill>();
		Plugin.Logger.LogInfo((object)"PlayerControllerCustom: HealSkill component added.");
		holyAuraSkill = ((Component)this).gameObject.AddComponent<HolyAuraSkill>();
		Plugin.Logger.LogInfo((object)"PlayerControllerCustom: HolyAuraSkill component added.");
		holyWallSkill = ((Component)this).gameObject.AddComponent<HolyWallSkill>();
		Plugin.Logger.LogInfo((object)"PlayerControllerCustom: HolyWallSkill component added.");
	}

	private void Update()
	{
		if (SemiFunc.RunIsLevel() && photonView.IsMine)
		{
		}
	}

	public void StartOwnerRPCSprintBoost(float speedModifier, float duration)
	{
		Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Requesting sprint boost with modifier {speedModifier} for {duration} seconds.");
		photonView.RPC("StartSprintBoostCoroutine_RPC", photonView.Owner, new object[2] { speedModifier, duration });
	}

	[PunRPC]
	private void StartSprintBoostCoroutine_RPC(float speedModifier, float duration)
	{
		Plugin.Logger.LogInfo((object)"PlayerControllerCustom: Starting sprint boost coroutine RPC.");
		StartBoostSprintCoroutine(speedModifier, duration);
	}

	private void StartBoostSprintCoroutine(float speedModifier, float duration)
	{
		if (boostSprintCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(boostSprintCoroutine);
			Plugin.Logger.LogInfo((object)"PlayerControllerCustom: Stopped existing sprint boost coroutine.");
		}
		boostSprintCoroutine = ((MonoBehaviour)this).StartCoroutine(BoostSprint(speedModifier, duration));
	}

	private IEnumerator BoostSprint(float speedModifier, float duration)
	{
		FieldInfo playerNameField = typeof(PlayerController).GetField("playerName", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo originalSprintField = typeof(PlayerController).GetField("playerOriginalSprintSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
		FieldInfo originalMoveField = typeof(PlayerController).GetField("playerOriginalMoveSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
		string playerName = (string)playerNameField.GetValue(PlayerController.instance);
		float originalSprint = (float)originalSprintField.GetValue(PlayerController.instance);
		float originalMove = (float)originalMoveField.GetValue(PlayerController.instance);
		Plugin.Logger.LogInfo((object)$"{playerName}: Sprint boost starting. Original Walk: {PlayerController.instance.MoveSpeed}, Original Sprint: {PlayerController.instance.SprintSpeed}");
		float newWalkSpeed = PlayerController.instance.MoveSpeed * speedModifier;
		float newSprintSpeed = PlayerController.instance.SprintSpeed * speedModifier;
		Plugin.Logger.LogInfo((object)$"{playerName}: New Walk: {newWalkSpeed}, New Sprint: {newSprintSpeed}");
		PlayerController.instance.MoveSpeed = newWalkSpeed;
		PlayerController.instance.SprintSpeed = newSprintSpeed;
		yield return (object)new WaitForSeconds(duration);
		PlayerController.instance.MoveSpeed = originalMove;
		PlayerController.instance.SprintSpeed = originalSprint;
		Plugin.Logger.LogInfo((object)$"{playerName}: Sprint boost ended. Speeds reset to Walk: {PlayerController.instance.MoveSpeed}, Sprint: {PlayerController.instance.SprintSpeed}");
	}

	[PunRPC]
	private void SyncMoveSpeed(float walkSpeed, float sprintSpeed)
	{
		PlayerController.instance.MoveSpeed = walkSpeed;
		PlayerController.instance.SprintSpeed = sprintSpeed;
		Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Synchronized move speeds: Walk = {walkSpeed}, Sprint = {sprintSpeed}");
	}

	public void StartOwnerRPCRegenStamina(float regenAmount, float duration)
	{
		Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Requesting stamina regeneration: {regenAmount} per tick for {duration} seconds.");
		photonView.RPC("StartRegenStamina_RPC", photonView.Owner, new object[2] { regenAmount, duration });
	}

	[PunRPC]
	private void StartRegenStamina_RPC(float regenAmount, float duration)
	{
		Plugin.Logger.LogInfo((object)"PlayerControllerCustom: Starting stamina regen RPC.");
		StartRegenStaminaCoroutine(regenAmount, duration);
	}

	private void StartRegenStaminaCoroutine(float regenAmount, float duration)
	{
		if (staminaRegenCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(staminaRegenCoroutine);
			Plugin.Logger.LogInfo((object)"PlayerControllerCustom: Stopped existing stamina regen coroutine.");
		}
		staminaRegenCoroutine = ((MonoBehaviour)this).StartCoroutine(RegenStamina(regenAmount, duration));
	}

	private IEnumerator RegenStamina(float regenAmount, float duration)
	{
		float elapsedTime = 0f;
		float tickInterval = 1f;
		while (elapsedTime < duration)
		{
			PlayerController.instance.EnergyCurrent = Mathf.Min(PlayerController.instance.EnergyCurrent + regenAmount, PlayerController.instance.EnergyStart);
			Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Regenerating stamina. Current: {PlayerController.instance.EnergyCurrent}");
			elapsedTime += tickInterval;
			yield return (object)new WaitForSeconds(tickInterval);
		}
	}

	public void StartOwnerRPCHealthRegen(int regenAmount, float duration)
	{
		Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Requesting health regeneration: {regenAmount} per tick for {duration} seconds.");
		photonView.RPC("StartHealthRegen_RPC", photonView.Owner, new object[2] { regenAmount, duration });
	}

	[PunRPC]
	private void StartHealthRegen_RPC(int regenAmount, float duration)
	{
		Plugin.Logger.LogInfo((object)"PlayerControllerCustom: Starting health regen RPC.");
		StartHealthRegenCoroutine(regenAmount, duration);
	}

	private void StartHealthRegenCoroutine(int regenAmount, float duration)
	{
		if (healthRegenCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(healthRegenCoroutine);
			Plugin.Logger.LogInfo((object)"PlayerControllerCustom: Stopped existing health regen coroutine.");
		}
		healthRegenCoroutine = ((MonoBehaviour)this).StartCoroutine(RegenHealth(regenAmount, duration));
	}

	private IEnumerator RegenHealth(int regenAmount, float duration)
	{
		FieldInfo healthField = typeof(PlayerHealth).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic);
		int initialHealth = (int)healthField.GetValue(playerHealth);
		Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Initial health is {initialHealth}.");
		float elapsedTime = 0f;
		float tickInterval = 1f;
		while (elapsedTime < duration)
		{
			playerHealth.Heal(regenAmount, true);
			int updatedHealth = (int)healthField.GetValue(playerHealth);
			Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Health regenerated to {updatedHealth}.");
			photonView.RPC("SyncHealth", (RpcTarget)4, new object[1] { updatedHealth });
			elapsedTime += tickInterval;
			yield return (object)new WaitForSeconds(tickInterval);
		}
	}

	public void StartOwnerHealRPC(int amount)
	{
		Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Requesting heal of {amount} HP.");
		photonView.RPC("Heal_RPC", photonView.Owner, new object[1] { amount });
	}

	[PunRPC]
	private void Heal_RPC(int amount)
	{
		Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Heal_RPC received with amount {amount}.");
		Heal(amount);
	}

	public void Heal(int amount)
	{
		playerHealth.Heal(amount, true);
		FieldInfo field = typeof(PlayerHealth).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field != null)
		{
			int num = (int)field.GetValue(playerHealth);
			photonView.RPC("SyncHealth", (RpcTarget)4, new object[1] { num });
			Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Healed. Updated health is {num}.");
		}
		else
		{
			Plugin.Logger.LogWarning((object)"PlayerControllerCustom: Field 'health' not found on PlayerHealth.");
		}
	}

	[PunRPC]
	private void SyncHealth(int updatedHealth)
	{
		FieldInfo field = typeof(PlayerHealth).GetField("health", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field != null)
		{
			field.SetValue(playerHealth, updatedHealth);
			Plugin.Logger.LogInfo((object)$"PlayerControllerCustom: Synchronized health to {updatedHealth}.");
		}
		else
		{
			Plugin.Logger.LogWarning((object)"PlayerControllerCustom: Field 'health' not found on PlayerHealth in SyncHealth RPC.");
		}
	}
}
