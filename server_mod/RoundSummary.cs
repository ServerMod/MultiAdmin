using System;
using GameConsole;
using TMPro;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x020000C0 RID: 192
public class RoundSummary : NetworkBehaviour
{
	// Token: 0x060004C4 RID: 1220 RVA: 0x00005226 File Offset: 0x00003426
	private void Awake()
	{
		Radio.roundEnded = false;
	}

	// Token: 0x060004C5 RID: 1221 RVA: 0x0000522E File Offset: 0x0000342E
	private void Start()
	{
		this.pm = PlayerManager.singleton;
		this.ccm = base.GetComponent<CharacterClassManager>();
		base.InvokeRepeating("CheckForEnding", 12f, 3f);
	}

	// Token: 0x060004C6 RID: 1222 RVA: 0x0001DE18 File Offset: 0x0001C018
	private void RoundRestart()
	{
		bool flag = false;
		foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Player"))
		{
			PlayerStats component = gameObject.GetComponent<PlayerStats>();
			if (component.isLocalPlayer && component.isServer)
			{
				flag = true;
				GameConsole.Console.singleton.AddLog("The round is about to restart! Please wait..", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
				component.Roundrestart();
			}
		}
		if (!flag)
		{
			GameConsole.Console.singleton.AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
		}
	}

	// Token: 0x060004C7 RID: 1223 RVA: 0x0001DEC0 File Offset: 0x0001C0C0
	public void CheckForEnding()
	{
		if (base.isLocalPlayer && base.isServer && !this.roundHasEnded)
		{
			if (!this.ccm.roundStarted)
			{
				return;
			}
			this._ClassDs = 0;
			this._ChaosInsurgency = 0;
			this._MobileForces = 0;
			this._Spectators = 0;
			this._Scientists = 0;
			this._SCPs = 0;
			this._SCPsNozombies = 0;
			GameObject[] players = this.pm.players;
			GameObject[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				CharacterClassManager component = array[i].GetComponent<CharacterClassManager>();
				if (component.curClass >= 0)
				{
					Team team = component.klasy[component.curClass].team;
					if (team == Team.CDP)
					{
						this._ClassDs++;
					}
					else if (team == Team.CHI)
					{
						this._ChaosInsurgency++;
					}
					else if (team == Team.MTF)
					{
						this._MobileForces++;
					}
					else if (team == Team.RIP)
					{
						this._Spectators++;
					}
					else if (team == Team.RSC)
					{
						this._Scientists++;
					}
					else if (team == Team.SCP)
					{
						this._SCPs++;
						if (component.curClass != 10)
						{
							this._SCPsNozombies++;
						}
					}
				}
			}
			int num = 0;
			if (this._ClassDs > 0)
			{
				num++;
			}
			if (this._MobileForces > 0 || this._Scientists > 0)
			{
				num++;
			}
			if (this._SCPs > 0)
			{
				num++;
			}
			if (this._ChaosInsurgency > 0 && (this._MobileForces > 0 || this._Scientists > 0))
			{
				num = 3;
			}
			if (num <= 1 && players.Length >= 2)
			{
				this.roundHasEnded = true;
			}
			if (this.debugMode)
			{
				this.roundHasEnded = false;
			}
			this.summary.scp_alive = this._SCPs;
			this.summary.scp_nozombies = this._SCPsNozombies;
			if (this.roundHasEnded)
			{
				this.summary.classD_escaped += this._ClassDs;
				this.summary.scientists_escaped += this._Scientists;
				int @int = ConfigFile.GetInt("auto_round_restart_time", 10);
				this.CallCmdSetSummary(this.summary, @int);
				base.Invoke("RoundRestart", (float)@int);
			}
		}
	}

	// Token: 0x060004C8 RID: 1224 RVA: 0x0001E104 File Offset: 0x0001C304
	private void Update()
	{
		if (RoundSummary.host == null)
		{
			GameObject gameObject = GameObject.Find("Host");
			if (gameObject != null)
			{
				RoundSummary.host = gameObject.GetComponent<RoundSummary>();
			}
		}
	}

	// Token: 0x060004C9 RID: 1225 RVA: 0x0000525C File Offset: 0x0000345C
	[Command(channel = 15)]
	private void CmdSetSummary(RoundSummary.Summary sum, int posttime)
	{
		this.CallRpcSetSummary(sum, posttime);
	}

	// Token: 0x060004CA RID: 1226 RVA: 0x0001E144 File Offset: 0x0001C344
	[ClientRpc(channel = 15)]
	public void RpcSetSummary(RoundSummary.Summary sum, int posttime)
	{
		Radio.roundEnded = true;
		string text = string.Empty;
		string text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"<color=#ff0000>",
			sum.classD_escaped,
			"/",
			sum.classD_start,
			"</color> ",
			TranslationReader.Get("Legancy_Interfaces", 3),
			"\n"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"<color=#ff0000>",
			sum.scientists_escaped,
			"/",
			sum.scientists_start,
			"</color> ",
			TranslationReader.Get("Legancy_Interfaces", 4),
			"\n"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"<color=#ff0000>",
			sum.scp_frags,
			"</color> ",
			TranslationReader.Get("Legancy_Interfaces", 5),
			"\n"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"<color=#ff0000>",
			Mathf.Clamp(sum.scp_start - sum.scp_alive, 0, sum.scp_start),
			"/",
			sum.scp_start,
			"</color> ",
			TranslationReader.Get("Legancy_Interfaces", 6),
			"\n"
		});
		text2 = text;
		text = string.Concat(new string[]
		{
			text2,
			TranslationReader.Get("Legancy_Interfaces", 7),
			": <color=#ff0000>",
			(!sum.warheadDetonated) ? TranslationReader.Get("Legancy_Interfaces", 9) : TranslationReader.Get("Legancy_Interfaces", 8),
			"</color>\n\n"
		});
		text += TranslationReader.Get("Legancy_Interfaces", 10).Replace("[time]", posttime.ToString());
		GameObject gameObject = UserMainInterface.singleton.summary;
		gameObject.SetActive(true);
		TextMeshProUGUI component = GameObject.FindGameObjectWithTag("Summary").GetComponent<TextMeshProUGUI>();
		component.text = text;
	}

	// Token: 0x060004CB RID: 1227 RVA: 0x00002195 File Offset: 0x00000395
	private void UNetVersion()
	{
	}

	// Token: 0x060004CC RID: 1228 RVA: 0x00005266 File Offset: 0x00003466
	protected static void InvokeCmdCmdSetSummary(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetSummary called on client.");
			return;
		}
		((RoundSummary)obj).CmdSetSummary(GeneratedNetworkCode._ReadSummary_RoundSummary(reader), (int)reader.ReadPackedUInt32());
	}

	// Token: 0x060004CD RID: 1229 RVA: 0x0001E378 File Offset: 0x0001C578
	public void CallCmdSetSummary(RoundSummary.Summary sum, int posttime)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetSummary called on server.");
			return;
		}
		if (base.isServer)
		{
			this.CmdSetSummary(sum, posttime);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)5));
		networkWriter.WritePackedUInt32((uint)RoundSummary.kCmdCmdSetSummary);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteSummary_RoundSummary(networkWriter, sum);
		networkWriter.WritePackedUInt32((uint)posttime);
		base.SendCommandInternal(networkWriter, 15, "CmdSetSummary");
	}

	// Token: 0x060004CE RID: 1230 RVA: 0x00005295 File Offset: 0x00003495
	protected static void InvokeRpcRpcSetSummary(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetSummary called on server.");
			return;
		}
		((RoundSummary)obj).RpcSetSummary(GeneratedNetworkCode._ReadSummary_RoundSummary(reader), (int)reader.ReadPackedUInt32());
	}

	// Token: 0x060004CF RID: 1231 RVA: 0x0001E410 File Offset: 0x0001C610
	public void CallRpcSetSummary(RoundSummary.Summary sum, int posttime)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSetSummary called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)2));
		networkWriter.WritePackedUInt32((uint)RoundSummary.kRpcRpcSetSummary);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteSummary_RoundSummary(networkWriter, sum);
		networkWriter.WritePackedUInt32((uint)posttime);
		this.SendRPCInternal(networkWriter, 15, "RpcSetSummary");
	}

	// Token: 0x060004D0 RID: 1232 RVA: 0x0001E490 File Offset: 0x0001C690
	static RoundSummary()
	{
		NetworkBehaviour.RegisterCommandDelegate(typeof(RoundSummary), RoundSummary.kCmdCmdSetSummary, new NetworkBehaviour.CmdDelegate(RoundSummary.InvokeCmdCmdSetSummary));
		RoundSummary.kRpcRpcSetSummary = -1626633486;
		NetworkBehaviour.RegisterRpcDelegate(typeof(RoundSummary), RoundSummary.kRpcRpcSetSummary, new NetworkBehaviour.CmdDelegate(RoundSummary.InvokeRpcRpcSetSummary));
		NetworkCRC.RegisterBehaviour("RoundSummary", 0);
	}

	// Token: 0x060004D1 RID: 1233 RVA: 0x0000A490 File Offset: 0x00008690
	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result;
		return result;
	}

	// Token: 0x060004D2 RID: 1234 RVA: 0x00002195 File Offset: 0x00000395
	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	// Token: 0x04000455 RID: 1109
	public bool debugMode;

	// Token: 0x04000456 RID: 1110
	private bool roundHasEnded;

	// Token: 0x04000457 RID: 1111
	private PlayerManager pm;

	// Token: 0x04000458 RID: 1112
	private CharacterClassManager ccm;

	// Token: 0x04000459 RID: 1113
	public static RoundSummary host;

	// Token: 0x0400045A RID: 1114
	public RoundSummary.Summary summary;

	// Token: 0x0400045B RID: 1115
	private int _ClassDs;

	// Token: 0x0400045C RID: 1116
	private int _ChaosInsurgency;

	// Token: 0x0400045D RID: 1117
	private int _MobileForces;

	// Token: 0x0400045E RID: 1118
	private int _Spectators;

	// Token: 0x0400045F RID: 1119
	private int _Scientists;

	// Token: 0x04000460 RID: 1120
	private int _SCPs;

	// Token: 0x04000461 RID: 1121
	private int _SCPsNozombies;

	// Token: 0x04000462 RID: 1122
	private static int kCmdCmdSetSummary = 509590172;

	// Token: 0x04000463 RID: 1123
	private static int kRpcRpcSetSummary;

	// Token: 0x020000C1 RID: 193
	[Serializable]
	public class Summary
	{
		// Token: 0x04000464 RID: 1124
		public int classD_escaped;

		// Token: 0x04000465 RID: 1125
		public int classD_start;

		// Token: 0x04000466 RID: 1126
		public int scientists_escaped;

		// Token: 0x04000467 RID: 1127
		public int scientists_start;

		// Token: 0x04000468 RID: 1128
		public int scp_frags;

		// Token: 0x04000469 RID: 1129
		public int scp_start;

		// Token: 0x0400046A RID: 1130
		public int scp_alive;

		// Token: 0x0400046B RID: 1131
		public int scp_nozombies;

		// Token: 0x0400046C RID: 1132
		public bool warheadDetonated;
	}
}
