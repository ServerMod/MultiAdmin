using System;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x0200005D RID: 93
public class AlphaWarheadDetonationController : NetworkBehaviour
{
	// Token: 0x06000185 RID: 389
	public AlphaWarheadDetonationController()
	{
	}

	// Token: 0x06000186 RID: 390
	public void StartDetonation()
	{
		if (this.detonationInProgress || !this.lever.GetState())
		{
			return;
		}
		this.detonationInProgress = true;
		this.NetworkdetonationTime = 90f;
		this.doorsOpen = false;
		this.startTime = Time.time;
	}

	// Token: 0x06000187 RID: 391
	public void CancelDetonation()
	{
		float timeSinceStart = Time.time - this.startTime;
		if (this.detonationInProgress && this.detonationTime > 2f && timeSinceStart >= (float)this.cooldown)
		{
			this.detonationInProgress = false;
			this.NetworkdetonationTime = 0f;
		}
	}

	// Token: 0x06000188 RID: 392
	public void FixedUpdate()
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

	// Token: 0x06000189 RID: 393
	public void Explode()
	{
		this.detonated = true;
		this.ExplodePlayers();
	}

	// Token: 0x0600018A RID: 394
	[ServerCallback]
	public void CmdOpenDoors()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Door"))
		{
			if (gameObject.GetComponent<Door>().isAbleToEmergencyOpen())
			{
				gameObject.GetComponent<Door>().SetState(true);
			}
		}
	}

	// Token: 0x0600018B RID: 395
	[ServerCallback]
	public void ExplodePlayers()
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

	// Token: 0x0600018C RID: 396
	[ServerCallback]
	public void CmdCloseBlastDoors()
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

	// Token: 0x0600018D RID: 397
	[ClientCallback]
	public void TransmitData(float t)
	{
		if (!NetworkClient.active)
		{
			return;
		}
		this.CmdSyncData(t);
	}

	// Token: 0x0600018E RID: 398
	[ServerCallback]
	public void CmdSyncData(float t)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		this.NetworkdetonationTime = t;
	}

	// Token: 0x0600018F RID: 399
	public void Start()
	{
		this.cooldown = ConfigFile.GetInt("nuke_disable_cooldown", 0);
		if (this.cooldown != 0)
		{
			ServerConsole.AddLog("|SM| Nuke cooldown is on for " + this.cooldown + " seconds");
		}
		this.lever = GameObject.Find("Lever_Alpha_Controller").GetComponent<LeverButton>();
		this.lights = UnityEngine.Object.FindObjectsOfType<ToggleableLight>();
	}

	// Token: 0x06000190 RID: 400
	public void SetLights(bool b)
	{
		ToggleableLight[] array = this.lights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetLights(b);
		}
	}

	// Token: 0x06000191 RID: 401
	public void UNetVersion()
	{
	}

	// Token: 0x17000020 RID: 32
	// (get) Token: 0x06000192 RID: 402
	// (set) Token: 0x06000193 RID: 403
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

	// Token: 0x06000194 RID: 404
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

	// Token: 0x06000195 RID: 405
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

	// Token: 0x040001B1 RID: 433
	[SyncVar]
	public float detonationTime;

	// Token: 0x040001B2 RID: 434
	public bool detonationInProgress;

	// Token: 0x040001B3 RID: 435
	public bool detonated;

	// Token: 0x040001B4 RID: 436
	public bool doorsOpen;

	// Token: 0x040001B5 RID: 437
	public bool blastDoors;

	// Token: 0x040001B6 RID: 438
	public GameObject host;

	// Token: 0x040001B7 RID: 439
	public bool lightStatus;

	// Token: 0x040001B8 RID: 440
	public AWSoundController awsc;

	// Token: 0x040001B9 RID: 441
	public LeverButton lever;

	// Token: 0x040001BA RID: 442
	public AlphaWarheadDetonationController awdc;

	// Token: 0x040001BB RID: 443
	public ToggleableLight[] lights;

	// Token: 0x04000D1A RID: 3354
	public float startTime;

	// Token: 0x04000D26 RID: 3366
	public int cooldown;
}
