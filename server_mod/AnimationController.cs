using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public partial class AnimationController : NetworkBehaviour
{
	public void DoAnimation(string trigger)
	{
		if (!base.isLocalPlayer && this.handAnimator != null)
		{
			this.handAnimator.SetTrigger(trigger);
		}
	}
}
