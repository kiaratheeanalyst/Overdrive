using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{

    	void Start()
    	{
                PhotonNetwork.ConnectUsingSettings();
    	}


	public override void OnConnectedToMaster ()
	{
		PhotonNetwork.JoinLobby();
	}


	public override void OnJoinedLobby ()
	{
		PhotonNetwork.JoinOrCreateRoom("Room",new RoomOptions {MaxPlayers=4}, TypedLobby.Default);
	}

	public override void OnJoinedRoom ()
	{
		PhotonNetwork.Instantiate("PlayerForPhoton", new Vector2(Random.Range(-10f, 12f),transform.position.y), Quaternion.identity);
	}
}
