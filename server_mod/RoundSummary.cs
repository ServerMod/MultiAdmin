using System;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x020000C0 RID: 192
public partial class RoundSummary : NetworkBehaviour
{
	// Token: 0x060004C3 RID: 1219 RVA: 0x0001A068 File Offset: 0x00018268
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
			foreach (GameObject gameObject in players)
			{
				CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
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
}
