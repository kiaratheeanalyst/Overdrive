using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNetworking : MonoBehaviour
{

	public MonoBehaviour[] hahaScriptsGoBRRR;

	private PhotonView photonView;

	void Start()
	{
		photonView = GetComponent<PhotonView>();
		if(!photonView.IsMine)
		{
			foreach (var script in hahaScriptsGoBRRR)
			{
				script.enabled=false;
			}
		}
	}

}
