using System;
using UnityEngine;
using UnityEngine.Networking;

public class AlphaWarheadDetonationController : NetworkBehaviour
{
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

	public void CancelDetonation()
	{
		float elapsed = Time.time - this.smStartTime;
		if (this.detonationInProgress && this.detonationTime > 2f && elapsed >= (float)this.smCooldown)
		{
			this.detonationInProgress = false;
			this.NetworkdetonationTime = 0f;
		}
	}

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
				this.detonated = false;
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

	private void Explode()
	{
		this.detonated = true;
		this.ExplodePlayers();
	}

	[ServerCallback]
	private void CmdOpenDoors()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
		{
            /*
             * Old version is missing the checks for
             * 
             * ... !door.permissionLevel.Contains("CONT") && door.permissionLevel != "UNACCESSIBLE" ...
             */
            if (!door.isOpen && !door.permissionLevel.Contains("CONT") && door.permissionLevel != "UNACCESSIBLE")
			{
                /*
                 * New update uses this code, is it a more standardized / labelled way of opening the warhead room door?
                 * 
                 * door.OpenWarhead();
                 */
                door.GetComponent<Door>().SetState(true);
			}
		}
	}

	[ServerCallback]
	private void ExplodePlayers()
	{
		if (!NetworkServer.active)
		{
			return;
		}

        /*
         * New update uses this code, try and find what difference it makes?
         * 
         * GameObject[] array = GameObject.FindGameObjectsWithTag("LiftTarget");
		 * foreach (GameObject gameObject in PlayerManager.singleton.players)
         * {
         *  foreach (GameObject gameObject2 in array)
		 *	{
		 *		gameObject.GetComponent<PlayerStats>().Explode(Vector3.Distance(gameObject2.transform.position, gameObject.transform.position) < 3.5f);
		 *	}
         * }
         */

        GameObject[] array = GameObject.FindGameObjectsWithTag("LiftTarget");
		foreach (GameObject player in PlayerManager.singleton.players)
		{
			foreach (GameObject lift in array)
			{
				player.GetComponent<PlayerStats>().Explode(Vector3.Distance(lift.transform.position, player.transform.position) < 3.5f);
			}
		}
	}

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

	[ClientCallback]
	private void TransmitData(float t)
	{
		if (!NetworkClient.active)
		{
			return;
		}
		this.CmdSyncData(t);
	}

	[ServerCallback]
	private void CmdSyncData(float t)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		this.NetworkdetonationTime = t;
	}

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

	private void SetLights(bool b)
	{
		ToggleableLight[] array = this.lights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetLights(b);
		}
	}

	private void UNetVersion()
	{
	}

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

	[SyncVar]
	public float detonationTime;

	private bool detonationInProgress;

	private bool detonated;

	private bool doorsOpen;

	private bool blastDoors;

	private GameObject host;

	private bool lightStatus;

	private AWSoundController awsc;

	private LeverButton lever;

	private AlphaWarheadDetonationController awdc;

	private ToggleableLight[] lights;

	private float smStartTime;

	private int smCooldown;

	private int smNukeActivationMinTime;

	private CharacterClassManager smCharacterClassManager;

	public bool smDetonated;
}
