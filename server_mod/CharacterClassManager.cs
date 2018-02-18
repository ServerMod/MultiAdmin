using System;
using System.Collections;
using System.Collections.Generic;
using GameConsole;
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PostProcessing;

// Token: 0x02000078 RID: 120
public class CharacterClassManager : NetworkBehaviour
{
	// Token: 0x0600025C RID: 604 RVA: 0x000035F9 File Offset: 0x000017F9
	public void SetUnit(int unit)
	{
		this.NetworkntfUnit = unit;
	}

	// Token: 0x0600025D RID: 605 RVA: 0x00003602 File Offset: 0x00001802
	public void SyncDeathPos(Vector3 v)
	{
		this.NetworkdeathPosition = v;
	}

	// Token: 0x0600025E RID: 606 RVA: 0x00013648 File Offset: 0x00011848
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

	// Token: 0x0600025F RID: 607 RVA: 0x00013704 File Offset: 0x00011904
	private void Start()
	{
		if (base.isLocalPlayer)
		{
			for (int i = 0; i < this.klasy.Length; i++)
			{
				this.klasy[i].fullName = TranslationReader.Get("Class_Names", i);
				this.klasy[i].description = TranslationReader.Get("Class_Descriptions", i);
			}
			CharacterClassManager.staticClasses = this.klasy;
		}
		else if (CharacterClassManager.staticClasses == null || CharacterClassManager.staticClasses.Length == 0)
		{
			for (int j = 0; j < this.klasy.Length; j++)
			{
				this.klasy[j].description = TranslationReader.Get("Class_Descriptions", j);
				this.klasy[j].fullName = TranslationReader.Get("Class_Names", j);
			}
		}
		else
		{
			this.klasy = CharacterClassManager.staticClasses;
		}
		this.lureSpj = UnityEngine.Object.FindObjectOfType<LureSubjectContainer>();
		this.scp049 = base.GetComponent<Scp049PlayerScript>();
		this.scp049_2 = base.GetComponent<Scp049_2PlayerScript>();
		this.scp096 = base.GetComponent<Scp096PlayerScript>();
		this.scp079 = base.GetComponent<Scp079PlayerScript>();
		this.scp106 = base.GetComponent<Scp106PlayerScript>();
		this.scp173 = base.GetComponent<Scp173PlayerScript>();
		this.forceClass = ConfigFile.GetInt("server_forced_class", -1);
		this.smBanComputerFirstPick = (ConfigFile.GetString("NO_SCP079_FIRST", "true").ToLower() == "true");
		this.ciPercentage = (float)ConfigFile.GetInt("ci_on_start_percent", 10);
		this.smStartRoundTimer = ConfigFile.GetInt("START_ROUND_TIMER", 20);
		this.smWaitForPlayers = ConfigFile.GetInt("START_ROUND_MINIMUM_PLAYERS", 2) - 1;
		base.StartCoroutine("Init");
		string text = ConfigFile.GetString("team_respawn_queue", "401431403144144") + "...........................";
		this.classTeamQueue.Clear();
		for (int k = 0; k < text.Length; k++)
		{
			int item = 4;
			if (!int.TryParse(text[k].ToString(), out item))
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
		this.SetMaxHP(1, "CLASSD_HP", 100);
		this.SetMaxHP(3, "SCP106_HP", 700);
		this.SetMaxHP(4, "SCIENTIST_HP", 120); // For some reason this and "NTFSCIENTIST_HP" are swapped?
		this.SetMaxHP(5, "SCP049_HP", 1200);
		this.SetMaxHP(6, "NTFSCIENTIST_HP", 100);
		this.SetMaxHP(7, "SCP079_HP", 100);
		this.SetMaxHP(8, "CI_HP", 120);
		this.SetMaxHP(9, "SCP096_HP", 2000);
		this.SetMaxHP(10, "SCP049-2_HP", 400);
		this.SetMaxHP(11, "NTFL_HP", 120);
		this.SetMaxHP(12, "NTFC_HP", 150);
		this.SetMaxHP(13, "NTFG_HP", 100);
		this.smBan049 = !ConfigFile.GetString("SCP049_DISABLE", "no").Equals("no");
		this.smBan096 = !ConfigFile.GetString("SCP096_DISABLE", "no").Equals("no");
		this.smBan079 = !ConfigFile.GetString("SCP079_DISABLE", "yes").Equals("no");
		this.smBan106 = !ConfigFile.GetString("SCP106_DISABLE", "no").Equals("no");
		this.smBan173 = !ConfigFile.GetString("SCP173_DISABLE", "no").Equals("no");
		this.smBan457 = !ConfigFile.GetString("SCP457_DISABLE", "no").Equals("no");
	}

	// Token: 0x06000260 RID: 608 RVA: 0x0000360B File Offset: 0x0000180B
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
				int timeLeft = this.smStartRoundTimer;
				int maxPlayers = 1;
				while (rs.info != "started")
				{
					if (maxPlayers > this.smWaitForPlayers)
					{
						int num = timeLeft;
						timeLeft = num - 1;
					}
					int num2 = PlayerManager.singleton.players.Length;
					if (num2 > maxPlayers)
					{
						maxPlayers = num2;
						if (maxPlayers == NetworkManager.singleton.maxConnections)
						{
							timeLeft = 0;
						}
						else if (timeLeft % 5 > 0)
						{
							timeLeft = timeLeft / 5 * 5 + 5;
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
			int num = iteration;
			iteration = num + 1;
			yield return new WaitForEndOfFrame();
			plys = null;
			plys = null;
		}
		yield break;
	}

	// Token: 0x06000261 RID: 609 RVA: 0x00013AA4 File Offset: 0x00011CA4
	[Command]
	[Client]
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

	// Token: 0x06000262 RID: 610 RVA: 0x0000361A File Offset: 0x0000181A
	public void ForceRoundStart()
	{
		this.smRoundStartTime = Time.time;
		ServerConsole.AddLog("New round has been started.");
		this.CmdUpdateStartText("started");
	}

	// Token: 0x06000263 RID: 611 RVA: 0x00003631 File Offset: 0x00001831
	[ServerCallback]
	private void CmdUpdateStartText(string str)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		RoundStart.singleton.Networkinfo = str;
	}

	// Token: 0x06000264 RID: 612 RVA: 0x00013AF8 File Offset: 0x00011CF8
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

	// Token: 0x06000265 RID: 613 RVA: 0x00003646 File Offset: 0x00001846
	public void RegisterEscape()
	{
		this.CallCmdRegisterEscape(base.gameObject);
	}

	// Token: 0x06000266 RID: 614 RVA: 0x00013B80 File Offset: 0x00011D80
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

	// Token: 0x06000267 RID: 615 RVA: 0x00013C60 File Offset: 0x00011E60
	public void ApplyProperties()
	{
		Class @class = this.klasy[this.curClass];
		this.InitSCPs();
		Inventory component = base.GetComponent<Inventory>();
		try
		{
			base.GetComponent<FootstepSync>().SetLoundness(@class.team);
		}
		catch
		{
		}
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
					FirstPersonController component2 = base.GetComponent<FirstPersonController>();
					PlayerStats component3 = base.GetComponent<PlayerStats>();
					if (@class.postprocessingProfile != null && base.GetComponentInChildren<PostProcessingBehaviour>() != null)
					{
						base.GetComponentInChildren<PostProcessingBehaviour>().profile = @class.postprocessingProfile;
					}
					this.unfocusedCamera.GetComponent<Camera>().enabled = true;
					this.unfocusedCamera.GetComponent<PostProcessingBehaviour>().enabled = true;
					component2.m_WalkSpeed = @class.walkSpeed;
					component2.m_RunSpeed = @class.runSpeed;
					component2.m_UseHeadBob = @class.useHeadBob;
					component2.m_JumpSpeed = @class.jumpSpeed;
					base.GetComponent<WeaponManager>().SetRecoil(@class.classRecoil);
					int maxHP = @class.maxHP;
					component3.maxHP = maxHP;
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

	// Token: 0x06000268 RID: 616 RVA: 0x00003654 File Offset: 0x00001854
	private void EnableFPC()
	{
		base.GetComponent<FirstPersonController>().enabled = true;
	}

	// Token: 0x06000269 RID: 617 RVA: 0x00014024 File Offset: 0x00012224
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

	// Token: 0x0600026A RID: 618 RVA: 0x00003662 File Offset: 0x00001862
	public void SetClassID(int id)
	{
		this.NetworkcurClass = id;
		if (id != 2 || base.isLocalPlayer)
		{
			this.aliveTime = 0f;
			this.ApplyProperties();
		}
	}

	// Token: 0x0600026B RID: 619 RVA: 0x000141AC File Offset: 0x000123AC
	public void InstantiateRagdoll(int id)
	{
		if (id < 0)
		{
			return;
		}
		Class @class = this.klasy[this.curClass];
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(@class.model_ragdoll);
		gameObject.transform.position = base.transform.position + @class.ragdoll_offset.position;
		gameObject.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles + @class.ragdoll_offset.rotation);
		gameObject.transform.localScale = @class.ragdoll_offset.scale;
	}

	// Token: 0x0600026C RID: 620 RVA: 0x00014248 File Offset: 0x00012448
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
			GameObject[] array = list2.ToArray();
			RoundSummary component2 = base.GetComponent<RoundSummary>();
			bool flag = false;
			if ((float)UnityEngine.Random.Range(0, 100) < this.ciPercentage)
			{
				flag = true;
			}
			if (this.smBanComputerFirstPick || this.smBan079)
			{
				this.klasy[7].banClass = true;
			}
			if (this.smBan049)
			{
				this.klasy[5].banClass = true;
			}
			if (this.smBan173)
			{
				this.klasy[0].banClass = true;
			}
			if (this.smBan096)
			{
				this.klasy[9].banClass = true;
			}
			if (this.smBan106)
			{
				this.klasy[3].banClass = true;
			}
			for (int j = 0; j < array.Length; j++)
			{
				int num = (this.forceClass != -1) ? this.forceClass : this.Find_Random_ID_Using_Defined_Team(this.classTeamQueue[j]);
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
					component2.summary.scp_start++;
				}
				if (this.klasy[num].team == Team.SCP)
				{
					if (this.smBanComputerFirstPick && this.smFirstPick && !this.smBan079)
					{
						this.klasy[7].banClass = false;
					}
					this.smFirstPick = false;
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
						component.playersToNTF.Add(array[j]);
					}
				}
				if (TutorialManager.status)
				{
					this.SetPlayersClass(14, base.gameObject);
				}
				else if (num != 4)
				{
					this.SetPlayersClass(num, array[j]);
				}
			}
			component.SummonNTF();
		}
	}

	// Token: 0x0600026D RID: 621 RVA: 0x00003688 File Offset: 0x00001888
	private void SetRoundStart(bool b)
	{
		this.NetworkroundStarted = b;
	}

	// Token: 0x0600026E RID: 622 RVA: 0x000144C0 File Offset: 0x000126C0
	[ServerCallback]
	private void CmdStartRound()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (!TutorialManager.status)
		{
			try
			{
				GameObject.Find("MeshDoor173").GetComponentInChildren<Door>().ForceCooldown((float)ConfigFile.GetInt("173_door_starting_cooldown", 25));
				UnityEngine.Object.FindObjectOfType<ChopperAutostart>().SetState(false);
			}
			catch
			{
			}
		}
		this.SetRoundStart(true);
	}

	// Token: 0x0600026F RID: 623 RVA: 0x00003691 File Offset: 0x00001891
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

	// Token: 0x06000270 RID: 624 RVA: 0x00014524 File Offset: 0x00012724
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
		if (list.Count == 0)
		{
			return 1;
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		if (this.klasy[list[index]].team == Team.SCP)
		{
			this.klasy[list[index]].banClass = true;
		}
		return list[index];
	}

	// Token: 0x06000271 RID: 625 RVA: 0x000036BF File Offset: 0x000018BF
	public bool SpawnProtection()
	{
		return this.aliveTime < 2f;
	}

	// Token: 0x06000272 RID: 626 RVA: 0x000145B8 File Offset: 0x000127B8
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

	// Token: 0x06000273 RID: 627 RVA: 0x00002195 File Offset: 0x00000395
	private void UNetVersion()
	{
	}

	// Token: 0x17000039 RID: 57
	// (get) Token: 0x06000274 RID: 628 RVA: 0x000036CE File Offset: 0x000018CE
	// (set) Token: 0x06000275 RID: 629 RVA: 0x00014650 File Offset: 0x00012850
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

	// Token: 0x1700003A RID: 58
	// (get) Token: 0x06000276 RID: 630 RVA: 0x000036D6 File Offset: 0x000018D6
	// (set) Token: 0x06000277 RID: 631 RVA: 0x00014694 File Offset: 0x00012894
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

	// Token: 0x1700003B RID: 59
	// (get) Token: 0x06000278 RID: 632 RVA: 0x000036DE File Offset: 0x000018DE
	// (set) Token: 0x06000279 RID: 633 RVA: 0x000146D8 File Offset: 0x000128D8
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

	// Token: 0x1700003C RID: 60
	// (get) Token: 0x0600027A RID: 634 RVA: 0x000036E6 File Offset: 0x000018E6
	// (set) Token: 0x0600027B RID: 635 RVA: 0x0001471C File Offset: 0x0001291C
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

	// Token: 0x0600027C RID: 636 RVA: 0x000036EE File Offset: 0x000018EE
	protected static void InvokeCmdCmdSuicide(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSuicide called on client.");
			return;
		}
		((CharacterClassManager)obj).CmdSuicide(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
	}

	// Token: 0x0600027D RID: 637 RVA: 0x00003713 File Offset: 0x00001913
	protected static void InvokeCmdCmdRegisterEscape(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRegisterEscape called on client.");
			return;
		}
		((CharacterClassManager)obj).CmdRegisterEscape(reader.ReadGameObject());
	}

	// Token: 0x0600027E RID: 638 RVA: 0x00014760 File Offset: 0x00012960
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

	// Token: 0x0600027F RID: 639 RVA: 0x000147D4 File Offset: 0x000129D4
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

	// Token: 0x06000280 RID: 640 RVA: 0x00014848 File Offset: 0x00012A48
	static CharacterClassManager()
	{
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), CharacterClassManager.kCmdCmdSuicide, new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdSuicide));
		CharacterClassManager.kCmdCmdRegisterEscape = -1826587486;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), CharacterClassManager.kCmdCmdRegisterEscape, new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdRegisterEscape));
		NetworkCRC.RegisterBehaviour("CharacterClassManager", 0);
	}

	// Token: 0x06000281 RID: 641 RVA: 0x000148B4 File Offset: 0x00012AB4
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

	// Token: 0x06000282 RID: 642 RVA: 0x000149A4 File Offset: 0x00012BA4
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

	// Token: 0x06000283 RID: 643 RVA: 0x00003738 File Offset: 0x00001938
	public void SetMaxHP(int id, string config_key, int defaultHp)
	{
		this.klasy[id].maxHP = ConfigFile.GetInt(config_key, defaultHp);
	}

	// Token: 0x04000272 RID: 626
	[SyncVar(hook = "SetUnit")]
	public int ntfUnit;

	// Token: 0x04000273 RID: 627
	public float ciPercentage;

	// Token: 0x04000274 RID: 628
	public int forceClass = -1;

	// Token: 0x04000275 RID: 629
	[SerializeField]
	private AudioClip bell;

	// Token: 0x04000276 RID: 630
	[SerializeField]
	private AudioClip bell_dead;

	// Token: 0x04000277 RID: 631
	[HideInInspector]
	public GameObject myModel;

	// Token: 0x04000278 RID: 632
	[HideInInspector]
	public GameObject charCamera;

	// Token: 0x04000279 RID: 633
	public Class[] klasy;

	// Token: 0x0400027A RID: 634
	public List<Team> classTeamQueue = new List<Team>();

	// Token: 0x0400027B RID: 635
	[SyncVar(hook = "SetClassID")]
	public int curClass;

	// Token: 0x0400027C RID: 636
	private int seed;

	// Token: 0x0400027D RID: 637
	private GameObject plyCam;

	// Token: 0x0400027E RID: 638
	public GameObject unfocusedCamera;

	// Token: 0x0400027F RID: 639
	[SyncVar(hook = "SyncDeathPos")]
	public Vector3 deathPosition;

	// Token: 0x04000280 RID: 640
	[SyncVar(hook = "SetRoundStart")]
	public bool roundStarted;

	// Token: 0x04000281 RID: 641
	private Scp049PlayerScript scp049;

	// Token: 0x04000282 RID: 642
	private Scp049_2PlayerScript scp049_2;

	// Token: 0x04000283 RID: 643
	private Scp079PlayerScript scp079;

	// Token: 0x04000284 RID: 644
	private Scp106PlayerScript scp106;

	// Token: 0x04000285 RID: 645
	private Scp173PlayerScript scp173;

	// Token: 0x04000286 RID: 646
	private Scp096PlayerScript scp096;

	// Token: 0x04000287 RID: 647
	private LureSubjectContainer lureSpj;

	// Token: 0x04000288 RID: 648
	private static Class[] staticClasses;

	// Token: 0x04000289 RID: 649
	private float aliveTime;

	// Token: 0x0400028A RID: 650
	private int prevId = -1;

	// Token: 0x0400028B RID: 651
	private static int kCmdCmdSuicide = -1051695024;

	// Token: 0x0400028C RID: 652
	private static int kCmdCmdRegisterEscape;

	// Token: 0x0400028D RID: 653
	public bool smBanComputerFirstPick;

	// Token: 0x0400028E RID: 654
	public bool smBan049;

	// Token: 0x0400028F RID: 655
	public bool smBan096;

	// Token: 0x04000290 RID: 656
	public bool smBan079;

	// Token: 0x04000291 RID: 657
	public bool smBan106;

	// Token: 0x04000292 RID: 658
	public bool smBan173;

	// Token: 0x04000293 RID: 659
	public bool smBan457;

	// Token: 0x04000294 RID: 660
	public bool smFirstPick;

	// Token: 0x04000295 RID: 661
	private int smStartRoundTimer;

	// Token: 0x04000296 RID: 662
	private int smWaitForPlayers;

	// Token: 0x04000297 RID: 663
	public float smRoundStartTime;
}
