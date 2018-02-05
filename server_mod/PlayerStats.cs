// PROBLEMATIC FILE - HAD TO USE IL ASSEMBLY TO MODIFY.
using System;
using Dissonance.Integrations.UNet_HLAPI;
using GameConsole;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x020000A4 RID: 164
public class PlayerStats : NetworkBehaviour
{
	// Token: 0x0600040D RID: 1037 RVA: 0x00004BC5 File Offset: 0x00002DC5
	public PlayerStats()
	{
	}

	// Token: 0x0600040E RID: 1038 RVA: 0x00004BE7 File Offset: 0x00002DE7
	public void Start()
	{
		this.ccm = base.GetComponent<CharacterClassManager>();
		this.ui = UserMainInterface.singleton;
	}

	// Token: 0x0600040F RID: 1039 RVA: 0x00004C00 File Offset: 0x00002E00
	public float GetHealthPercent()
	{
		return Mathf.Clamp01(1f - (float)this.health / (float)this.ccm.klasy[this.ccm.curClass].maxHP);
	}

	// Token: 0x06000410 RID: 1040 RVA: 0x00004C33 File Offset: 0x00002E33
	[Command(channel = 2)]
	public void CmdSelfDeduct(PlayerStats.HitInfo info)
	{
		this.HurtPlayer(info, base.gameObject);
	}

	// Token: 0x06000411 RID: 1041 RVA: 0x0001A318 File Offset: 0x00018518
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

	// Token: 0x06000412 RID: 1042 RVA: 0x0001A3EC File Offset: 0x000185EC
	public void Update()
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

	// Token: 0x06000413 RID: 1043 RVA: 0x00004C42 File Offset: 0x00002E42
	[Command(channel = 2)]
	public void CmdTesla()
	{
		this.HurtPlayer(new PlayerStats.HitInfo((float)UnityEngine.Random.Range(100, 200), base.GetComponent<HlapiPlayer>().PlayerId, "TESLA"), base.gameObject);
	}

	// Token: 0x06000414 RID: 1044
	public void SetHPAmount(int hp)
	{
		this.Networkhealth = hp;
	}

	// Token: 0x06000415 RID: 1045
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
			if (info.tool.Equals("POCKET"))
			{
				go.GetComponent<Inventory>().RemoveAll();
			}
			if (info.amount != 999799f && !info.tool.Equals("POCKET") && component2.curClass != 7)
			{
				this.DoRagdoll(info, go);
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

	// Token: 0x06000416 RID: 1046 RVA: 0x00004C7B File Offset: 0x00002E7B
	[ServerCallback]
	public void CmdRoundrestart()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		this.CallRpcRoundrestart();
	}

	// Token: 0x06000417 RID: 1047 RVA: 0x0001A578 File Offset: 0x00018778
	[ClientRpc(channel = 7)]
	public void RpcRoundrestart()
	{
		if (!base.isServer)
		{
			CustomNetworkManager customNetworkManager = UnityEngine.Object.FindObjectOfType<CustomNetworkManager>();
			customNetworkManager.reconnect = true;
			base.Invoke("ChangeLevel", 0.5f);
		}
	}

	// Token: 0x06000418 RID: 1048 RVA: 0x00004C8E File Offset: 0x00002E8E
	public void Roundrestart()
	{
		this.CmdRoundrestart();
		base.Invoke("ChangeLevel", 2.5f);
	}

	// Token: 0x06000419 RID: 1049 RVA: 0x0001A5B0 File Offset: 0x000187B0
	public void ChangeLevel()
	{
		if (base.isServer)
		{
			GameConsole.Console.singleton.AddLog("round ended", new Color32(128, 128, 128, byte.MaxValue), false);
			NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
			return;
		}
		NetworkManager.singleton.StopClient();
	}

	// Token: 0x0600041A RID: 1050 RVA: 0x000020C4 File Offset: 0x000002C4
	public void UNetVersion()
	{
	}

	// Token: 0x17000058 RID: 88
	// (get) Token: 0x0600041B RID: 1051 RVA: 0x0001A610 File Offset: 0x00018810
	// (set) Token: 0x0600041C RID: 1052 RVA: 0x00004CA6 File Offset: 0x00002EA6
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

	// Token: 0x0600041D RID: 1053 RVA: 0x00004CE5 File Offset: 0x00002EE5
	public static void InvokeCmdCmdSelfDeduct(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSelfDeduct called on client.");
			return;
		}
		((PlayerStats)obj).CmdSelfDeduct(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
	}

	// Token: 0x0600041E RID: 1054 RVA: 0x00004D0E File Offset: 0x00002F0E
	public static void InvokeCmdCmdTesla(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTesla called on client.");
			return;
		}
		((PlayerStats)obj).CmdTesla();
	}

	// Token: 0x0600041F RID: 1055 RVA: 0x0001A624 File Offset: 0x00018824
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

	// Token: 0x06000420 RID: 1056 RVA: 0x0001A6B0 File Offset: 0x000188B0
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

	// Token: 0x06000421 RID: 1057 RVA: 0x00004D31 File Offset: 0x00002F31
	public static void InvokeRpcRpcRoundrestart(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRoundrestart called on server.");
			return;
		}
		((PlayerStats)obj).RpcRoundrestart();
	}

	// Token: 0x06000422 RID: 1058 RVA: 0x0001A72C File Offset: 0x0001892C
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

	// Token: 0x06000423 RID: 1059 RVA: 0x0001A798 File Offset: 0x00018998
	static PlayerStats()
	{
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerStats), PlayerStats.kCmdCmdSelfDeduct, new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeCmdCmdSelfDeduct));
		PlayerStats.kCmdCmdTesla = -1109720487;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerStats), PlayerStats.kCmdCmdTesla, new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeCmdCmdTesla));
		PlayerStats.kRpcRpcRoundrestart = 907411477;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), PlayerStats.kRpcRpcRoundrestart, new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeRpcRpcRoundrestart));
		NetworkCRC.RegisterBehaviour("PlayerStats", 0);
	}

	// Token: 0x06000424 RID: 1060 RVA: 0x0001A834 File Offset: 0x00018A34
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

	// Token: 0x06000425 RID: 1061 RVA: 0x0001A8A0 File Offset: 0x00018AA0
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

	// Token: 0x06000B22 RID: 2850
	public void ServerModHurtHook(GameObject go)
	{
		CharacterClassManager component2 = go.GetComponent<CharacterClassManager>();
		if (component2.curClass == 0)
		{
			component2.is173InRound = false;
		}
	}

	// Token: 0x06000B73 RID: 2931
	public void test(GameObject ob)
	{
	}

	// Token: 0x06000EE5 RID: 3813
	public void test(PlayerStats.HitInfo info)
	{
	}

	// Token: 0x06000F59 RID: 3929
	public void DoRagdoll(GameObject go, CharacterClassManager component2, PlayerStats.HitInfo info)
	{
		info.ToString();
	}

	// Token: 0x06000FF4 RID: 4084
	public void DoRagdoll(PlayerStats.HitInfo info, GameObject go)
	{
		CharacterClassManager component = go.GetComponent<CharacterClassManager>();
		base.GetComponent<RagdollManager>().SpawnRagdoll(go.transform.position, go.transform.rotation, component.curClass, info, component.klasy[component.curClass].team != Team.SCP, go.GetComponent<HlapiPlayer>().PlayerId, go.GetComponent<NicknameSync>().myNick);
	}

	// Token: 0x0400039D RID: 925
	public PlayerStats.HitInfo lastHitInfo = new PlayerStats.HitInfo(0f, "NONE", "NONE");

	// Token: 0x0400039E RID: 926
	[SyncVar(hook = "SetHPAmount")]
	public int health;

	// Token: 0x0400039F RID: 927
	public int maxHP;

	// Token: 0x040003A0 RID: 928
	public UserMainInterface ui;

	// Token: 0x040003A1 RID: 929
	public CharacterClassManager ccm;

	// Token: 0x040003A2 RID: 930
	public static int kCmdCmdSelfDeduct = -2147454163;

	// Token: 0x040003A3 RID: 931
	public static int kCmdCmdTesla;

	// Token: 0x040003A4 RID: 932
	public static int kRpcRpcRoundrestart;

	// Token: 0x020000A5 RID: 165
	[Serializable]
	public struct HitInfo
	{
		// Token: 0x06000426 RID: 1062 RVA: 0x00004D54 File Offset: 0x00002F54
		public HitInfo(float amnt, string plyID, string weapon)
		{
			this.amount = amnt;
			this.tool = weapon;
			this.time = ServerTime.time;
		}

		// Token: 0x040003A5 RID: 933
		public float amount;

		// Token: 0x040003A6 RID: 934
		public string tool;

		// Token: 0x040003A7 RID: 935
		public int time;
	}
}
