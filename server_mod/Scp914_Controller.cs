using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x020000C9 RID: 201
public partial class Scp914_Controller : NetworkBehaviour
{
	// Token: 0x0600058B RID: 1419
	[Command(channel = 2)]
	public void CmdSetupPickup(string label, int result, Vector3 pos)
	{
		GameObject gameObject = GameObject.Find(label);
		if (gameObject != null)
		{
			Pickup pickup = gameObject.GetComponent<Pickup>();
			if (pickup != null)
			{
				pickup.SetDurability(this.avItems[result].durability);
				pickup.SetID(result);
				pickup.SetPosition(pos);
			}
		}
	}
}
