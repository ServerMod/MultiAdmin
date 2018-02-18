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
	// Token: 0x06000685 RID: 1669 RVA: 0x00006673 File Offset: 0x00004873
	private void Update()
	{
		if (this.popup.activeSelf && Input.GetKey(KeyCode.Escape))
		{
			this.ClickButton();
		}
	}

	// Token: 0x06000686 RID: 1670 RVA: 0x00006697 File Offset: 0x00004897
	public override void OnClientDisconnect(NetworkConnection conn)
	{
		this.ShowLog((int)conn.lastError);
	}

	// Token: 0x06000687 RID: 1671 RVA: 0x000066A5 File Offset: 0x000048A5
	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		this.ShowLog(errorCode);
	}

	// Token: 0x06000688 RID: 1672 RVA: 0x00026184 File Offset: 0x00024384
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

	// Token: 0x06000689 RID: 1673 RVA: 0x000066AE File Offset: 0x000048AE
	public override void OnServerDisconnect(NetworkConnection conn)
	{
		HlapiServer.OnServerDisconnect(conn);
		base.OnServerDisconnect(conn);
		ServerConsole.AddLog("Player disconnect:");
		conn.Disconnect();
		conn.Dispose();
	}

	// Token: 0x0600068A RID: 1674 RVA: 0x000066D3 File Offset: 0x000048D3
	public void OnLevelWasLoaded(int level)
	{
		if (this.reconnect)
		{
			this.ShowLog(14);
			base.Invoke("Reconnect", 2f);
		}
	}

	// Token: 0x0600068B RID: 1675 RVA: 0x000066F8 File Offset: 0x000048F8
	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		base.OnClientSceneChanged(conn);
		if (!this.reconnect && this.logs[this.curLogID].autoHideOnSceneLoad)
		{
			this.popup.SetActive(false);
		}
	}

	// Token: 0x0600068C RID: 1676 RVA: 0x0000672F File Offset: 0x0000492F
	private void Reconnect()
	{
		if (this.reconnect)
		{
			base.StartClient();
			this.reconnect = false;
		}
	}

	// Token: 0x0600068D RID: 1677 RVA: 0x0000674A File Offset: 0x0000494A
	public void StopReconnecting()
	{
		this.reconnect = false;
	}

	// Token: 0x0600068E RID: 1678 RVA: 0x00026228 File Offset: 0x00024428
	public void ShowLog(int id)
	{
		this.curLogID = id;
		this.popup.SetActive(true);
		this.content.text = TranslationReader.Get("Connection_Errors", id);
		this.content.rectTransform.sizeDelta = Vector3.zero;
	}

	// Token: 0x0600068F RID: 1679 RVA: 0x00026278 File Offset: 0x00024478
	public void ClickButton()
	{
		foreach (ConnInfoButton connInfoButton in this.logs[this.curLogID].button.actions)
		{
			connInfoButton.UseButton();
		}
	}

	// Token: 0x06000690 RID: 1680 RVA: 0x000262BC File Offset: 0x000244BC
	private void Start()
	{
		this.console = GameConsole.Console.singleton;
		if (!SteamAPI.Init())
		{
			this.console.AddLog("Failed to init SteamAPI.", new Color32(128, 128, 128, byte.MaxValue), false);
		}
	}

	// Token: 0x06000691 RID: 1681
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

	// Token: 0x06000692 RID: 1682 RVA: 0x00006753 File Offset: 0x00004953
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

	// Token: 0x06000693 RID: 1683 RVA: 0x00006762 File Offset: 0x00004962
	private void NonsteamHost()
	{
		base.onlineScene = "Facility";
		base.maxConnections = 20;
		this.StartHostWithPort();
	}

	// Token: 0x06000694 RID: 1684 RVA: 0x0000677D File Offset: 0x0000497D
	public void StartHostWithPort()
	{
		ServerConsole.AddLog("Server starting at port " + base.networkPort);
		this.StartHost();
	}

	// Token: 0x06000695 RID: 1685 RVA: 0x0002635C File Offset: 0x0002455C
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

	// Token: 0x040005D8 RID: 1496
	public GameObject popup;

	// Token: 0x040005D9 RID: 1497
	public GameObject createpop;

	// Token: 0x040005DA RID: 1498
	public RectTransform contSize;

	// Token: 0x040005DB RID: 1499
	public Text content;

	// Token: 0x040005DC RID: 1500
	public CustomNetworkManager.DisconnectLog[] logs;

	// Token: 0x040005DD RID: 1501
	private int curLogID;

	// Token: 0x040005DE RID: 1502
	public bool reconnect;

	// Token: 0x040005DF RID: 1503
	[Space(20f)]
	public string versionstring;

	// Token: 0x040005E0 RID: 1504
	private GameConsole.Console console;

	// Token: 0x020000ED RID: 237
	[Serializable]
	public class DisconnectLog
	{
		// Token: 0x040005E1 RID: 1505
		[Multiline]
		public string msg_en;

		// Token: 0x040005E2 RID: 1506
		public CustomNetworkManager.DisconnectLog.LogButton button;

		// Token: 0x040005E3 RID: 1507
		public bool autoHideOnSceneLoad;

		// Token: 0x020000EE RID: 238
		[Serializable]
		public class LogButton
		{
			// Token: 0x040005E4 RID: 1508
			public ConnInfoButton[] actions;
		}
	}
}
