using System;
using System.Collections;
using System.IO;
using Dissonance.Integrations.UNet_HLAPI;
using GameConsole;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x020000EC RID: 236
public class CustomNetworkManager : NetworkManager
{
	// Token: 0x06000681 RID: 1665
	private void Update()
	{
		if (this.popup.activeSelf && Input.GetKey(KeyCode.Escape))
		{
			this.ClickButton();
		}
	}

	// Token: 0x06000682 RID: 1666
	public override void OnClientDisconnect(NetworkConnection conn)
	{
		this.ShowLog((int)conn.lastError);
	}

	// Token: 0x06000683 RID: 1667
	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		this.ShowLog(errorCode);
	}

	// Token: 0x06000684 RID: 1668
	public override void OnServerConnect(NetworkConnection conn)
	{
		foreach (BanPlayer.Ban ban in BanPlayer.bans)
		{
			if (ban.ip == conn.address && BanPlayer.NotExpired(ban.time))
			{
				conn.Disconnect();
			}
		}
		ServerConsole.AddLog("Player connect:" + conn.address.ToString());
		if (base.numPlayers == base.maxConnections)
		{
			ServerConsole.AddLog("Server full");
		}
	}

	// Token: 0x06000685 RID: 1669
	public override void OnServerDisconnect(NetworkConnection conn)
	{
		HlapiServer.OnServerDisconnect(conn);
		base.OnServerDisconnect(conn);
		ServerConsole.AddLog("Player disconnect:");
		conn.Disconnect();
		conn.Dispose();
	}

	// Token: 0x06000686 RID: 1670
	public void OnLevelWasLoaded(int level)
	{
		if (this.reconnect)
		{
			this.ShowLog(14);
			base.Invoke("Reconnect", 2f);
		}
	}

	// Token: 0x06000687 RID: 1671
	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		base.OnClientSceneChanged(conn);
		if (!this.reconnect && this.logs[this.curLogID].autoHideOnSceneLoad)
		{
			this.popup.SetActive(false);
		}
	}

	// Token: 0x06000688 RID: 1672
	private void Reconnect()
	{
		if (this.reconnect)
		{
			base.StartClient();
			this.reconnect = false;
		}
	}

	// Token: 0x06000689 RID: 1673
	public void StopReconnecting()
	{
		this.reconnect = false;
	}

	// Token: 0x0600068A RID: 1674
	public void ShowLog(int id)
	{
		this.curLogID = id;
		this.popup.SetActive(true);
		this.content.text = TranslationReader.Get("Connection_Errors", id);
		this.content.rectTransform.sizeDelta = Vector3.zero;
	}

	// Token: 0x0600068B RID: 1675
	public void ClickButton()
	{
		ConnInfoButton[] actions = this.logs[this.curLogID].button.actions;
		for (int i = 0; i < actions.Length; i++)
		{
			actions[i].UseButton();
		}
	}

	// Token: 0x0600068C RID: 1676
	private void Start()
	{
		this.console = GameConsole.Console.singleton;
		if (!SteamAPI.Init())
		{
			this.console.AddLog("Failed to init SteamAPI.", new Color32(128, 128, 128, byte.MaxValue), false);
		}
	}

	// Token: 0x0600068D RID: 1677
	public void CreateMatch()
	{
		ServerConsole.AddLog("ServerMod - Version 1.4 beta (Patch 2)");
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

	// Token: 0x0600068E RID: 1678
	private IEnumerator CreateLobby()
	{
		yield return new WaitForEndOfFrame();
		base.maxConnections = ConfigFile.GetInt("max_players", 20);
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
		}
		ServerConsole.AddLog("Initializing game-server...");
		this.StartHost();
		while (SceneManager.GetActiveScene().name != "Facility")
		{
			yield return new WaitForEndOfFrame();
		}
		ServerConsole.AddLog("Level loaded. Creating match...");
		string value = string.Concat(new string[]
		{
			ConfigFile.GetString("server_name", "Unnamed server"),
			":[:BREAK:]:",
			ConfigFile.GetString("serverinfo_pastebin_id", "7wV681fT"),
			":[:BREAK:]:",
			this.versionstring
		});
		ServerConsole.ip = ip;
		WWWForm wwwform = new WWWForm();
		wwwform.AddField("update", 1);
		wwwform.AddField("ip", ip);
		wwwform.AddField("info", value);
		wwwform.AddField("port", base.networkPort);
		wwwform.AddField("players", 0);
		bool codeNotGenerated = false;
		string pth = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
		if (File.Exists(pth))
		{
			StreamReader streamReader = new StreamReader(pth);
			string text = streamReader.ReadToEnd();
			wwwform.AddField("passcode", text);
			ServerConsole.password = text;
			streamReader.Close();
		}
		else
		{
			wwwform.AddField("passcode", string.Empty);
			codeNotGenerated = true;
		}
		WWW www2 = new WWW("https://hubertmoszka.pl/authenticator.php", wwwform);
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

	// Token: 0x0600068F RID: 1679
	private void NonsteamHost()
	{
		base.onlineScene = "Facility";
		base.maxConnections = 20;
		this.StartHostWithPort();
	}

	// Token: 0x06000690 RID: 1680
	public void StartHostWithPort()
	{
		ServerConsole.AddLog("Server starting at port " + base.networkPort);
		this.StartHost();
	}

	// Token: 0x06000691 RID: 1681
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
				throw new Exception();
			}
			ServerConsole.AddLog("Port queue loaded: " + @string);
			for (int i = 0; i < array.Length; i++)
			{
				ServerConsole.AddLog("Trying to init port: " + array[i] + "...");
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
						ServerConsole.AddLog("Custom config detected, using " + text);
					}
					else
					{
						ServerConsole.AddLog("No custom config detected, using config.txt");
					}
					ServerConsole.AddLog("Done!LOGTYPE-10");
					return base.networkPort;
				}
				ServerConsole.AddLog("...failed.LOGTYPE-6");
			}
		}
		catch
		{
			ServerConsole.AddLog(q);
		}
		return 7777;
	}

	// Token: 0x040005CA RID: 1482
	public GameObject popup;

	// Token: 0x040005CB RID: 1483
	public GameObject createpop;

	// Token: 0x040005CC RID: 1484
	public RectTransform contSize;

	// Token: 0x040005CD RID: 1485
	public Text content;

	// Token: 0x040005CE RID: 1486
	public CustomNetworkManager.DisconnectLog[] logs;

	// Token: 0x040005CF RID: 1487
	private int curLogID;

	// Token: 0x040005D0 RID: 1488
	public bool reconnect;

	// Token: 0x040005D1 RID: 1489
	[Space(20f)]
	public string versionstring;

	// Token: 0x040005D2 RID: 1490
	private GameConsole.Console console;

	// Token: 0x020000ED RID: 237
	[Serializable]
	public class DisconnectLog
	{
		// Token: 0x040005D3 RID: 1491
		[Multiline]
		public string msg_en;

		// Token: 0x040005D4 RID: 1492
		public CustomNetworkManager.DisconnectLog.LogButton button;

		// Token: 0x040005D5 RID: 1493
		public bool autoHideOnSceneLoad;

		// Token: 0x020000EE RID: 238
		[Serializable]
		public class LogButton
		{
			// Token: 0x040005D6 RID: 1494
			public ConnInfoButton[] actions;
		}
	}
}
