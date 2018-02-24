using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class ResolutionManager : MonoBehaviour
{
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
