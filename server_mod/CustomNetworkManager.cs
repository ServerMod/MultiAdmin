using System;
using System.Collections;
using System.IO;
using GameConsole;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x020000E6 RID: 230
public class CustomNetworkManager : NetworkManager
{
	// Token: 0x0600064E RID: 1614
	public CustomNetworkManager()
	{
	}

	// Token: 0x0600064F RID: 1615
	public override void OnClientDisconnect(NetworkConnection conn)
	{
		this.ShowLog((int)conn.lastError);
	}

	// Token: 0x06000650 RID: 1616
	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		this.ShowLog(errorCode);
	}

	// Token: 0x06000651 RID: 1617
	public override void OnServerConnect(NetworkConnection conn)
	{
		foreach (BanPlayer.Ban ban in BanPlayer.bans)
		{
			if (ban.ip == conn.address && BanPlayer.NotExpired(ban.time))
			{
				conn.Disconnect();
			}
			ServerConsole.AddLog("Player connect:");
			if (base.numPlayers == base.maxConnections)
			{
				ServerConsole.AddLog("Server full");
			}
		}
	}

	// Token: 0x06000652 RID: 1618
	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);
		ServerConsole.AddLog("Player disconnect:");
		conn.Disconnect();
		conn.Dispose();
	}

	// Token: 0x06000653 RID: 1619
	public void OnLevelWasLoaded(int level)
	{
		if (this.reconnect)
		{
			this.ShowLog(14);
			base.Invoke("Reconnect", 2f);
		}
	}

	// Token: 0x06000654 RID: 1620
	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		base.OnClientSceneChanged(conn);
		if (!this.reconnect && this.logs[this.curLogID].autoHideOnSceneLoad)
		{
			this.popup.SetActive(false);
		}
	}

	// Token: 0x06000655 RID: 1621
	public void Reconnect()
	{
		if (this.reconnect)
		{
			base.StartClient();
			this.reconnect = false;
		}
	}

	// Token: 0x06000656 RID: 1622
	public void StopReconnecting()
	{
		this.reconnect = false;
	}

	// Token: 0x06000657 RID: 1623
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

	// Token: 0x06000658 RID: 1624
	public void ClickButton()
	{
		ConnInfoButton[] actions = this.logs[this.curLogID].button.actions;
		for (int i = 0; i < actions.Length; i++)
		{
			actions[i].UseButton();
		}
	}

	// Token: 0x06000659 RID: 1625
	public void Start()
	{
		this.console = GameConsole.Console.singleton;
		if (!SteamAPI.Init())
		{
			this.console.AddLog("Failed to init SteamAPI.", new Color32(128, 128, 128, byte.MaxValue), false);
		}
	}

	// Token: 0x0600065A RID: 1626
	public void CreateMatch()
	{
		ServerConsole.AddLog("ServerMod - Version 1.2");
		base.maxConnections = ConfigFile.GetInt("max_players", 20);
		this.ShowLog(13);
		this.createpop.SetActive(false);
		NetworkServer.Reset();
		this.GetFreePort();
		if (ServerStatic.isDedicated)
		{
			base.StartCoroutine(this.CreateLobby());
			return;
		}
		this.NonsteamHost();
	}

	// Token: 0x0600065B RID: 1627
	public IEnumerator CreateLobby()
	{
		yield return new WaitForEndOfFrame();
		string ip = string.Empty;
		if (ConfigFile.GetString("server_ip", "auto") != "auto")
		{
			ip = ConfigFile.GetString("server_ip", "auto");
			ServerConsole.AddLog("Custom config detected. Your game-server IP will be " + ip);
		}
		else
		{
			ServerConsole.AddLog("Downloading your external IP address from: http://icanhazip.com/");
			WWW www = new WWW("http://icanhazip.com/");
			yield return www;
			if (!string.IsNullOrEmpty(www.error))
			{
				ServerConsole.AddLog("Error: connection to http://icanhazip.com/ failed. Website returned: " + www.error + " | Aborting startup... LOGTYPE-8");
				yield break;
			}
			ip = www.text.Remove(www.text.Length - 1);
			ServerConsole.AddLog("Done, your game-server IP will be " + ip);
			www = null;
			www = null;
			www = null;
		}
		ServerConsole.AddLog("Initializing game-server...");
		this.StartHost();
		while (SceneManager.GetActiveScene().name != "Facility")
		{
			yield return new WaitForEndOfFrame();
		}
		ServerConsole.AddLog("Level loaded. Creating match...");
		string info = UnityEngine.Object.FindObjectOfType<ServerConsole>().smParseName(ConfigFile.GetString("server_name", "Unnamed server"), 0);
		ServerConsole.ip = ip;
		WWWForm form = new WWWForm();
		form.AddField("update", 1);
		form.AddField("ip", ip);
		form.AddField("info", info);
		form.AddField("port", base.networkPort);
		form.AddField("players", 0);
		bool codeNotGenerated = false;
		string pth = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
		if (File.Exists(pth))
		{
			StreamReader streamReader = new StreamReader(pth);
			string text = streamReader.ReadToEnd();
			form.AddField("passcode", text);
			ServerConsole.password = text;
			streamReader.Close();
		}
		else
		{
			form.AddField("passcode", string.Empty);
			codeNotGenerated = true;
		}
		WWW www2 = new WWW("https://hubertmoszka.pl/authenticator.php", form);
		yield return www2;
		if (string.IsNullOrEmpty(www2.error))
		{
			if (www2.text.Contains("YES"))
			{
				UnityEngine.Object.FindObjectOfType<ServerConsole>().RunServer();
				ServerConsole.AddLog("The match is ready!LOGTYPE-8");
				if (codeNotGenerated)
				{
					try
					{
						StreamWriter streamWriter = new StreamWriter(pth);
						string text2 = www2.text.Remove(0, www2.text.IndexOf(":")).Remove(www2.text.IndexOf(":"));
						while (text2.Contains(":"))
						{
							text2 = text2.Replace(":", string.Empty);
						}
						streamWriter.WriteLine(text2);
						streamWriter.Close();
						ServerConsole.AddLog("New password saved.LOGTYPE-8");
					}
					catch
					{
						ServerConsole.AddLog("New password could not be saved.LOGTYPE-8");
					}
					yield return new WaitForSeconds(2f);
					ServerConsole.AddLog("THIS SESSION HAS TO BE RESTARTED TO CONTINUE.LOGTYPE-2");
					yield return new WaitForSeconds(2f);
					ServerConsole.AddLog("This is a standard procedure. Don't worry about that crash!LOGTYPE-2");
					yield return new WaitForSeconds(2f);
					ServerConsole.AddLog("Forcing the crash in:LOGTYPE-2");
					int num;
					for (int i = 5; i > 0; i = num - 1)
					{
						yield return new WaitForSeconds(1f);
						ServerConsole.AddLog(i + "LOGTYPE-2");
						num = i;
					}
					Application.Quit();
				}
			}
			else
			{
				ServerConsole.AddLog(string.Concat(new string[]
				{
					"Your server won't be visible on the public server list - ",
					www2.text,
					" (",
					ip,
					")LOGTYPE-8"
				}));
				ServerConsole.AddLog("If you are 100% sure that the server is working correctly send your IP address at: LOGTYPE-8");
				ServerConsole.AddLog("server.verification@hubertmoszka.pl LOGTYPE-8");
			}
		}
		else
		{
			ServerConsole.AddLog("Could not create the match - " + www2.error + "LOGTYPE-8");
		}
		yield break;
	}

	// Token: 0x0600065C RID: 1628
	public void NonsteamHost()
	{
		base.onlineScene = "Facility";
		base.maxConnections = 20;
		this.StartHostWithPort();
	}

	// Token: 0x0600065D RID: 1629
	public void StartHostWithPort()
	{
		ServerConsole.AddLog("Server starting at port " + base.networkPort);
		this.StartHost();
	}

	// Token: 0x0600065E RID: 1630
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

	// Token: 0x04000597 RID: 1431
	public GameObject popup;

	// Token: 0x04000598 RID: 1432
	public GameObject createpop;

	// Token: 0x04000599 RID: 1433
	public RectTransform contSize;

	// Token: 0x0400059A RID: 1434
	public TextMeshProUGUI content;

	// Token: 0x0400059B RID: 1435
	public Button button;

	// Token: 0x0400059C RID: 1436
	public CustomNetworkManager.DisconnectLog[] logs;

	// Token: 0x0400059D RID: 1437
	public int curLogID;

	// Token: 0x0400059E RID: 1438
	public bool reconnect;

	// Token: 0x0400059F RID: 1439
	[Space(20f)]
	public string versionstring;

	// Token: 0x040005A0 RID: 1440
	public GameConsole.Console console;

	// Token: 0x020000E7 RID: 231
	[Serializable]
	public class DisconnectLog
	{
		// Token: 0x0600065F RID: 1631
		public DisconnectLog()
		{
		}

		// Token: 0x040005A1 RID: 1441
		[Multiline]
		public string msg_en;

		// Token: 0x040005A2 RID: 1442
		[Multiline]
		public string msg_pl;

		// Token: 0x040005A3 RID: 1443
		public Vector2 msgSize_en;

		// Token: 0x040005A4 RID: 1444
		public Vector2 msgSize_pl;

		// Token: 0x040005A5 RID: 1445
		public CustomNetworkManager.DisconnectLog.LogButton button;

		// Token: 0x040005A6 RID: 1446
		public bool autoHideOnSceneLoad;

		// Token: 0x020000E8 RID: 232
		[Serializable]
		public class LogButton
		{
			// Token: 0x06000660 RID: 1632
			public LogButton()
			{
			}

			// Token: 0x040005A7 RID: 1447
			public ConnInfoButton[] actions;

			// Token: 0x040005A8 RID: 1448
			public string content_en;

			// Token: 0x040005A9 RID: 1449
			public string content_pl;

			// Token: 0x040005AA RID: 1450
			public float size_en;

			// Token: 0x040005AB RID: 1451
			public float size_pl;
		}
	}
}
