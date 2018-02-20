using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000052 RID: 82
public partial class ResolutionManager : MonoBehaviour
{
	// Token: 0x0600016E RID: 366 RVA: 0x00010114 File Offset: 0x0000E314
	public static void RefreshScreen()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
		{
			return;
		}
		ResolutionManager.presets[ResolutionManager.preset].SetResolution();
		try
		{
			UnityEngine.Object.FindObjectOfType<ResolutionText>().txt.text = ResolutionManager.presets[ResolutionManager.preset].width + " × " + ResolutionManager.presets[ResolutionManager.preset].height;
		}
		catch
		{
		}
	}
}
