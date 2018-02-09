using System;
using Dissonance.Integrations.UNet_HLAPI;
using GameConsole;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x020000A6 RID: 166
public class PlayerStats : NetworkBehaviour
{
	// Token: 0x06000409 RID: 1033
	private void Start()
	{
		this.ccm = base.GetComponent<CharacterClassManager>();
		this.ui = UserMainInterface.singleton;
		this.smDo106Cleaning = ConfigFile.GetString("clean_106", "no").Equals("yes");
	}

	// Token: 0x0600040A RID: 1034 RVA: 0x00016140 File Offset: 0x00014340
	public float GetHealthPercent()
	{
		return Mathf.Clamp01(1f - (float)this.health / (float)this.ccm.klasy[this.ccm.curClass].maxHP);
	}

	// Token: 0x0600040B RID: 1035 RVA: 0x00016173 File Offset: 0x00014373
	[Command(channel = 2)]
	public void CmdSelfDeduct(PlayerStats.HitInfo info)
	{
		this.HurtPlayer(info, base.gameObject);
	}

	// Token: 0x0600040C RID: 1036 RVA: 0x00016184 File Offset: 0x00014384
	public void Explode()
	{
		bool flag = this.health >= 1 && base.transform.position.y < 900f;
		if (this.ccm.curClass == 3)
		{
			Scp106PlayerScript component = base.GetComponent<Scp106PlayerScript>();
			component.DeletePortal();
			if (component.goingViaThePortal)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			foreach (LiftIdentity liftIdentity in UnityEngine.Object.FindObjectsOfType<LiftIdentity>())
			{
				if (liftIdentity.InArea(base.transform.position))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			this.HurtPlayer(new PlayerStats.HitInfo(999999f, "WORLD", "NUKE"), base.gameObject);
		}
	}

	// Token: 0x0600040D RID: 1037 RVA: 0x00016258 File Offset: 0x00014458
	private void Update()
	{
		if (base.isLocalPlayer && this.ccm.curClass != 2)
		{
			this.ui.SetHP(this.health, this.maxHP);
			GameConsole.Console.singleton.UpdateValue("info", this.lastHitInfo.tool);
		}
		if (base.isLocalPlayer)
		{
			this.ui.hpOBJ.SetActive(this.ccm.curClass != 2);
		}
	}

	// Token: 0x0600040E RID: 1038 RVA: 0x000162DE File Offset: 0x000144DE
	[Command(channel = 2)]
	public void CmdTesla()
	{
		this.HurtPlayer(new PlayerStats.HitInfo((float)UnityEngine.Random.Range(100, 200), base.GetComponent<HlapiPlayer>().PlayerId, "TESLA"), base.gameObject);
	}

	// Token: 0x0600040F RID: 1039
	public void SetHPAmount(int hp)
	{
		this.Networkhealth = hp;
	}

	// Token: 0x06000410 RID: 1040
	public void HurtPlayer(PlayerStats.HitInfo info, GameObject go)
	{
		PlayerStats component = go.GetComponent<PlayerStats>();
		CharacterClassManager component2 = go.GetComponent<CharacterClassManager>();
		PlayerStats playerStats = component;
		playerStats.Networkhealth = playerStats.health - Mathf.CeilToInt(info.amount);
		if (component.health < 1 && component2.curClass != 2)
		{
			if (component2.curClass == 3)
			{
				go.GetComponent<Scp106PlayerScript>().CallRpcAnnounceContaining();
			}
			if (info.amount != 999799f && component2.curClass != 7)
			{
				if (info.tool.Equals("POCKET"))
				{
					if (!this.smDo106Cleaning)
					{
						this.smDoRagdoll(info, go);
					}
				}
				else
				{
					this.smDoRagdoll(info, go);
				}
			}
			component2.NetworkdeathPosition = go.transform.position;
			component.SetHPAmount(100);
			component2.SetClassID(2);
			if (TutorialManager.status)
			{
				PlayerManager.localPlayer.GetComponent<TutorialManager>().KillNPC();
			}
		}
	}

	// Token: 0x06000411 RID: 1041 RVA: 0x0001641B File Offset: 0x0001461B
	[ServerCallback]
	private void CmdRoundrestart()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		this.CallRpcRoundrestart();
	}

	// Token: 0x06000412 RID: 1042 RVA: 0x00016430 File Offset: 0x00014630
	[ClientRpc(channel = 7)]
	private void RpcRoundrestart()
	{
		if (!base.isServer)
		{
			CustomNetworkManager customNetworkManager = UnityEngine.Object.FindObjectOfType<CustomNetworkManager>();
			customNetworkManager.reconnect = true;
			base.Invoke("ChangeLevel", 0.5f);
		}
	}

	// Token: 0x06000413 RID: 1043 RVA: 0x00016465 File Offset: 0x00014665
	public void Roundrestart()
	{
		this.CmdRoundrestart();
		base.Invoke("ChangeLevel", 2.5f);
	}

	// Token: 0x06000414 RID: 1044 RVA: 0x0001647D File Offset: 0x0001467D
	private void ChangeLevel()
	{
		if (base.isServer)
		{
			NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
		}
		else
		{
			NetworkManager.singleton.StopClient();
		}
	}

	// Token: 0x06000415 RID: 1045 RVA: 0x00002495 File Offset: 0x00000695
	private void UNetVersion()
	{
	}

	// Token: 0x1700005A RID: 90
	// (get) Token: 0x06000416 RID: 1046 RVA: 0x000164B0 File Offset: 0x000146B0
	// (set) Token: 0x06000417 RID: 1047 RVA: 0x000164C3 File Offset: 0x000146C3
	public int Networkhealth
	{
		get
		{
			return this.health;
		}
		set
		{
			uint dirtyBit = 1u;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				this.SetHPAmount(value);
				base.syncVarHookGuard = false;
			}
			base.SetSyncVar<int>(value, ref this.health, dirtyBit);
		}
	}

	// Token: 0x06000418 RID: 1048 RVA: 0x00016502 File Offset: 0x00014702
	protected static void InvokeCmdCmdSelfDeduct(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSelfDeduct called on client.");
			return;
		}
		((PlayerStats)obj).CmdSelfDeduct(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
	}

	// Token: 0x06000419 RID: 1049 RVA: 0x0001652B File Offset: 0x0001472B
	protected static void InvokeCmdCmdTesla(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTesla called on client.");
			return;
		}
		((PlayerStats)obj).CmdTesla();
	}

	// Token: 0x0600041A RID: 1050 RVA: 0x00016550 File Offset: 0x00014750
	public void CallCmdSelfDeduct(PlayerStats.HitInfo info)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSelfDeduct called on server.");
			return;
		}
		if (base.isServer)
		{
			this.CmdSelfDeduct(info);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)5));
		networkWriter.WritePackedUInt32((uint)PlayerStats.kCmdCmdSelfDeduct);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteHitInfo_PlayerStats(networkWriter, info);
		base.SendCommandInternal(networkWriter, 2, "CmdSelfDeduct");
	}

	// Token: 0x0600041B RID: 1051 RVA: 0x000165DC File Offset: 0x000147DC
	public void CallCmdTesla()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdTesla called on server.");
			return;
		}
		if (base.isServer)
		{
			this.CmdTesla();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)5));
		networkWriter.WritePackedUInt32((uint)PlayerStats.kCmdCmdTesla);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		base.SendCommandInternal(networkWriter, 2, "CmdTesla");
	}

	// Token: 0x0600041C RID: 1052 RVA: 0x00016658 File Offset: 0x00014858
	protected static void InvokeRpcRpcRoundrestart(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRoundrestart called on server.");
			return;
		}
		((PlayerStats)obj).RpcRoundrestart();
	}

	// Token: 0x0600041D RID: 1053 RVA: 0x0001667C File Offset: 0x0001487C
	public void CallRpcRoundrestart()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRoundrestart called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)2));
		networkWriter.WritePackedUInt32((uint)PlayerStats.kRpcRpcRoundrestart);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		this.SendRPCInternal(networkWriter, 7, "RpcRoundrestart");
	}

	// Token: 0x0600041E RID: 1054 RVA: 0x000166E8 File Offset: 0x000148E8
	static PlayerStats()
	{
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerStats), PlayerStats.kCmdCmdSelfDeduct, new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeCmdCmdSelfDeduct));
		PlayerStats.kCmdCmdTesla = -1109720487;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerStats), PlayerStats.kCmdCmdTesla, new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeCmdCmdTesla));
		PlayerStats.kRpcRpcRoundrestart = 907411477;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), PlayerStats.kRpcRpcRoundrestart, new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeRpcRpcRoundrestart));
		NetworkCRC.RegisterBehaviour("PlayerStats", 0);
	}

	// Token: 0x0600041F RID: 1055 RVA: 0x00016784 File Offset: 0x00014984
	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)this.health);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1u) != 0u)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)this.health);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	// Token: 0x06000420 RID: 1056 RVA: 0x000167F0 File Offset: 0x000149F0
	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			this.health = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			this.SetHPAmount((int)reader.ReadPackedUInt32());
		}
	}

	// Token: 0x06000BA0 RID: 2976
	public void smDoRagdoll(PlayerStats.HitInfo info, GameObject go)
	{
		PlayerStats component = go.GetComponent<PlayerStats>();
		CharacterClassManager characterClassManager;
		base.GetComponent<RagdollManager>().SpawnRagdoll(go.transform.position, go.transform.rotation, characterClassManager.curClass, info, characterClassManager.klasy[characterClassManager.curClass].team != Team.SCP, go.GetComponent<HlapiPlayer>().PlayerId, go.GetComponent<NicknameSync>().myNick);
	}

	// Token: 0x040003A4 RID: 932
	public PlayerStats.HitInfo lastHitInfo = new PlayerStats.HitInfo(0f, "NONE", "NONE");

	// Token: 0x040003A5 RID: 933
	[SyncVar(hook = "SetHPAmount")]
	public int health;

	// Token: 0x040003A6 RID: 934
	public int maxHP;

	// Token: 0x040003A7 RID: 935
	private UserMainInterface ui;

	// Token: 0x040003A8 RID: 936
	private CharacterClassManager ccm;

	// Token: 0x040003A9 RID: 937
	private static int kCmdCmdSelfDeduct = -2147454163;

	// Token: 0x040003AA RID: 938
	private static int kCmdCmdTesla;

	// Token: 0x040003AB RID: 939
	private static int kRpcRpcRoundrestart;

	// Token: 0x04000C26 RID: 3110
	public bool smDo106Cleaning;

	// Token: 0x020000A7 RID: 167
	[Serializable]
	public struct HitInfo
	{
		// Token: 0x06000421 RID: 1057 RVA: 0x00016831 File Offset: 0x00014A31
		public HitInfo(float amnt, string plyID, string weapon)
		{
			this.amount = amnt;
			this.tool = weapon;
			this.time = ServerTime.time;
		}

		// Token: 0x040003AC RID: 940
		public float amount;

		// Token: 0x040003AD RID: 941
		public string tool;

		// Token: 0x040003AE RID: 942
		public int time;
	}
}
