using System;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x02000065 RID: 101
public class AlphaWarheadDetonationController : NetworkBehaviour
{
	// Token: 0x060001B7 RID: 439
	public void StartDetonation()
	{
		if (this.detonationInProgress || !this.lever.GetState())
		{
			return;
		}
		this.detonationInProgress = true;
		this.NetworkdetonationTime = 90f;
		this.doorsOpen = false;
		this.smStartTime = Time.time;
	}

	// Token: 0x060001B8 RID: 440
	public void CancelDetonation()
	{
		float timeSinceStart = Time.time - this.smStartTime;
		if (this.detonationInProgress && this.detonationTime > 2f && timeSinceStart >= (float)this.smCooldown)
		{
			this.detonationInProgress = false;
			this.NetworkdetonationTime = 0f;
		}
	}

	// Token: 0x060001B9 RID: 441
	private void FixedUpdate()
	{
		if (base.isLocalPlayer && this.awdc != null && this.lightStatus != (this.awdc.detonationTime != 0f))
		{
			this.lightStatus = (this.awdc.detonationTime != 0f);
			this.SetLights(this.lightStatus);
		}
		if (base.name == "Host")
		{
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
					this.CmdOpenDoors();
				}
				if (this.detonationTime < 2f && !this.blastDoors && this.detonationInProgress && base.isLocalPlayer)
				{
					this.blastDoors = true;
					this.CmdCloseBlastDoors();
				}
			}
			else
			{
				if (this.detonationTime < 0f)
				{
					base.GetComponent<RoundSummary>().summary.warheadDetonated = true;
					this.Explode();
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

	// Token: 0x060001BA RID: 442
	private void Explode()
	{
		this.detonated = true;
		this.ExplodePlayers();
	}

	// Token: 0x060001BB RID: 443
	[ServerCallback]
	private void CmdOpenDoors()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
		{
			if (door.isOpen)
			{
				door.GetComponent<Door>().SetState(true);
			}
		}
	}

	// Token: 0x060001BC RID: 444
	[ServerCallback]
	private void ExplodePlayers()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		for (int i = 0; i < players.Length; i++)
		{
			players[i].GetComponent<PlayerStats>().Explode();
		}
	}

	// Token: 0x060001BD RID: 445
	[ServerCallback]
	private void CmdCloseBlastDoors()
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

	// Token: 0x060001BE RID: 446
	[ClientCallback]
	private void TransmitData(float t)
	{
		if (!NetworkClient.active)
		{
			return;
		}
		this.CmdSyncData(t);
	}

	// Token: 0x060001BF RID: 447
	[ServerCallback]
	private void CmdSyncData(float t)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		this.NetworkdetonationTime = t;
	}

	// Token: 0x060001C0 RID: 448
	private void Start()
	{
		this.smCooldown = ConfigFile.GetInt("nuke_disable_cooldown", 0);
		if (!TutorialManager.status)
		{
			this.lever = GameObject.Find("Lever_Alpha_Controller").GetComponent<LeverButton>();
			this.lights = UnityEngine.Object.FindObjectsOfType<ToggleableLight>();
		}
	}

	// Token: 0x060001C1 RID: 449
	private void SetLights(bool b)
	{
		ToggleableLight[] array = this.lights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetLights(b);
		}
	}

	// Token: 0x060001C2 RID: 450
	private void UNetVersion()
	{
	}

	// Token: 0x17000029 RID: 41
	// (get) Token: 0x060001C3 RID: 451
	// (set) Token: 0x060001C4 RID: 452
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

	// Token: 0x060001C5 RID: 453
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

	// Token: 0x060001C6 RID: 454
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

	// Token: 0x040001F1 RID: 497
	[SyncVar]
	public float detonationTime;

	// Token: 0x040001F2 RID: 498
	private bool detonationInProgress;

	// Token: 0x040001F3 RID: 499
	private bool detonated;

	// Token: 0x040001F4 RID: 500
	private bool doorsOpen;

	// Token: 0x040001F5 RID: 501
	private bool blastDoors;

	// Token: 0x040001F6 RID: 502
	private GameObject host;

	// Token: 0x040001F7 RID: 503
	private bool lightStatus;

	// Token: 0x040001F8 RID: 504
	private AWSoundController awsc;

	// Token: 0x040001F9 RID: 505
	private LeverButton lever;

	// Token: 0x040001FA RID: 506
	private AlphaWarheadDetonationController awdc;

	// Token: 0x040001FB RID: 507
	private ToggleableLight[] lights;

	// Token: 0x04000BF4 RID: 3060
	private float smStartTime;

	// Token: 0x04000BF5 RID: 3061
	private int smCooldown;
}
