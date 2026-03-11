using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace JP_RepoHolySkills;

public class TimedDestroyer : MonoBehaviourPun
{
	public float lifeTime = 10f;

	private void Start()
	{
		((MonoBehaviour)this).StartCoroutine(SelfDestruct());
	}

	private IEnumerator SelfDestruct()
	{
		yield return (object)new WaitForSeconds(lifeTime);
		if (((MonoBehaviourPun)this).photonView.IsMine)
		{
			PhotonNetwork.Destroy(((Component)this).gameObject);
		}
	}
}
