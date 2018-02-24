using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

public partial class PlayerInteract : NetworkBehaviour
{
	[Command(channel = 4)]
	public void CmdUse294(string label)
	{
		GameObject use = GameObject.Find(label);
		if (use != null)
		{
			use.GetComponent<Scp294>().Buy();
		}
	}
}
