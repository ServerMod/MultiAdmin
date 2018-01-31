using System;
using System.Collections;
using System.Collections.Generic;
using GameConsole;
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PostProcessing;

// Token: 0x02000068 RID: 104
public class CharacterClassManager : NetworkBehaviour
{
	// Token: 0x060001E7 RID: 487
	public void SetUnit(int unit)
	{
		this.NetworkntfUnit = unit;
	}

	// Token: 0x060001E8 RID: 488
	public void SyncDeathPos(Vector3 v)
	{
		this.NetworkdeathPosition = v;
	}

	// Token: 0x060001E9 RID: 489
	[ServerCallback]
	public void AllowContain()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (TutorialManager.status)
		{
			return;
		}
		foreach (GameObject gameObject in PlayerManager.singleton.players)
		{
			if (Vector3.Distance(gameObject.transform.position, this.lureSpj.transform.position) < 1.97f)
			{
				CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
				PlayerStats component2 = gameObject.GetComponent<PlayerStats>();
				if (component.klasy[component.curClass].team != Team.SCP && component.curClass != 2)
				{
					component2.HurtPlayer(new PlayerStats.HitInfo(10000f, "WORLD", "LURE"), gameObject);
					this.lureSpj.SetState(true);
				}
			}
		}
	}

	// Token: 0x060001EA RID: 490
	private void Start()
	{
		this.lureSpj = UnityEngine.Object.FindObjectOfType<LureSubjectContainer>();
		this.scp049 = base.GetComponent<Scp049PlayerScript>();
		this.scp049_2 = base.GetComponent<Scp049_2PlayerScript>();
		this.scp079 = base.GetComponent<Scp079PlayerScript>();
		this.scp106 = base.GetComponent<Scp106PlayerScript>();
		this.scp173 = base.GetComponent<Scp173PlayerScript>();
		this.ban_computer_for_first_pick = (ConfigFile.GetString("NO_SCP079_FIRST", "true").ToLower() == "true");
		this.forceClass = ConfigFile.GetInt("server_forced_class", -1);
		this.ciPercentage = (float)ConfigFile.GetInt("ci_on_start_percent", 10);
		base.StartCoroutine("Init");
		string text = ConfigFile.GetString("team_respawn_queue", "401431403144144") + "...........................";
		this.classTeamQueue.Clear();
		for (int i = 0; i < text.Length; i++)
		{
			int item = 4;
			if (!int.TryParse(text[i].ToString(), out item))
			{
				item = 4;
			}
			this.classTeamQueue.Add((Team)item);
		}
		if (!base.isLocalPlayer && TutorialManager.status)
		{
			this.ApplyProperties();
		}
		this.SetMaxHP(0, "SCP173_HP", 2000);
		this.SetMaxHP(5, "SCP049_HP", 1200);
		this.SetMaxHP(7, "SCP079_HP", 100);
		this.SetMaxHP(3, "SCP106_HP", 700);
		this.SetMaxHP(9, "SCP457_HP", 700);
		this.SetMaxHP(10, "SCP049-2_HP", 400);
	}

	// Token: 0x060001EB RID: 491
	private IEnumerator Init()
	{
		GameObject host = null;
		while (host == null)
		{
			host = GameObject.Find("Host");
			yield return new WaitForEndOfFrame();
		}
		while (this.seed == 0)
		{
			this.seed = host.GetComponent<RandomSeedSync>().seed;
			UnityEngine.Object.FindObjectOfType<GameConsole.Console>().UpdateValue("seed", this.seed.ToString());
		}
		if (!base.isLocalPlayer)
		{
			yield break;
		}
		yield return new WaitForSeconds(2f);
		if (base.isServer)
		{
			if (ServerStatic.isDedicated)
			{
				ServerConsole.AddLog("Waiting for players..");
			}
			CursorManager.roundStarted = true;
			RoundStart rs = RoundStart.singleton;
			if (TutorialManager.status)
			{
				this.ForceRoundStart();
			}
			else
			{
				rs.ShowButton();
				int timeLeft = 20;
				int maxPlayers = 1;
				while (rs.info != "started")
				{
					if (maxPlayers > 1)
					{
						int num = timeLeft;
						timeLeft = num - 1;
					}
					int count = PlayerManager.singleton.players.Length;
					if (count > maxPlayers)
					{
						maxPlayers = count;
						if (timeLeft < 5)
						{
							timeLeft = 5;
						}
						else if (timeLeft < 10)
						{
							timeLeft = 10;
						}
						else if (timeLeft < 15)
						{
							timeLeft = 15;
						}
						else
						{
							timeLeft = 20;
						}
						if (maxPlayers == NetworkManager.singleton.maxConnections)
						{
							timeLeft = 0;
						}
					}
					if (timeLeft > 0)
					{
						this.CmdUpdateStartText(timeLeft.ToString());
					}
					else
					{
						this.ForceRoundStart();
					}
					yield return new WaitForSeconds(1f);
				}
			}
			CursorManager.roundStarted = false;
			this.CmdStartRound();
			this.SetRandomRoles();
			rs = null;
			rs = null;
			rs = null;
		}
		else
		{
			while (!host.GetComponent<CharacterClassManager>().roundStarted)
			{
				yield return new WaitForEndOfFrame();
			}
			yield return new WaitForSeconds(2f);
			if (this.curClass < 0)
			{
				this.CallCmdSuicide(default(PlayerStats.HitInfo));
			}
		}
		int iteration = 0;
		for (;;)
		{
			GameObject[] plys = PlayerManager.singleton.players;
			if (iteration >= plys.Length)
			{
				yield return new WaitForSeconds(3f);
				iteration = 0;
			}
			try
			{
				plys[iteration].GetComponent<CharacterClassManager>().InitSCPs();
			}
			catch
			{
			}
			int num2 = iteration;
			iteration = num2 + 1;
			yield return new WaitForEndOfFrame();
			plys = null;
			plys = null;
			plys = null;
		}
		yield break;
	}

	// Token: 0x060001EC RID: 492
	[Client]
	[Command]
	public void CmdSuicide(PlayerStats.HitInfo hitInfo)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning("[Client] function 'System.Void CharacterClassManager::CmdSuicide(PlayerStats/HitInfo)' called on server");
			return;
		}
		hitInfo.amount = ((hitInfo.amount != 0f) ? hitInfo.amount : 999799f);
		base.GetComponent<PlayerStats>().HurtPlayer(hitInfo, base.gameObject);
	}

	// Token: 0x060001ED RID: 493
	public void ForceRoundStart()
	{
		ServerConsole.AddLog("New round has been started.");
		this.CmdUpdateStartText("started");
	}

	// Token: 0x060001EE RID: 494
	[ServerCallback]
	private void CmdUpdateStartText(string str)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		RoundStart.singleton.Networkinfo = str;
	}

	// Token: 0x060001EF RID: 495
	public void InitSCPs()
	{
		if (this.curClass != -1 && !TutorialManager.status)
		{
			Class c = this.klasy[this.curClass];
			this.scp049.Init(this.curClass, c);
			this.scp049_2.Init(this.curClass, c);
			this.scp079.Init(this.curClass, c);
			this.scp106.Init(this.curClass, c);
			this.scp173.Init(this.curClass, c);
		}
	}

	// Token: 0x060001F0 RID: 496
	public void RegisterEscape()
	{
		this.CallCmdRegisterEscape(base.gameObject);
	}

	// Token: 0x060001F1 RID: 497
	[Command(channel = 2)]
	private void CmdRegisterEscape(GameObject sender)
	{
		CharacterClassManager component = sender.GetComponent<CharacterClassManager>();
		if (Vector3.Distance(sender.transform.position, base.GetComponent<Escape>().worldPosition) < (float)(base.GetComponent<Escape>().radius * 2))
		{
			RoundSummary component2 = GameObject.Find("Host").GetComponent<RoundSummary>();
			if (this.klasy[component.curClass].team == Team.CDP)
			{
				component2.summary.classD_escaped++;
				this.SetClassID(8);
				base.GetComponent<PlayerStats>().SetHPAmount(this.klasy[8].maxHP);
			}
			if (this.klasy[component.curClass].team == Team.RSC)
			{
				component2.summary.scientists_escaped++;
				this.SetClassID(4);
				base.GetComponent<PlayerStats>().SetHPAmount(this.klasy[4].maxHP);
			}
		}
	}

	// Token: 0x060001F2 RID: 498
	public void ApplyProperties()
	{
		Class @class = this.klasy[this.curClass];
		this.InitSCPs();
		Inventory component = base.GetComponent<Inventory>();
		if (base.isLocalPlayer)
		{
			base.GetComponent<Radio>().UpdateClass();
			base.GetComponent<Handcuffs>().CallCmdTarget(null);
			base.GetComponent<Spectator>().Init();
			base.GetComponent<Searching>().Init(@class.team == Team.SCP | @class.team == Team.RIP);
		}
		if (TutorialManager.status || base.isLocalPlayer)
		{
			if (@class.team == Team.RIP)
			{
				if (base.isLocalPlayer)
				{
					base.GetComponent<WeaponManager>().DisableAllWeaponCameras();
					base.GetComponent<Inventory>().DropAll();
					component.items.Clear();
					component.NetworkcurItem = -1;
					base.GetComponent<FirstPersonController>().enabled = false;
					UnityEngine.Object.FindObjectOfType<StartScreen>().PlayAnimation(this.curClass);
					base.GetComponent<HorrorSoundController>().horrorSoundSource.PlayOneShot(this.bell_dead);
					base.transform.position = new Vector3(0f, 2048f, 0f);
					base.transform.rotation = Quaternion.Euler(Vector3.zero);
					base.GetComponent<PlayerStats>().maxHP = @class.maxHP;
					this.unfocusedCamera.GetComponent<Camera>().enabled = false;
					this.unfocusedCamera.GetComponent<PostProcessingBehaviour>().enabled = false;
				}
				this.RefreshPlyModel(-1);
			}
			else
			{
				if (base.isLocalPlayer)
				{
					base.GetComponent<Scp106PlayerScript>().SetDoors();
					GameObject randomPosition = UnityEngine.Object.FindObjectOfType<SpawnpointManager>().GetRandomPosition(this.curClass);
					if (randomPosition != null)
					{
						base.transform.position = randomPosition.transform.position;
						base.transform.rotation = randomPosition.transform.rotation;
					}
					else
					{
						base.transform.position = this.deathPosition;
					}
					component.items.Clear();
					component.NetworkcurItem = -1;
					foreach (int id in @class.startItems)
					{
						component.AddItem(id, -4.65664672E+11f);
					}
					UnityEngine.Object.FindObjectOfType<StartScreen>().PlayAnimation(this.curClass);
					if (!base.GetComponent<HorrorSoundController>().horrorSoundSource.isPlaying)
					{
						base.GetComponent<HorrorSoundController>().horrorSoundSource.PlayOneShot(this.bell);
					}
					base.Invoke("EnableFPC", 0.2f);
				}
				this.RefreshPlyModel(-1);
				if (base.isLocalPlayer)
				{
					base.GetComponent<Radio>().NetworkcurPreset = 0;
					base.GetComponent<Radio>().CallCmdUpdatePreset(0);
					base.GetComponent<AmmoBox>().SetAmmoAmount();
					FirstPersonController component3 = base.GetComponent<FirstPersonController>();
					PlayerStats component2 = base.GetComponent<PlayerStats>();
					if (@class.postprocessingProfile != null && base.GetComponentInChildren<PostProcessingBehaviour>() != null)
					{
						base.GetComponentInChildren<PostProcessingBehaviour>().profile = @class.postprocessingProfile;
					}
					this.unfocusedCamera.GetComponent<Camera>().enabled = true;
					this.unfocusedCamera.GetComponent<PostProcessingBehaviour>().enabled = true;
					component3.m_WalkSpeed = @class.walkSpeed;
					component3.m_RunSpeed = @class.runSpeed;
					component3.m_UseHeadBob = @class.useHeadBob;
					component3.m_FootstepSounds = @class.stepClips;
					component3.m_JumpSpeed = @class.jumpSpeed;
					base.GetComponent<WeaponManager>().SetRecoil(@class.classRecoil);
					int maxHP = @class.maxHP;
					component2.maxHP = maxHP;
					UnityEngine.Object.FindObjectOfType<UserMainInterface>().lerpedHP = (float)maxHP;
				}
				else
				{
					base.GetComponent<PlayerStats>().maxHP = @class.maxHP;
				}
			}
			if (base.isLocalPlayer)
			{
				UnityEngine.Object.FindObjectOfType<InventoryDisplay>().isSCP = (this.curClass == 2 | @class.team == Team.SCP);
				UnityEngine.Object.FindObjectOfType<InterfaceColorAdjuster>().ChangeColor(@class.classColor);
				return;
			}
		}
		else
		{
			this.RefreshPlyModel(-1);
		}
	}

	// Token: 0x060001F3 RID: 499
	private void EnableFPC()
	{
		base.GetComponent<FirstPersonController>().enabled = true;
	}

	// Token: 0x060001F4 RID: 500
	public void RefreshPlyModel(int classID = -1)
	{
		if (this.myModel != null)
		{
			UnityEngine.Object.Destroy(this.myModel);
		}
		Class @class = this.klasy[(classID >= 0) ? classID : this.curClass];
		if (@class.team != Team.RIP)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(@class.model_player);
			gameObject.transform.SetParent(base.gameObject.transform);
			gameObject.transform.localPosition = @class.model_offset.position;
			gameObject.transform.localRotation = Quaternion.Euler(@class.model_offset.rotation);
			gameObject.transform.localScale = @class.model_offset.scale;
			this.myModel = gameObject;
			if (this.myModel.GetComponent<Animator>() != null)
			{
				base.GetComponent<AnimationController>().animator = this.myModel.GetComponent<Animator>();
			}
			if (base.isLocalPlayer)
			{
				if (this.myModel.GetComponent<Renderer>() != null)
				{
					this.myModel.GetComponent<Renderer>().enabled = false;
				}
				Renderer[] componentsInChildren = this.myModel.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
				foreach (Collider collider in this.myModel.GetComponentsInChildren<Collider>())
				{
					if (collider.name != "LookingTarget")
					{
						collider.enabled = false;
					}
				}
			}
		}
		base.GetComponent<CapsuleCollider>().enabled = (@class.team != Team.RIP);
	}

	// Token: 0x060001F5 RID: 501
	public void SetClassID(int id)
	{
		this.NetworkcurClass = id;
		if (id != 2 || base.isLocalPlayer)
		{
			this.aliveTime = 0f;
			this.ApplyProperties();
		}
	}

	// Token: 0x060001F6 RID: 502
	public void InstantiateRagdoll(int id)
	{
		if (id < 0 || id == 7)
		{
			return;
		}
		Class @class = this.klasy[this.curClass];
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(@class.model_ragdoll);
		gameObject.transform.position = base.transform.position + @class.ragdoll_offset.position;
		gameObject.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles + @class.ragdoll_offset.rotation);
		gameObject.transform.localScale = @class.ragdoll_offset.scale;
	}

	// Token: 0x060001F7 RID: 503
	public void SetRandomRoles()
	{
		MTFRespawn component = base.GetComponent<MTFRespawn>();
		if (base.isLocalPlayer && base.isServer)
		{
			List<GameObject> list = new List<GameObject>();
			List<GameObject> list2 = new List<GameObject>();
			foreach (GameObject item in PlayerManager.singleton.players)
			{
				list.Add(item);
			}
			while (list.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				list2.Add(list[index]);
				list.RemoveAt(index);
			}
			GameObject[] array2 = list2.ToArray();
			RoundSummary component2 = base.GetComponent<RoundSummary>();
			bool flag = false;
			if ((float)UnityEngine.Random.Range(0, 100) < this.ciPercentage)
			{
				flag = true;
			}
			if (this.ban_computer_for_first_pick)
			{
				this.klasy[7].banClass = true;
			}
			this.first_scp = true;
			for (int i = 0; i < array2.Length; i++)
			{
				int num = (this.forceClass != -1) ? this.forceClass : this.Find_Random_ID_Using_Defined_Team(this.classTeamQueue[i]);
				if (this.klasy[num].team == Team.CDP)
				{
					component2.summary.classD_start++;
				}
				if (this.klasy[num].team == Team.RSC)
				{
					component2.summary.scientists_start++;
				}
				if (this.klasy[num].team == Team.SCP)
				{
					if (this.ban_computer_for_first_pick && this.first_scp)
					{
						this.klasy[7].banClass = false;
					}
					this.first_scp = false;
					component2.summary.scp_start++;
				}
				if (num == 4)
				{
					if (flag)
					{
						num = 8;
					}
					else
					{
						component.playersToNTF.Add(array2[i]);
					}
				}
				if (TutorialManager.status)
				{
					this.SetPlayersClass(14, base.gameObject);
				}
				else if (num != 4)
				{
					this.SetPlayersClass(num, array2[i]);
				}
			}
			component.SummonNTF();
		}
	}

	// Token: 0x060001F8 RID: 504
	private void SetRoundStart(bool b)
	{
		this.NetworkroundStarted = b;
	}

	// Token: 0x060001F9 RID: 505
	[ServerCallback]
	private void CmdStartRound()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (!TutorialManager.status)
		{
			Door componentInChildren = GameObject.Find("MeshDoor173").GetComponentInChildren<Door>();
			componentInChildren.curCooldown = 25f;
			componentInChildren.InvokeDeductCooldown();
			UnityEngine.Object.FindObjectOfType<ChopperAutostart>().SetState(false);
		}
		this.SetRoundStart(true);
	}

	// Token: 0x060001FA RID: 506
	[ServerCallback]
	public void SetPlayersClass(int classid, GameObject ply)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		ply.GetComponent<CharacterClassManager>().SetClassID(classid);
		ply.GetComponent<PlayerStats>().SetHPAmount(this.klasy[classid].maxHP);
	}

	// Token: 0x060001FB RID: 507
	private int Find_Random_ID_Using_Defined_Team(Team team)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < this.klasy.Length; i++)
		{
			if (this.klasy[i].team == team && !this.klasy[i].banClass)
			{
				list.Add(i);
			}
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		if (this.klasy[list[index]].team == Team.SCP)
		{
			this.klasy[list[index]].banClass = true;
		}
		return list[index];
	}

	// Token: 0x060001FC RID: 508
	public bool SpawnProtection()
	{
		return this.aliveTime < 2f;
	}

	// Token: 0x060001FD RID: 509
	private void Update()
	{
		if (this.curClass == 2)
		{
			this.aliveTime = 0f;
		}
		else
		{
			this.aliveTime += Time.deltaTime;
		}
		if (base.isLocalPlayer)
		{
			if (ServerStatic.isDedicated)
			{
				CursorManager.isServerOnly = true;
			}
			if (base.isServer)
			{
				this.AllowContain();
			}
		}
		if (this.prevId != this.curClass)
		{
			this.RefreshPlyModel(-1);
			this.prevId = this.curClass;
		}
		if (base.name == "Host")
		{
			Radio.roundStarted = this.roundStarted;
		}
	}

	// Token: 0x060001FE RID: 510
	private void UNetVersion()
	{
	}

	// Token: 0x17000027 RID: 39
	// (get) Token: 0x060001FF RID: 511
	// (set) Token: 0x06000200 RID: 512
	public int NetworkntfUnit
	{
		get
		{
			return this.ntfUnit;
		}
		set
		{
			uint dirtyBit = 1u;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				this.SetUnit(value);
				base.syncVarHookGuard = false;
			}
			base.SetSyncVar<int>(value, ref this.ntfUnit, dirtyBit);
		}
	}

	// Token: 0x17000028 RID: 40
	// (get) Token: 0x06000201 RID: 513
	// (set) Token: 0x06000202 RID: 514
	public int NetworkcurClass
	{
		get
		{
			return this.curClass;
		}
		set
		{
			uint dirtyBit = 2u;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				this.SetClassID(value);
				base.syncVarHookGuard = false;
			}
			base.SetSyncVar<int>(value, ref this.curClass, dirtyBit);
		}
	}

	// Token: 0x17000029 RID: 41
	// (get) Token: 0x06000203 RID: 515
	// (set) Token: 0x06000204 RID: 516
	public Vector3 NetworkdeathPosition
	{
		get
		{
			return this.deathPosition;
		}
		set
		{
			uint dirtyBit = 4u;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				this.SyncDeathPos(value);
				base.syncVarHookGuard = false;
			}
			base.SetSyncVar<Vector3>(value, ref this.deathPosition, dirtyBit);
		}
	}

	// Token: 0x1700002A RID: 42
	// (get) Token: 0x06000205 RID: 517
	// (set) Token: 0x06000206 RID: 518
	public bool NetworkroundStarted
	{
		get
		{
			return this.roundStarted;
		}
		set
		{
			uint dirtyBit = 8u;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				this.SetRoundStart(value);
				base.syncVarHookGuard = false;
			}
			base.SetSyncVar<bool>(value, ref this.roundStarted, dirtyBit);
		}
	}

	// Token: 0x06000207 RID: 519
	protected static void InvokeCmdCmdSuicide(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSuicide called on client.");
			return;
		}
		((CharacterClassManager)obj).CmdSuicide(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
	}

	// Token: 0x06000208 RID: 520
	protected static void InvokeCmdCmdRegisterEscape(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRegisterEscape called on client.");
			return;
		}
		((CharacterClassManager)obj).CmdRegisterEscape(reader.ReadGameObject());
	}

	// Token: 0x06000209 RID: 521
	public void CallCmdSuicide(PlayerStats.HitInfo hitInfo)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSuicide called on server.");
			return;
		}
		if (base.isServer)
		{
			this.CmdSuicide(hitInfo);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write(5);
		networkWriter.WritePackedUInt32((uint)CharacterClassManager.kCmdCmdSuicide);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteHitInfo_PlayerStats(networkWriter, hitInfo);
		base.SendCommandInternal(networkWriter, 0, "CmdSuicide");
	}

	// Token: 0x0600020A RID: 522
	public void CallCmdRegisterEscape(GameObject sender)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRegisterEscape called on server.");
			return;
		}
		if (base.isServer)
		{
			this.CmdRegisterEscape(sender);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write(5);
		networkWriter.WritePackedUInt32((uint)CharacterClassManager.kCmdCmdRegisterEscape);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(sender);
		base.SendCommandInternal(networkWriter, 2, "CmdRegisterEscape");
	}

	// Token: 0x0600020B RID: 523
	static CharacterClassManager()
	{
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), CharacterClassManager.kCmdCmdSuicide, new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdSuicide));
		CharacterClassManager.kCmdCmdRegisterEscape = -1826587486;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), CharacterClassManager.kCmdCmdRegisterEscape, new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdRegisterEscape));
		NetworkCRC.RegisterBehaviour("CharacterClassManager", 0);
	}

	// Token: 0x0600020C RID: 524
	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)this.ntfUnit);
			writer.WritePackedUInt32((uint)this.curClass);
			writer.Write(this.deathPosition);
			writer.Write(this.roundStarted);
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
			writer.WritePackedUInt32((uint)this.ntfUnit);
		}
		if ((base.syncVarDirtyBits & 2u) != 0u)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)this.curClass);
		}
		if ((base.syncVarDirtyBits & 4u) != 0u)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(this.deathPosition);
		}
		if ((base.syncVarDirtyBits & 8u) != 0u)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(this.roundStarted);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	// Token: 0x0600020D RID: 525
	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			this.ntfUnit = (int)reader.ReadPackedUInt32();
			this.curClass = (int)reader.ReadPackedUInt32();
			this.deathPosition = reader.ReadVector3();
			this.roundStarted = reader.ReadBoolean();
			return;
		}
		uint num = reader.ReadPackedUInt32();
		if ((num & 1u) != 0u)
		{
			this.SetUnit((int)reader.ReadPackedUInt32());
		}
		if ((num & 2u) != 0u)
		{
			this.SetClassID((int)reader.ReadPackedUInt32());
		}
		if ((num & 4u) != 0u)
		{
			this.SyncDeathPos(reader.ReadVector3());
		}
		if ((num & 8u) != 0u)
		{
			this.SetRoundStart(reader.ReadBoolean());
		}
	}

	// Token: 0x06000C57 RID: 3159
	public void SetMaxHP(int id, string config_key, int defaultHp)
	{
		Class c = this.klasy[id];
		c.maxHP = ConfigFile.GetInt(config_key, defaultHp);
		if (c.maxHP != defaultHp)
		{
			ServerConsole.AddLog(string.Concat(new object[]
			{
				"Set non-default hp for ",
				c.fullName,
				" with value ",
				c.maxHP
			}));
		}
	}

	// Token: 0x040001F3 RID: 499
	[SyncVar(hook = "SetUnit")]
	public int ntfUnit;

	// Token: 0x040001F4 RID: 500
	public float ciPercentage;

	// Token: 0x040001F5 RID: 501
	public int forceClass = -1;

	// Token: 0x040001F6 RID: 502
	[SerializeField]
	private AudioClip bell;

	// Token: 0x040001F7 RID: 503
	[SerializeField]
	private AudioClip bell_dead;

	// Token: 0x040001F8 RID: 504
	[HideInInspector]
	public GameObject myModel;

	// Token: 0x040001F9 RID: 505
	[HideInInspector]
	public GameObject charCamera;

	// Token: 0x040001FA RID: 506
	public Class[] klasy;

	// Token: 0x040001FB RID: 507
	public List<Team> classTeamQueue = new List<Team>();

	// Token: 0x040001FC RID: 508
	[SyncVar(hook = "SetClassID")]
	public int curClass;

	// Token: 0x040001FD RID: 509
	private int seed;

	// Token: 0x040001FE RID: 510
	private GameObject plyCam;

	// Token: 0x040001FF RID: 511
	public GameObject unfocusedCamera;

	// Token: 0x04000200 RID: 512
	[SyncVar(hook = "SyncDeathPos")]
	public Vector3 deathPosition;

	// Token: 0x04000201 RID: 513
	[SyncVar(hook = "SetRoundStart")]
	public bool roundStarted;

	// Token: 0x04000202 RID: 514
	private Scp049PlayerScript scp049;

	// Token: 0x04000203 RID: 515
	private Scp049_2PlayerScript scp049_2;

	// Token: 0x04000204 RID: 516
	private Scp079PlayerScript scp079;

	// Token: 0x04000205 RID: 517
	private Scp106PlayerScript scp106;

	// Token: 0x04000206 RID: 518
	private Scp173PlayerScript scp173;

	// Token: 0x04000207 RID: 519
	private LureSubjectContainer lureSpj;

	// Token: 0x04000208 RID: 520
	private float aliveTime;

	// Token: 0x04000209 RID: 521
	private int prevId = -1;

	// Token: 0x0400020A RID: 522
	private static int kCmdCmdSuicide = -1051695024;

	// Token: 0x0400020B RID: 523
	private static int kCmdCmdRegisterEscape;

	// Token: 0x04000C94 RID: 3220
	private bool first_scp;

	// Token: 0x04000C95 RID: 3221
	private bool ban_computer_for_first_pick;
}
