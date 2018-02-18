using System;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x0200006D RID: 109
public class AlphaWarheadDetonationController : NetworkBehaviour
{
	// Token: 0x060001F8 RID: 504
	public void StartDetonation()
	{
		float elapsed = Time.time - this.smCharacterClassManager.smRoundStartTime;
		if (this.detonationInProgress || !this.lever.GetState() || elapsed < (float)this.smNukeActivationMinTime)
		{
			return;
		}
		this.detonationInProgress = true;
		this.NetworkdetonationTime = 90f;
		this.doorsOpen = false;
		this.smStartTime = Time.time;
	}

	// Token: 0x060001F9 RID: 505
	public void CancelDetonation()
	{
		float elapsed = Time.time - this.smStartTime;
		if (this.detonationInProgress && this.detonationTime > 2f && elapsed >= (float)this.smCooldown)
		{
			this.detonationInProgress = false;
			this.NetworkdetonationTime = 0f;
		}
	}

	// Token: 0x060001FA RID: 506 RVA: 0x00011D20 File Offset: 0x0000FF20
	private void FixedUpdate()
	{
		if (base.isLocalPlayer && this.awdc != null && this.lightStatus != (this.awdc.detonationTime != 0f))
		{
			this.lightStatus = (this.awdc.detonationTime != 0f);
			this.SetLights(this.lightStatus);
		}
		if (base.name == "Host")
		{
			if (this.detonated)
			{
				this.ExplodePlayers();
			}
			if (this.detonationTime > 0f)
			{
				this.NetworkdetonationTime = this.detonationTime - Time.deltaTime;
				if (!this.lever.GetState())
				{
					this.CancelDetonation();
				}
				if (this.detonationTime < 83f && !this.doorsOpen && base.isLocalPlayer)
				{
					this.doorsOpen = true;
					this.OpenDoors();
				}
				if (this.detonationTime < 2f && !this.blastDoors && this.detonationInProgress && base.isLocalPlayer)
				{
					this.blastDoors = true;
					this.CloseBlastDoors();
				}
			}
			else
			{
				if (this.detonationTime < 0f)
				{
					base.GetComponent<RoundSummary>().summary.warheadDetonated = true;
					this.Explode();
					this.smDetonated = true;
				}
				this.NetworkdetonationTime = 0f;
			}
		}
		if (base.isLocalPlayer && base.isServer)
		{
			this.TransmitData(this.detonationTime);
		}
		if (this.awsc == null || this.awdc == null)
		{
			this.awsc = UnityEngine.Object.FindObjectOfType<AWSoundController>();
			if (this.host == null)
			{
				this.host = GameObject.Find("Host");
			}
			if (this.host != null)
			{
				this.awdc = this.host.GetComponent<AlphaWarheadDetonationController>();
				return;
			}
		}
		else
		{
			this.awsc.UpdateSound(90f - this.awdc.detonationTime, this.detonated);
		}
	}

	// Token: 0x060001FB RID: 507 RVA: 0x00003298 File Offset: 0x00001498
	private void Explode()
	{
		this.detonated = true;
		this.ExplodePlayers();
	}

	// Token: 0x060001FC RID: 508 RVA: 0x00011F18 File Offset: 0x00010118
	[ServerCallback]
	private void OpenDoors()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
		{
			if (!door.isOpen && !door.permissionLevel.Contains("CONT") && door.permissionLevel != "UNACCESSIBLE")
			{
				door.OpenWarhead();
			}
		}
	}

	// Token: 0x060001FD RID: 509 RVA: 0x00011F78 File Offset: 0x00010178
	[ServerCallback]
	private void ExplodePlayers()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("LiftTarget");
		foreach (GameObject gameObject in PlayerManager.singleton.players)
		{
			foreach (GameObject gameObject2 in array)
			{
				gameObject.GetComponent<PlayerStats>().Explode(Vector3.Distance(gameObject2.transform.position, gameObject.transform.position) < 3.5f);
			}
		}
	}

	// Token: 0x060001FE RID: 510 RVA: 0x00012000 File Offset: 0x00010200
	[ServerCallback]
	private void CloseBlastDoors()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		BlastDoor[] array = UnityEngine.Object.FindObjectsOfType<BlastDoor>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetClosed(true);
		}
	}

	// Token: 0x060001FF RID: 511 RVA: 0x000032A7 File Offset: 0x000014A7
	[ClientCallback]
	private void TransmitData(float t)
	{
		if (!NetworkClient.active)
		{
			return;
		}
		this.CmdSyncData(t);
	}

	// Token: 0x06000200 RID: 512 RVA: 0x000032B8 File Offset: 0x000014B8
	[ServerCallback]
	private void CmdSyncData(float t)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		this.NetworkdetonationTime = t;
	}

	// Token: 0x06000201 RID: 513 RVA: 0x00012034 File Offset: 0x00010234
	private void Start()
	{
		this.smCharacterClassManager = base.GetComponent<CharacterClassManager>();
		this.smCooldown = ConfigFile.GetInt("nuke_disable_cooldown", 0);
		this.smNukeActivationMinTime = ConfigFile.GetInt("nuke_min_time", 0);
		this.smDetonated = false;
		if (!TutorialManager.status)
		{
			this.lever = GameObject.Find("Lever_Alpha_Controller").GetComponent<LeverButton>();
			this.lights = UnityEngine.Object.FindObjectsOfType<ToggleableLight>();
		}
	}

	// Token: 0x06000202 RID: 514 RVA: 0x000120A0 File Offset: 0x000102A0
	private void SetLights(bool b)
	{
		ToggleableLight[] array = this.lights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetLights(b);
		}
	}

	// Token: 0x06000203 RID: 515 RVA: 0x00002195 File Offset: 0x00000395
	private void UNetVersion()
	{
	}

	// Token: 0x17000032 RID: 50
	// (get) Token: 0x06000204 RID: 516 RVA: 0x000032C9 File Offset: 0x000014C9
	// (set) Token: 0x06000205 RID: 517 RVA: 0x000032D1 File Offset: 0x000014D1
	public float NetworkdetonationTime
	{
		get
		{
			return this.detonationTime;
		}
		set
		{
			base.SetSyncVar<float>(value, ref this.detonationTime, 1u);
		}
	}

	// Token: 0x06000206 RID: 518 RVA: 0x000120CC File Offset: 0x000102CC
	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(this.detonationTime);
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
			writer.Write(this.detonationTime);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	// Token: 0x06000207 RID: 519 RVA: 0x000032E1 File Offset: 0x000014E1
	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			this.detonationTime = reader.ReadSingle();
			return;
		}
		if ((reader.ReadPackedUInt32() & 1u) != 0u)
		{
			this.detonationTime = reader.ReadSingle();
		}
	}

	// Token: 0x0400022B RID: 555
	[SyncVar]
	public float detonationTime;

	// Token: 0x0400022C RID: 556
	private bool detonationInProgress;

	// Token: 0x0400022D RID: 557
	private bool detonated;

	// Token: 0x0400022E RID: 558
	private bool doorsOpen;

	// Token: 0x0400022F RID: 559
	private bool blastDoors;

	// Token: 0x04000230 RID: 560
	private GameObject host;

	// Token: 0x04000231 RID: 561
	private bool lightStatus;

	// Token: 0x04000232 RID: 562
	private AWSoundController awsc;

	// Token: 0x04000233 RID: 563
	private LeverButton lever;

	// Token: 0x04000234 RID: 564
	private AlphaWarheadDetonationController awdc;

	// Token: 0x04000235 RID: 565
	private ToggleableLight[] lights;

	// Token: 0x04000236 RID: 566
	private float smStartTime;

	// Token: 0x04000237 RID: 567
	private int smCooldown;

	// Token: 0x04000238 RID: 568
	private int smNukeActivationMinTime;

	// Token: 0x04000239 RID: 569
	private CharacterClassManager smCharacterClassManager;

	// Token: 0x0400023A RID: 570
	public bool smDetonated;
}
