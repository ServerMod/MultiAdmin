using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// Token: 0x0200008A RID: 138
public class Inventory : NetworkBehaviour
{
	// Token: 0x06000309 RID: 777 RVA: 0x00003EF2 File Offset: 0x000020F2
	public Inventory()
	{
	}

	// Token: 0x0600030A RID: 778 RVA: 0x00003F18 File Offset: 0x00002118
	public void SyncVerItems(SyncListInt i)
	{
		this.verifiedItems = i;
	}

	// Token: 0x0600030B RID: 779 RVA: 0x00016200 File Offset: 0x00014400
	public void Awake()
	{
		for (int i = 0; i < this.availableItems.Length; i++)
		{
			this.availableItems[i].id = i;
		}
		this.verifiedItems.InitializeBehaviour(this, Inventory.kListverifiedItems);
	}

	// Token: 0x0600030C RID: 780 RVA: 0x000020C4 File Offset: 0x000002C4
	public void Log(string msg)
	{
	}

	// Token: 0x0600030D RID: 781 RVA: 0x00003F21 File Offset: 0x00002121
	public void SetCurItem(int ci)
	{
		if (base.GetComponent<MicroHID_GFX>().onFire)
		{
			return;
		}
		this.NetworkcurItem = ci;
	}

	// Token: 0x0600030E RID: 782 RVA: 0x00016248 File Offset: 0x00014448
	public void Start()
	{
		if (base.isLocalPlayer && base.isServer)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("Pickup");
			foreach (GameObject gameObject in array)
			{
				gameObject.GetComponent<Pickup>().iCanSeeThatAsHost = true;
			}
		}
		this.ccm = base.GetComponent<CharacterClassManager>();
		this.crosshair = GameObject.Find("CrosshairImage").GetComponent<RawImage>();
		this.ac = base.GetComponent<AnimationController>();
		if (base.isLocalPlayer)
		{
			UnityEngine.Object.FindObjectOfType<InventoryDisplay>().localplayer = base.gameObject;
		}
	}

	// Token: 0x0600030F RID: 783 RVA: 0x000162E4 File Offset: 0x000144E4
	public void RefreshModels()
	{
		for (int i = 0; i < this.availableItems.Length; i++)
		{
			this.availableItems[i].firstpersonModel.SetActive(base.isLocalPlayer & i == this.curItem);
		}
	}

	// Token: 0x06000310 RID: 784 RVA: 0x0001632C File Offset: 0x0001452C
	public void DropItem(int id)
	{
		if (base.isLocalPlayer)
		{
			if (this.items[id].id == this.curItem)
			{
				this.NetworkcurItem = -1;
			}
			this.CallCmdSetPickup(this.items[id].id, this.items[id].durability, base.transform.position, this.kamera.transform.rotation, base.transform.rotation);
			this.items.RemoveAt(id);
		}
	}

	// Token: 0x06000311 RID: 785 RVA: 0x000163BC File Offset: 0x000145BC
	public void DropAll()
	{
		for (int i = 0; i < 20; i++)
		{
			if (this.items.Count > 0)
			{
				this.DropItem(0);
			}
		}
		AmmoBox component = base.GetComponent<AmmoBox>();
		for (int j = 0; j < component.types.Length; j++)
		{
			if (component.types[j].quantity > 0)
			{
				this.CallCmdSetPickup(component.types[j].inventoryID, (float)component.types[j].quantity, base.transform.position, this.kamera.transform.rotation, base.transform.rotation);
				component.types[j].quantity = 0;
			}
		}
	}

	// Token: 0x06000312 RID: 786 RVA: 0x0001646C File Offset: 0x0001466C
	public void AddItem(int id, float dur = -4.65664672E+11f)
	{
		if (base.isLocalPlayer)
		{
			if (TutorialManager.status)
			{
				PickupTrigger[] array = UnityEngine.Object.FindObjectsOfType<PickupTrigger>();
				PickupTrigger pickupTrigger = null;
				foreach (PickupTrigger pickupTrigger2 in array)
				{
					if ((pickupTrigger2.filter == -1 || pickupTrigger2.filter == id) && (pickupTrigger == null || pickupTrigger2.prioirty < pickupTrigger.prioirty))
					{
						pickupTrigger = pickupTrigger2;
					}
				}
				try
				{
					if (pickupTrigger != null)
					{
						pickupTrigger.Trigger(id);
					}
				}
				catch
				{
					MonoBehaviour.print("Error");
				}
			}
			Item item = new Item(this.availableItems[id]);
			if (base.GetComponent<Inventory>().items.Count < 8 || item.noEquipable)
			{
				if (dur != -4.65664672E+11f)
				{
					item.durability = dur;
				}
				this.items.Add(item);
			}
			else
			{
				base.GetComponent<Searching>().ShowErrorMessage();
			}
		}
	}

	// Token: 0x06000313 RID: 787 RVA: 0x00016588 File Offset: 0x00014788
	public void Update()
	{
		if (TutorialManager.status && !base.isLocalPlayer)
		{
			this.ac.SyncItem(this.curItem);
		}
		if (base.isLocalPlayer)
		{
			this.ac.SyncItem(this.curItem);
			int num = Mathf.Clamp(this.curItem, 0, this.availableItems.Length - 1);
			if (this.ccm.curClass >= 0 && this.ccm.klasy[this.ccm.curClass].forcedCrosshair != -1)
			{
				num = this.ccm.klasy[this.ccm.curClass].forcedCrosshair;
			}
			this.crosshair.texture = this.availableItems[num].crosshair;
			this.crosshair.color = this.availableItems[num].crosshairColor;
		}
		if (this.prevIt != this.curItem)
		{
			this.RefreshModels();
			this.prevIt = this.curItem;
		}
	}

	// Token: 0x06000314 RID: 788 RVA: 0x00016694 File Offset: 0x00014894
	[Command(channel = 2)]
	public void CmdSetPickup(int dropedItemID, float dur, Vector3 pos, Quaternion camRot, Quaternion myRot)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.pickupPrefab);
		NetworkServer.Spawn(gameObject);
		gameObject.GetComponent<Pickup>().SetDurability(dur);
		gameObject.GetComponent<Pickup>().SetID(dropedItemID);
		gameObject.GetComponent<Pickup>().SetPosition(((this.ccm.curClass != 2) ? pos : this.ccm.deathPosition) + Vector3.up * 0.9f);
		gameObject.GetComponent<Pickup>().SetRotation(new Vector3(camRot.eulerAngles.x, myRot.eulerAngles.y, 0f));
		gameObject.GetComponent<Pickup>().SetName(string.Concat(new object[]
		{
			"PICKUP#",
			dropedItemID,
			":",
			UnityEngine.Random.Range(0f, 1E+10f).ToString("0000000000")
		}));
	}

	// Token: 0x06000315 RID: 789 RVA: 0x000020C4 File Offset: 0x000002C4
	public void UNetVersion()
	{
	}

	// Token: 0x17000040 RID: 64
	// (get) Token: 0x06000316 RID: 790 RVA: 0x00016790 File Offset: 0x00014990
	// (set) Token: 0x06000317 RID: 791 RVA: 0x00003F3B File Offset: 0x0000213B
	public int NetworkcurItem
	{
		get
		{
			return this.curItem;
		}
		set
		{
			uint dirtyBit = 2u;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				this.SetCurItem(value);
				base.syncVarHookGuard = false;
			}
			base.SetSyncVar<int>(value, ref this.curItem, dirtyBit);
		}
	}

	// Token: 0x06000318 RID: 792 RVA: 0x00003F7A File Offset: 0x0000217A
	public static void InvokeSyncListverifiedItems(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList verifiedItems called on server.");
			return;
		}
		((Inventory)obj).verifiedItems.HandleMsg(reader);
	}

	// Token: 0x06000319 RID: 793 RVA: 0x000167A4 File Offset: 0x000149A4
	public static void InvokeCmdCmdSetPickup(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPickup called on client.");
			return;
		}
		((Inventory)obj).CmdSetPickup((int)reader.ReadPackedUInt32(), reader.ReadSingle(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadQuaternion());
	}

	// Token: 0x0600031A RID: 794 RVA: 0x000167F4 File Offset: 0x000149F4
	public void CallCmdSetPickup(int dropedItemID, float dur, Vector3 pos, Quaternion camRot, Quaternion myRot)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetPickup called on server.");
			return;
		}
		if (base.isServer)
		{
			this.CmdSetPickup(dropedItemID, dur, pos, camRot, myRot);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write(0);
		networkWriter.Write((short)((ushort)5));
		networkWriter.WritePackedUInt32((uint)Inventory.kCmdCmdSetPickup);
		networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)dropedItemID);
		networkWriter.Write(dur);
		networkWriter.Write(pos);
		networkWriter.Write(camRot);
		networkWriter.Write(myRot);
		base.SendCommandInternal(networkWriter, 2, "CmdSetPickup");
	}

	// Token: 0x0600031B RID: 795 RVA: 0x000168B8 File Offset: 0x00014AB8
	static Inventory()
	{
		NetworkBehaviour.RegisterCommandDelegate(typeof(Inventory), Inventory.kCmdCmdSetPickup, new NetworkBehaviour.CmdDelegate(Inventory.InvokeCmdCmdSetPickup));
		Inventory.kListverifiedItems = -1745481958;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(Inventory), Inventory.kListverifiedItems, new NetworkBehaviour.CmdDelegate(Inventory.InvokeSyncListverifiedItems));
		NetworkCRC.RegisterBehaviour("Inventory", 0);
	}

	// Token: 0x0600031C RID: 796 RVA: 0x00016928 File Offset: 0x00014B28
	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			SyncListInt.WriteInstance(writer, this.verifiedItems);
			writer.WritePackedUInt32((uint)this.curItem);
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
			SyncListInt.WriteInstance(writer, this.verifiedItems);
		}
		if ((base.syncVarDirtyBits & 2u) != 0u)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)this.curItem);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	// Token: 0x0600031D RID: 797 RVA: 0x000169D4 File Offset: 0x00014BD4
	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			SyncListInt.ReadReference(reader, this.verifiedItems);
			this.curItem = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SyncListInt.ReadReference(reader, this.verifiedItems);
		}
		if ((num & 2) != 0)
		{
			this.SetCurItem((int)reader.ReadPackedUInt32());
		}
	}

	// Token: 0x0600031E RID: 798 RVA: 0x00003FA3 File Offset: 0x000021A3
	public void RemoveItem(int id)
	{
		if (this.items[id].id == this.curItem)
		{
			this.NetworkcurItem = -1;
		}
		this.items.RemoveAt(id);
	}

	// Token: 0x0600031F RID: 799
	public void RemoveAll()
	{
		this.items.Clear();
	}

	// Token: 0x040002F9 RID: 761
	public Item[] availableItems;

	// Token: 0x040002FA RID: 762
	public List<Item> items = new List<Item>();

	// Token: 0x040002FB RID: 763
	[SyncVar(hook = "SyncVerItems")]
	public SyncListInt verifiedItems = new SyncListInt();

	// Token: 0x040002FC RID: 764
	public AnimationController ac;

	// Token: 0x040002FD RID: 765
	[SyncVar(hook = "SetCurItem")]
	public int curItem;

	// Token: 0x040002FE RID: 766
	public GameObject kamera;

	// Token: 0x040002FF RID: 767
	public Item localInventoryItem;

	// Token: 0x04000300 RID: 768
	public GameObject pickupPrefab;

	// Token: 0x04000301 RID: 769
	public RawImage crosshair;

	// Token: 0x04000302 RID: 770
	public CharacterClassManager ccm;

	// Token: 0x04000303 RID: 771
	public int prevIt = -10;

	// Token: 0x04000304 RID: 772
	public static int kListverifiedItems;

	// Token: 0x04000305 RID: 773
	public static int kCmdCmdSetPickup = 1938936418;
}
