using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace JP_RepoHolySkills;

public class CombinedPrefabPool : IPunPrefabPool
{
	private IPunPrefabPool basePool;

	private Dictionary<string, GameObject> moddedPrefabs = new Dictionary<string, GameObject>();

	public CombinedPrefabPool(IPunPrefabPool basePool)
	{
		this.basePool = basePool;
	}

	public void AddModdedPrefab(string key, GameObject prefab)
	{
		if (!moddedPrefabs.ContainsKey(key))
		{
			moddedPrefabs.Add(key, prefab);
		}
	}

	public Dictionary<string, GameObject> GetModdedPrefabs()
	{
		return moddedPrefabs;
	}

	public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
	{
		Plugin.Logger.LogInfo((object)$"CombinedPrefabPool: Instantiate called for '{prefabId}'.");
		if (moddedPrefabs.TryGetValue(prefabId, out var value))
		{
			Plugin.Logger.LogInfo((object)$"CombinedPrefabPool: Found modded prefab for '{prefabId}'. Instantiating...");
			GameObject val = UnityEngine.Object.Instantiate<GameObject>(value, position, rotation);
			val.SetActive(false);
			if ((UnityEngine.Object)(object)val.GetComponent<PhotonView>() == (UnityEngine.Object)null)
			{
				Plugin.Logger.LogInfo((object)$"CombinedPrefabPool: Adding missing PhotonView to '{prefabId}'.");
				PhotonView val2 = val.AddComponent<PhotonView>();
			}
			return val;
		}
		Plugin.Logger.LogInfo((object)$"CombinedPrefabPool: '{prefabId}' not found in modded prefabs. Delegating to base pool.");
		return basePool.Instantiate(prefabId, position, rotation);
	}

	public void Destroy(GameObject gameObject)
	{
		basePool.Destroy(gameObject);
	}
}
