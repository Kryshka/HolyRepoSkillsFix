using UnityEngine;

namespace JP_RepoHolySkills.MapToolControllerCustoms;

internal class MapToolControllerCustom : MonoBehaviour
{
	private void Awake()
	{
		Plugin.Logger.LogInfo((object)"MapToolControllerCustom awake");
	}
}
