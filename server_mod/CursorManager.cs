using System;
using UnityEngine;

// Token: 0x0200006E RID: 110
public partial class CursorManager : MonoBehaviour
{
	// Token: 0x06000219 RID: 537 RVA: 0x00012850 File Offset: 0x00010A50
	public void LateUpdate()
	{
		bool flag = CursorManager.eqOpen | CursorManager.pauseOpen | CursorManager.isServerOnly | CursorManager.consoleOpen | CursorManager.is079 | CursorManager.scp106 | CursorManager.roundStarted | CursorManager.raOp;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = flag;
	}
}
