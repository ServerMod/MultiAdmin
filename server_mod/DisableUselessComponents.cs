using System;
using UnityEngine;
using UnityEngine.Networking;

public partial class DisableUselessComponents : NetworkBehaviour
{
	public void Start()
	{
		if (!base.isLocalPlayer)
		{
			if (base.GetComponent<CharacterController>() != null)
			{
				UnityEngine.Object.DestroyImmediate(base.GetComponent<FirstPersonController>());
			}
			Behaviour[] array = this.uselessComponents;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			if (base.GetComponent<CharacterController>() != null)
			{
				UnityEngine.Object.Destroy(base.GetComponent<CharacterController>());
				return;
			}
		}
		else
		{
			PlayerManager.localPlayer = base.gameObject;
			this.CallCmdSetName((!base.isServer) ? "Player" : "Host", ServerStatic.isDedicated);
			if (base.GetComponent<FirstPersonController>() != null)
			{
				base.GetComponent<FirstPersonController>().enabled = false;
			}
		}
	}
}
