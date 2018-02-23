using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// Token: 0x020000D7 RID: 215
public partial class Searching : NetworkBehaviour
{
	// Token: 0x060005E6 RID: 1510 RVA: 0x000236B0 File Offset: 0x000218B0
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
