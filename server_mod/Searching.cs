using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public partial class Searching : NetworkBehaviour
{
	[Command(channel = 2)]
	public void CmdPickupItem(GameObject t, GameObject taker)
	{
		int id = 0;
		Pickup component = t.GetComponent<Pickup>();
		Inventory component2 = taker.GetComponent<Inventory>();
		if (component != null)
		{
			id = component.id;
			component.PickupItem();
			if (component2 != null)
			{
				component2.smNetFreeSlots--;
			}
		}
		Locker component3 = t.GetComponent<Locker>();
		if (component3 != null)
		{
			id = component3.GetItem();
			component3.SetTaken(true);
			if (component2 != null)
			{
				component2.smNetFreeSlots--;
			}
		}
		this.CallRpcPickupItem(taker, id, (!(t.GetComponent<Pickup>() == null)) ? component.durability : -1f);
	}
}
