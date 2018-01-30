using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x02000062 RID: 98
public partial class AnimationController : NetworkBehaviour
{
	// Token: 0x060001B0 RID: 432
	public void DoAnimation(string trigger)
	{
		if (!base.isLocalPlayer && this.handAnimator != null)
		{
			this.handAnimator.SetTrigger(trigger);
		}
	}
}
