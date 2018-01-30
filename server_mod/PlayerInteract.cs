using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x020000A0 RID: 160
public partial class PlayerInteract : NetworkBehaviour
{
	// Token: 0x060003DA RID: 986 RVA: 0x00004A0D File Offset: 0x00002C0D
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
