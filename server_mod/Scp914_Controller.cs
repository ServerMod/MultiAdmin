using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;


public partial class Scp914_Controller : NetworkBehaviour
{
    [Command(channel = 2)]
    public void CmdSetupPickup(string label, int result, Vector3 pos)
    {
        GameObject gameObject = GameObject.Find(label);
        if (gameObject != null)
        {
            Pickup pickup = gameObject.GetComponent<Pickup>();
            Pickup parentPickup = gameObject.GetComponentInParent<Pickup>();
            if (pickup != null && parentPickup != null)
            {
                pickup.SetDurability(this.avItems[result].durability);
                parentPickup.SetID(result);
                parentPickup.SetPosition(pos);
            }
        }
    }
}
