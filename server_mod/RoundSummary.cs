using System;
using GameConsole;
using TMPro;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x020000BC RID: 188
public class RoundSummary : NetworkBehaviour
{
	// Token: 0x0600049D RID: 1181 RVA: 0x00005191 File Offset: 0x00003391
	private void Awake()
	{
		Radio.roundEnded = false;
	}

	// Token: 0x0600049E RID: 1182 RVA: 0x00005199 File Offset: 0x00003399
	private void Start()
	{
		this.pm = PlayerManager.singleton;
		this.ccm = base.GetComponent<CharacterClassManager>();
		base.InvokeRepeating("CheckForEnding", 12f, 3f);
	}

	// Token: 0x0600049F RID: 1183 RVA: 0x0001D0D0 File Offset: 0x0001B2D0
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

	// Token: 0x060004A0 RID: 1184
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

	// Token: 0x060004A1 RID: 1185 RVA: 0x0001D410 File Offset: 0x0001B610
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

	// Token: 0x060004A2 RID: 1186 RVA: 0x000051C7 File Offset: 0x000033C7
	[Command(channel = 15)]
	private void CmdSetSummary(RoundSummary.Summary sum, int posttime)
	{
		this.CallRpcSetSummary(sum, posttime);
	}

	// Token: 0x060004A3 RID: 1187 RVA: 0x0001D450 File Offset: 0x0001B650
	[ClientRpc(channel = 15)]
	public void RpcSetSummary(RoundSummary.Summary sum, int posttime)
	{
		Radio.roundEnded = true;
		string text = string.Empty;
		if (PlayerPrefs.GetString("langver", "en") == "pl")
		{
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"<color=#ff0000>",
				sum.classD_escaped,
				"/",
				sum.classD_start,
				"</color> Personelu Klasy D uciekło z placówki\n"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"<color=#ff0000>",
				sum.scientists_escaped,
				"/",
				sum.scientists_start,
				"</color> Naukowców ocalało\n"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"<color=#ff0000>",
				sum.scp_frags,
				"</color> Zabitych przez SCP\n"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"<color=#ff0000>",
				sum.scp_start - sum.scp_nozombies,
				"/",
				sum.scp_start,
				"</color> Unieszkodliwionych podmiotów SCP\n"
			});
			text = text + "Głowica Alfa: <color=#ff0000>" + ((!sum.warheadDetonated) ? "Nie została użyta" : "Zdetonowana") + "</color>\n\n";
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"Następna runda rozpocznie się w ciągu ",
				posttime,
				" sekund."
			});
		}
		else
		{
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"<color=#ff0000>",
				sum.classD_escaped,
				"/",
				sum.classD_start,
				"</color> Class-D Personnel escaped\n"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"<color=#ff0000>",
				sum.scientists_escaped,
				"/",
				sum.scientists_start,
				"</color> Scientists survived\n"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"<color=#ff0000>",
				sum.scp_frags,
				"</color> Killed by SCP\n"
			});
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"<color=#ff0000>",
				sum.scp_start - sum.scp_alive,
				"/",
				sum.scp_start,
				"</color> Terminated SCP subjects\n"
			});
			text = text + "Alpha Warhead: <color=#ff0000>" + ((!sum.warheadDetonated) ? "Unused" : "Detonated") + "</color>\n\n";
			text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"The next round will start within ",
				posttime,
				" seconds."
			});
		}
		GameObject gameObject = UserMainInterface.singleton.summary;
		gameObject.SetActive(true);
		TextMeshProUGUI component = GameObject.FindGameObjectWithTag("Summary").GetComponent<TextMeshProUGUI>();
		component.text = text;
	}

	// Token: 0x060004A4 RID: 1188 RVA: 0x0000215A File Offset: 0x0000035A
	private void UNetVersion()
	{
	}

	// Token: 0x060004A5 RID: 1189 RVA: 0x000051D1 File Offset: 0x000033D1
	protected static void InvokeCmdCmdSetSummary(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetSummary called on client.");
			return;
		}
		((RoundSummary)obj).CmdSetSummary(GeneratedNetworkCode._ReadSummary_RoundSummary(reader), (int)reader.ReadPackedUInt32());
	}

	// Token: 0x060004A6 RID: 1190 RVA: 0x0001D774 File Offset: 0x0001B974
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

	// Token: 0x060004A7 RID: 1191 RVA: 0x00005200 File Offset: 0x00003400
	protected static void InvokeRpcRpcSetSummary(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetSummary called on server.");
			return;
		}
		((RoundSummary)obj).RpcSetSummary(GeneratedNetworkCode._ReadSummary_RoundSummary(reader), (int)reader.ReadPackedUInt32());
	}

	// Token: 0x060004A8 RID: 1192 RVA: 0x0001D80C File Offset: 0x0001BA0C
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

	// Token: 0x060004A9 RID: 1193 RVA: 0x0001D88C File Offset: 0x0001BA8C
	static RoundSummary()
	{
		NetworkBehaviour.RegisterCommandDelegate(typeof(RoundSummary), RoundSummary.kCmdCmdSetSummary, new NetworkBehaviour.CmdDelegate(RoundSummary.InvokeCmdCmdSetSummary));
		RoundSummary.kRpcRpcSetSummary = -1626633486;
		NetworkBehaviour.RegisterRpcDelegate(typeof(RoundSummary), RoundSummary.kRpcRpcSetSummary, new NetworkBehaviour.CmdDelegate(RoundSummary.InvokeRpcRpcSetSummary));
		NetworkCRC.RegisterBehaviour("RoundSummary", 0);
	}

	// Token: 0x060004AA RID: 1194 RVA: 0x0000A298 File Offset: 0x00008498
	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result;
		return result;
	}

	// Token: 0x060004AB RID: 1195 RVA: 0x0000215A File Offset: 0x0000035A
	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	// Token: 0x04000426 RID: 1062
	public bool debugMode;

	// Token: 0x04000427 RID: 1063
	private bool roundHasEnded;

	// Token: 0x04000428 RID: 1064
	private PlayerManager pm;

	// Token: 0x04000429 RID: 1065
	private CharacterClassManager ccm;

	// Token: 0x0400042A RID: 1066
	public static RoundSummary host;

	// Token: 0x0400042B RID: 1067
	public RoundSummary.Summary summary;

	// Token: 0x0400042C RID: 1068
	private int _ClassDs;

	// Token: 0x0400042D RID: 1069
	private int _ChaosInsurgency;

	// Token: 0x0400042E RID: 1070
	private int _MobileForces;

	// Token: 0x0400042F RID: 1071
	private int _Spectators;

	// Token: 0x04000430 RID: 1072
	private int _Scientists;

	// Token: 0x04000431 RID: 1073
	private int _SCPs;

	// Token: 0x04000432 RID: 1074
	private int _SCPsNozombies;

	// Token: 0x04000433 RID: 1075
	private static int kCmdCmdSetSummary = 509590172;

	// Token: 0x04000434 RID: 1076
	private static int kRpcRpcSetSummary;

	// Token: 0x020000BD RID: 189
	[Serializable]
	public class Summary
	{
		// Token: 0x04000435 RID: 1077
		public int classD_escaped;

		// Token: 0x04000436 RID: 1078
		public int classD_start;

		// Token: 0x04000437 RID: 1079
		public int scientists_escaped;

		// Token: 0x04000438 RID: 1080
		public int scientists_start;

		// Token: 0x04000439 RID: 1081
		public int scp_frags;

		// Token: 0x0400043A RID: 1082
		public int scp_start;

		// Token: 0x0400043B RID: 1083
		public int scp_alive;

		// Token: 0x0400043C RID: 1084
		public int scp_nozombies;

		// Token: 0x0400043D RID: 1085
		public bool warheadDetonated;
	}
}
