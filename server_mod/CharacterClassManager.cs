using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x02000070 RID: 112
public partial class CharacterClassManager : NetworkBehaviour
{
	// Token: 0x0600022B RID: 555
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
			if (this.smBan457)
			{
				this.klasy[9].banClass = true;
			}
			if (this.smBan106)
			{
				this.klasy[3].banClass = true;
			}


			this.smFirstPick = true;
			for (int i = 0; i < array.Length; i++)
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
						component.playersToNTF.Add(array[i]);
					}
				}
				if (TutorialManager.status)
				{
					this.SetPlayersClass(14, base.gameObject);
				}
				else if (num != 4)
				{
					this.SetPlayersClass(num, array[i]);
				}
			}
			component.SummonNTF();
		}
	}
}
