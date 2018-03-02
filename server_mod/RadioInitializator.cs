using System;
using System.Collections.Generic;
using Dissonance;
using Dissonance.Audio.Playback;
using Dissonance.Integrations.UNet_HLAPI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public partial class RadioInitializator : NetworkBehaviour
{
	public void LateUpdate()
	{
		if (base.isLocalPlayer && !base.isServer)
		{
			foreach (GameObject gameObject in this.pm.players)
			{
				if (gameObject != base.gameObject)
				{
					Radio component = gameObject.GetComponent<Radio>();
					component.SetRelationship();
					string playerId = gameObject.GetComponent<HlapiPlayer>().PlayerId;
					if (!(component.mySource == null))
					{
						VoicePlayback component2 = component.mySource.GetComponent<VoicePlayback>();
						bool flag = component.mySource.spatialBlend == 0f && component2.Priority != ChannelPriority.None && component.ShouldBeVisible(base.gameObject);
						if (RadioInitializator.names.Contains(playerId))
						{
							int index = RadioInitializator.names.IndexOf(playerId);
							if (!flag)
							{
								UnityEngine.Object.Destroy(RadioInitializator.spawns[index]);
								RadioInitializator.spawns.RemoveAt(index);
								RadioInitializator.names.RemoveAt(index);
								return;
							}
							RadioInitializator.spawns[index].GetComponent<Image>().color = this.color_in.Evaluate(component2.Amplitude * 3f);
							RadioInitializator.spawns[index].GetComponent<Outline>().effectColor = this.color_out.Evaluate(component2.Amplitude * 3f);
						}
						else if (flag)
						{
							GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.prefab, this.parent);
							gameObject2.transform.localScale = Vector3.one;
							gameObject2.GetComponentInChildren<Text>().text = component.GetComponent<NicknameSync>().myNick;
							RadioInitializator.spawns.Add(gameObject2);
							RadioInitializator.names.Add(playerId);
							return;
						}
					}
				}
			}
		}
	}
}
