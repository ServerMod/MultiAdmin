using System;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CursorManager : MonoBehaviour
{
	public void LateUpdate()
	{
		bool visible = CursorManager.eqOpen | CursorManager.pauseOpen | CursorManager.isServerOnly | CursorManager.consoleOpen | CursorManager.is079 | CursorManager.scp106 | CursorManager.roundStarted | CursorManager.raOp;
		Cursor.lockState = ((!visible) ? CursorLockMode.Locked : CursorLockMode.None);
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
		{
			Cursor.lockState = CursorLockMode.None;
		}
		Cursor.visible = visible;
	}
}
