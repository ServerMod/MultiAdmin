using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public partial class Inventory : NetworkBehaviour
{
	[Command(channel = 2)]
	public void CmdSetPickup(int dropedItemID, float dur, Vector3 pos, Quaternion camRot, Quaternion myRot)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.pickupPrefab);
		NetworkServer.Spawn(gameObject);
		gameObject.GetComponent<Pickup>().SetDurability(dur);
		gameObject.GetComponent<Pickup>().SetID(dropedItemID);
		gameObject.GetComponent<Pickup>().SetPosition(((this.ccm.curClass != 2) ? pos : this.ccm.deathPosition) + Vector3.up * 0.9f);
		gameObject.GetComponent<Pickup>().SetRotation(new Vector3(camRot.eulerAngles.x, myRot.eulerAngles.y, 0f));
		gameObject.GetComponent<Pickup>().SetName(string.Concat(new object[]
		{
			"PICKUP#",
			dropedItemID,
			":",
			UnityEngine.Random.Range(0f, 1E+10f).ToString("0000000000")
		}));
		this.smNetFreeSlots++;
	}

	public int smNetFreeSlots;
}
