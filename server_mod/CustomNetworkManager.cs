using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using GameConsole;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// Token: 0x020000E4 RID: 228
public class CustomNetworkManager : NetworkManager
{
	// Token: 0x06000659 RID: 1625
	public CustomNetworkManager()
	{
	}

	// Token: 0x0600065A RID: 1626
	public override void OnClientDisconnect(NetworkConnection conn)
	{
		this.ShowLog((int)conn.lastError);
	}

	// Token: 0x0600065B RID: 1627
	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		this.ShowLog(errorCode);
	}

	// Token: 0x0600065C RID: 1628
	public override void OnServerConnect(NetworkConnection conn)
	{
		foreach (BanPlayer.Ban ban in BanPlayer.bans)
		{
			if (ban.ip == conn.address && BanPlayer.NotExpired(ban.time))
			{
				conn.Disconnect();
			}
		}
		ServerConsole.AddLog("Player connect:");
		if (base.numPlayers == base.maxConnections)
		{
			ServerConsole.AddLog("Server full");
		}
		this.UpdateMotd(0);
	}

	// Token: 0x0600065D RID: 1629
	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);
		ServerConsole.AddLog("Player disconnect:");
		conn.Disconnect();
		conn.Dispose();
		this.UpdateMotd(1);
	}

	// Token: 0x0600065E RID: 1630
	public void OnLevelWasLoaded(int level)
	{
		if (this.reconnect)
		{
			this.ShowLog(14);
			base.Invoke("Reconnect", 2f);
		}
	}

	// Token: 0x0600065F RID: 1631
	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		base.OnClientSceneChanged(conn);
		if (!this.reconnect && this.logs[this.curLogID].autoHideOnSceneLoad)
		{
			this.popup.SetActive(false);
		}
	}

	// Token: 0x06000660 RID: 1632
	public void Reconnect()
	{
		if (this.reconnect)
		{
			base.StartClient();
			this.reconnect = false;
		}
	}

	// Token: 0x06000661 RID: 1633
	public void StopReconnecting()
	{
		this.reconnect = false;
	}

	// Token: 0x06000662 RID: 1634
	public void ShowLog(int id)
	{
		this.curLogID = id;
		bool flag = PlayerPrefs.GetString("langver", "en") == "pl";
		this.popup.SetActive(true);
		this.contSize.sizeDelta = ((!flag) ? this.logs[id].msgSize_en : this.logs[id].msgSize_pl);
		this.content.text = ((!flag) ? this.logs[id].msg_en : this.logs[id].msg_pl);
		this.button.GetComponentInChildren<Text>().text = ((!flag) ? this.logs[id].button.content_en : this.logs[id].button.content_pl);
		this.button.GetComponent<RectTransform>().sizeDelta = new Vector2((!flag) ? this.logs[id].button.size_en : this.logs[id].button.size_pl, 80f);
	}

	// Token: 0x06000663 RID: 1635
	public void ClickButton()
	{
		ConnInfoButton[] actions = this.logs[this.curLogID].button.actions;
		for (int i = 0; i < actions.Length; i++)
		{
			actions[i].UseButton();
		}
	}

	// Token: 0x06000664 RID: 1636
	public void Start()
	{
		NetworkServer.Configure(new ConnectionConfig
		{
			IsAcksLong = true,
			MaxSentMessageQueueSize = 512
		}, 20);
		if (false)
		{
			base.logLevel = LogFilter.FilterLevel.Debug;
			Application.stackTraceLogType = StackTraceLogType.ScriptOnly;
			Network.logLevel = NetworkLogLevel.Full;
		}
		this.console = GameConsole.Console.singleton;
		this.Callback_lobbyCreated = Callback<LobbyCreated_t>.Create(new Callback<LobbyCreated_t>.DispatchDelegate(this.OnLobbyCreated));
		this.Callback_lobbyList = Callback<LobbyMatchList_t>.Create(new Callback<LobbyMatchList_t>.DispatchDelegate(this.OnGetLobbiesList));
		if (!SteamAPI.Init())
		{
			this.console.AddLog("Failed to init SteamAPI.", new Color32(128, 128, 128, byte.MaxValue), false);
		}
	}

	// Token: 0x06000665 RID: 1637
	public void CreateMatch()
	{
		ServerConsole.AddLog("ServerMod - Version 1.0");
		this.maxPlayers = ConfigFile.GetInt("max_players", 20);
		base.maxConnections = this.maxPlayers;
		ServerConsole.AddLog("max players: " + this.maxPlayers);
		this.createpop.SetActive(false);
		NetworkServer.Reset();
		this.GetFreePort();
		if (ServerStatic.isDedicated)
		{
			if (SteamManager.Initialized)
			{
				SteamServerManager._instance.CreateServer();
				return;
			}
			ServerConsole.AddLog("Warning: Steam was not detected. Match won't be visible on server list.");
			this.NonsteamHost();
			return;
		}
		else
		{
			this.ShowLog(13);
			if (Input.GetKey(KeyCode.Space))
			{
				this.NonsteamHost();
				return;
			}
			SteamServerManager._instance.CreateServer();
			return;
		}
	}

	// Token: 0x06000666 RID: 1638
	public void NonsteamHost()
	{
		base.onlineScene = "Facility";
		base.maxConnections = this.maxPlayers;
		GameConsole.Console.singleton.AddLog("max players: " + this.maxPlayers, new Color32(128, 128, 128, byte.MaxValue), false);
		ServerConsole.AddLog("max players: " + this.maxPlayers);
		this.StartHostWithPort();
	}

	// Token: 0x06000667 RID: 1639
	public void StartHostWithPort()
	{
		ServerConsole.AddLog("Server starting at port " + base.networkPort);
		this.StartHost();
	}

	// Token: 0x06000668 RID: 1640
	public int GetFreePort()
	{
		string @string = ConfigFile.GetString("port_queue", "7777,7778,7779,7780,7781,7782,7783,7784");
		try
		{
			while (@string.Contains(" "))
			{
				@string.Replace(" ", string.Empty);
			}
		}
		catch
		{
			ServerConsole.AddLog("Failed to remove spaces from config - port_queue - please make it manualy.");
		}
		string q = string.Empty;
		try
		{
			q = "Failed to split ports.";
			string[] array = @string.Split(new char[]
			{
				','
			});
			if (array.Length == 0)
			{
				q = "Failed to detect ports.";
			}
			for (int i = 0; i < array.Length; i++)
			{
				q = "Failed to convert [" + array[i] + "]  into integer number!";
				base.networkPort = int.Parse(array[i]);
				if (NetworkServer.Listen(base.networkPort))
				{
					NetworkServer.Reset();
					string text = string.Concat(new object[]
					{
						Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						"/SCP Secret Laboratory/",
						base.networkPort,
						"/config.txt"
					});
					if (File.Exists(text))
					{
						q = "Failed to read config file: " + text;
						ConfigFile.path = text;
						ConfigFile.singleton.ReloadConfig();
					}
					return base.networkPort;
				}
			}
		}
		catch
		{
			ServerConsole.AddLog(q);
		}
		return 7777;
	}

	// Token: 0x06000669 RID: 1641
	public void FindMatch()
	{
		SteamMatchmaking.AddRequestLobbyListStringFilter("ver", this.versionstring[0], ELobbyComparison.k_ELobbyComparisonEqual);
		SteamMatchmaking.AddRequestLobbyListResultCountFilter(500);
		SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
		SteamMatchmaking.RequestLobbyList();
	}

	// Token: 0x0600066A RID: 1642
	public void OnLobbyCreated(LobbyCreated_t result)
	{
		if (result.m_eResult == EResult.k_EResultOK)
		{
			this.console.AddLog("Steam lobby created!", new Color32(128, 128, 128, byte.MaxValue), false);
			ServerConsole.AddLog("Steam lobby created!");
			string text = new IPAddress(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)SteamGameServer.GetPublicIP()))).ToString();
			this.console.AddLog("Your machine IP is " + text, new Color32(128, 128, 128, byte.MaxValue), false);
			string @string = ConfigFile.GetString("server_ip", "auto");
			string text2 = ConfigFile.GetString("server_name", "[nick]'s game");
			text2 = ((!text2.Contains("[nick]")) ? text2 : text2.Replace("[nick]", SteamFriends.GetPersonaName()));
			if (@string != "auto")
			{
				text = @string;
			}
			this.steam_id = (CSteamID)result.m_ulSteamIDLobby;
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "ServerIP", text);
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "MOTD", text2);
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "port", base.networkPort.ToString());
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "ver", this.versionstring[0]);
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "SMMaxPlayers", this.maxPlayers.ToString());
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "SMPlayers", base.numPlayers.ToString());
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "info_type", ConfigFile.GetString("serverinfo_mode", "off"));
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "info_ref", ConfigFile.GetString("serverinfo_pastebin_id", "7wV681fT"));
			this.CloseOrphanedLobbies(base.networkPort);
			this.SaveCurrentLobby(this.steam_id, base.networkPort);
			this.UpdateMotd(0);
			this.isHost = true;
			ServerConsole.AddLog("Loading level...");
			this.StartHostWithPort();
			return;
		}
		this.console.AddLog("Steam lobby not created. Error: " + result.m_eResult.ToString() + ".", new Color32(128, 128, 128, byte.MaxValue), false);
		ServerConsole.AddLog("Steam lobby not created. Error: " + result.m_eResult.ToString() + ".");
	}

	// Token: 0x0600066B RID: 1643
	public void OnGetLobbiesList(LobbyMatchList_t result)
	{
		base.StartCoroutine(this.ShowList(result));
	}

	// Token: 0x0600066C RID: 1644
	public IEnumerator ShowList(LobbyMatchList_t result)
	{
		ServerListManager slm = ServerListManager.singleton;
		yield return new WaitForSeconds(0.5f);
		slm.resultRecieved = true;
		this.console.AddLog("Found lobbies: " + result.m_nLobbiesMatching, new Color32(128, 128, 128, byte.MaxValue), false);
		int i = 0;
		while ((long)i < (long)((ulong)result.m_nLobbiesMatching))
		{
			CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(i);
			if (SteamMatchmaking.GetLobbyData(lobbyByIndex, "ServerIP") != string.Empty)
			{
				slm.AddRecord(lobbyByIndex, SteamMatchmaking.GetLobbyData(lobbyByIndex, "MOTD"));
				yield return new WaitForEndOfFrame();
			}
			int num = i;
			i = num + 1;
		}
		yield break;
	}

	// Token: 0x0600066D RID: 1645
	public void CloseOrphanedLobbies(int current_port)
	{
		this.LoadLobbies();
		List<CustomNetworkManager.Lobby> list = new List<CustomNetworkManager.Lobby>();
		if (this.steam_lobbies != null)
		{
			foreach (CustomNetworkManager.Lobby lobby in this.steam_lobbies)
			{
				if (current_port == lobby.port)
				{
					this.console.AddLog("Found orphaned lobby, leaving it", new Color32(128, 128, 128, byte.MaxValue), false);
					ServerConsole.AddLog("Found orphaned lobby, leaving it");
					SteamMatchmaking.LeaveLobby(lobby.lobby_id);
				}
				else
				{
					list.Add(lobby);
				}
			}
		}
		this.steam_lobbies = list;
	}

	// Token: 0x0600066E RID: 1646
	public void SaveCurrentLobby(CSteamID current_id, int current_port)
	{
		if (this.steam_lobbies == null)
		{
			this.steam_lobbies = new List<CustomNetworkManager.Lobby>();
		}
		this.steam_lobbies.Add(new CustomNetworkManager.Lobby(current_id, current_port));
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Create(Application.persistentDataPath + "/lobby.registry");
		binaryFormatter.Serialize(fileStream, this.steam_lobbies);
		fileStream.Close();
	}

	// Token: 0x0600066F RID: 1647
	public void UpdateMotd(int sub_playercount = 0)
	{
		CSteamID csteamID = this.steam_id;
		string text = ConfigFile.GetString("server_name", "[nick]'s game");
		string newValue = new IPAddress(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)SteamGameServer.GetPublicIP()))).ToString();
		text = text.Replace("[nick]", SteamFriends.GetPersonaName());
		text = text.Replace("$player_count", (base.numPlayers - sub_playercount).ToString());
		text = text.Replace("$port", base.networkPort.ToString());
		text = text.Replace("$ip", newValue);
		text = text.Replace("$number", (base.networkPort - 7776).ToString());
		text = text.Replace("$lobby_id", this.steam_id.ToString());
		text = text.Replace("$version", this.versionstring[0]);
		text = text.Replace("$max_players", this.maxPlayers.ToString());
		string newValue2 = base.numPlayers.ToString();
		if (base.numPlayers == base.maxConnections)
		{
			newValue2 = "FULL";
		}
		else
		{
			newValue2 = base.numPlayers.ToString() + "/" + this.maxPlayers;
		}
		text = text.Replace("$full_player_count", newValue2);
		SteamMatchmaking.SetLobbyData(this.steam_id, "MOTD", text);
		SteamMatchmaking.SetLobbyData(this.steam_id, "SMMaxPlayers", this.maxPlayers.ToString());
		SteamMatchmaking.SetLobbyData(this.steam_id, "SMPlayers", base.numPlayers.ToString());
	}

	// Token: 0x06000670 RID: 1648
	public void LeaveLobby()
	{
		CSteamID csteamID = this.steam_id;
		this.LoadLobbies();
		SteamMatchmaking.LeaveLobby(this.steam_id);
		List<CustomNetworkManager.Lobby> list = new List<CustomNetworkManager.Lobby>();
		foreach (CustomNetworkManager.Lobby lobby in this.steam_lobbies)
		{
			if (lobby.lobby_id != this.steam_id)
			{
				list.Add(lobby);
			}
		}
		this.steam_lobbies = list;
		this.SaveLobbyList();
	}

	// Token: 0x06000671 RID: 1649
	public void LoadLobbies()
	{
		if (File.Exists(Application.persistentDataPath + "/lobby.registry"))
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			FileStream fileStream = File.Open(Application.persistentDataPath + "/lobby.registry", FileMode.Open);
			this.steam_lobbies = (List<CustomNetworkManager.Lobby>)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			return;
		}
		this.steam_lobbies = new List<CustomNetworkManager.Lobby>();
	}

	// Token: 0x06000672 RID: 1650
	public void SaveLobbyList()
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Create(Application.persistentDataPath + "/lobby.registry");
		binaryFormatter.Serialize(fileStream, this.steam_lobbies);
		fileStream.Close();
	}

	// Token: 0x04000586 RID: 1414
	public GameObject popup;

	// Token: 0x04000587 RID: 1415
	public GameObject createpop;

	// Token: 0x04000588 RID: 1416
	public RectTransform contSize;

	// Token: 0x04000589 RID: 1417
	public TextMeshProUGUI content;

	// Token: 0x0400058A RID: 1418
	public Button button;

	// Token: 0x0400058B RID: 1419
	public CustomNetworkManager.DisconnectLog[] logs;

	// Token: 0x0400058C RID: 1420
	public int curLogID;

	// Token: 0x0400058D RID: 1421
	public bool reconnect;

	// Token: 0x0400058E RID: 1422
	[Space(20f)]
	public string[] versionstring;

	// Token: 0x0400058F RID: 1423
	public Callback<LobbyCreated_t> Callback_lobbyCreated;

	// Token: 0x04000590 RID: 1424
	public Callback<LobbyEnter_t> Callback_lobbyEnter;

	// Token: 0x04000591 RID: 1425
	public Callback<LobbyMatchList_t> Callback_lobbyList;

	// Token: 0x04000592 RID: 1426
	public bool isHost;

	// Token: 0x04000593 RID: 1427
	public GameConsole.Console console;

	// Token: 0x04000594 RID: 1428
	public CSteamID steam_id;

	// Token: 0x04000595 RID: 1429
	public List<CustomNetworkManager.Lobby> steam_lobbies;

	// Token: 0x04000596 RID: 1430
	public int maxPlayers;

	// Token: 0x020000E5 RID: 229
	[Serializable]
	public class DisconnectLog
	{
		// Token: 0x06000673 RID: 1651
		public DisconnectLog()
		{
		}

		// Token: 0x04000597 RID: 1431
		[Multiline]
		public string msg_en;

		// Token: 0x04000598 RID: 1432
		[Multiline]
		public string msg_pl;

		// Token: 0x04000599 RID: 1433
		public Vector2 msgSize_en;

		// Token: 0x0400059A RID: 1434
		public Vector2 msgSize_pl;

		// Token: 0x0400059B RID: 1435
		public CustomNetworkManager.DisconnectLog.LogButton button;

		// Token: 0x0400059C RID: 1436
		public bool autoHideOnSceneLoad;

		// Token: 0x020000E6 RID: 230
		[Serializable]
		public class LogButton
		{
			// Token: 0x06000674 RID: 1652
			public LogButton()
			{
			}

			// Token: 0x0400059D RID: 1437
			public ConnInfoButton[] actions;

			// Token: 0x0400059E RID: 1438
			public string content_en;

			// Token: 0x0400059F RID: 1439
			public string content_pl;

			// Token: 0x040005A0 RID: 1440
			public float size_en;

			// Token: 0x040005A1 RID: 1441
			public float size_pl;
		}
	}

	// Token: 0x020000E7 RID: 231
	[Serializable]
	public class Lobby
	{
		// Token: 0x06000675 RID: 1653
		public Lobby(CSteamID id, int port)
		{
			this.lobby_id = id;
			this.port = port;
		}

		// Token: 0x040005A2 RID: 1442
		public CSteamID lobby_id;

		// Token: 0x040005A3 RID: 1443
		public int port;
	}
}
