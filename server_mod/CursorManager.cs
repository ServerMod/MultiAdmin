using System;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CursorManager : MonoBehaviour
{
	public void LateUpdate()
	{
		bool visible = CursorManager.eqOpen | CursorManager.pauseOpen | CursorManager.isServerOnly | CursorManager.consoleOpen | CursorManager.is079 | CursorManager.scp106 | CursorManager.roundStarted | CursorManager.raOp;

        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
		{
			Cursor.lockState = CursorLockMode.None;
		}
        else
        {
            Cursor.lockState = ((!visible) ? CursorLockMode.Locked : CursorLockMode.None);
        }

        Cursor.visible = visible;
	}
}
