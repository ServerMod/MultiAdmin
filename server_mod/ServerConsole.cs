using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using GameConsole;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x02000171 RID: 369
public class ServerConsole : MonoBehaviour
{
	// Token: 0x06000814 RID: 2068 RVA: 0x00002088 File Offset: 0x00000288
	public ServerConsole()
	{
	}

	// Token: 0x06000815 RID: 2069 RVA: 0x00007686 File Offset: 0x00005886
	public static IEnumerator CheckLog()
	{
		for (;;)
		{
			string[] files = Directory.GetFiles("SCPSL_Data/Dedicated/" + ServerConsole.session, "cs*.mapi", SearchOption.TopDirectoryOnly);
			foreach (string text in files)
			{
				string text2 = text.Remove(0, text.IndexOf("cs"));
				string text3 = string.Empty;
				string empty = string.Empty;
				try
				{
					"Error while reading the file: " + text2;
					StreamReader streamReader = new StreamReader("SCPSL_Data/Dedicated/" + ServerConsole.session + "/" + text2);
					string text4 = streamReader.ReadToEnd();
					if (text4.Contains("terminator"))
					{
						text4 = text4.Remove(text4.LastIndexOf("terminator"));
					}
					text3 = ServerConsole.EnterCommand(text4);
					try
					{
						"Error while closing the file: " + text2 + " :: " + text4;
					}
					catch
					{
					}
					streamReader.Close();
					try
					{
						"Error while deleting the file: " + text2 + " :: " + text4;
					}
					catch
					{
					}
					File.Delete("SCPSL_Data/Dedicated/" + ServerConsole.session + "/" + text2);
				}
				catch
				{
				}
				if (!string.IsNullOrEmpty(text3))
				{
					ServerConsole.AddLog(text3);
				}
				yield return new WaitForSeconds(0.07f);
			}
			string[] array = null;
			yield return new WaitForSeconds(1f);
			if (ServerConsole.consoleID == null || ServerConsole.consoleID.HasExited)
			{
				ServerConsole.TerminateProcess();
			}
		}
		yield break;
	}

	// Token: 0x06000816 RID: 2070 RVA: 0x0002E780 File Offset: 0x0002C980
	public static void AddLog(string q)
	{
		if (ServerStatic.isDedicated)
		{
			StreamWriter streamWriter = new StreamWriter(string.Concat(new object[]
			{
				"SCPSL_Data/Dedicated/",
				ServerConsole.session,
				"/sl",
				ServerConsole.logID,
				".mapi"
			}));
			ServerConsole.logID++;
			streamWriter.WriteLine(q);
			streamWriter.Close();
			MonoBehaviour.print(q);
		}
	}

	// Token: 0x06000817 RID: 2071 RVA: 0x0002E7F0 File Offset: 0x0002C9F0
	public static string EnterCommand(string cmds)
	{
		string result = "Command accepted.";
		string[] array = cmds.ToUpper().Split(new char[]
		{
			' '
		});
		if (array.Length != 0)
		{
			string a = array[0];
			if (a == "FORCESTART")
			{
				bool flag = false;
				GameObject gameObject = GameObject.Find("Host");
				if (gameObject != null)
				{
					CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
					if (component != null && component.isLocalPlayer && component.isServer && !component.roundStarted)
					{
						component.ForceRoundStart();
						flag = true;
					}
				}
				result = ((!flag) ? "Failed to force start.LOGTYPE14" : "Forced round start.");
			}
			else if (a == "CONFIG")
			{
				if (File.Exists(ConfigFile.path))
				{
					Application.OpenURL(ConfigFile.path);
				}
				else
				{
					result = "Config file not found!";
				}
			}
			else
			{
				result = GameConsole.Console.singleton.TypeCommand(cmds);
			}
		}
		return result;
	}

	// Token: 0x06000818 RID: 2072 RVA: 0x0000768E File Offset: 0x0000588E
	public void Start()
	{
		if (!ServerStatic.isDedicated)
		{
			return;
		}
		ServerConsole.logID = 0;
		base.StartCoroutine(ServerConsole.CheckLog());
	}

	// Token: 0x06000819 RID: 2073 RVA: 0x000076AA File Offset: 0x000058AA
	public void RunServer()
	{
		base.StartCoroutine(this.RefreshSession());
	}

	// Token: 0x0600081A RID: 2074
	public IEnumerator RefreshSession()
	{
		for (;;)
		{
			WWWForm wwwform = new WWWForm();
			wwwform.AddField("update", 0);
			wwwform.AddField("ip", ServerConsole.ip);
			wwwform.AddField("passcode", ServerConsole.password);
			int num = 0;
			try
			{
				num = GameObject.FindGameObjectsWithTag("Player").Length - 1;
			}
			catch
			{
			}
			wwwform.AddField("players", num);
			wwwform.AddField("port", base.GetComponent<CustomNetworkManager>().networkPort);
			float timeBefore = Time.realtimeSinceStartup;
			WWW www = new WWW("https://hubertmoszka.pl/authenticator.php", wwwform);
			yield return www;
			if (!string.IsNullOrEmpty(www.error) || !www.text.Contains("YES"))
			{
				ServerConsole.AddLog("Could not update the session - " + www.error + www.text + "LOGTYPE-8");
			}
			wwwform.AddField("update", 1);
			wwwform.AddField("ip", ServerConsole.ip);
			wwwform.AddField("passcode", ServerConsole.password);
			wwwform.AddField("info", this.smParseName(ConfigFile.GetString("server_name", "Unnamed server"), num));
			wwwform.AddField("players", num);
			wwwform.AddField("port", base.GetComponent<CustomNetworkManager>().networkPort);
			timeBefore = Time.realtimeSinceStartup;
			www = new WWW("https://hubertmoszka.pl/authenticator.php", wwwform);
			yield return www;
			if (!string.IsNullOrEmpty(www.error) || !www.text.Contains("YES"))
			{
				ServerConsole.AddLog("Could not update the session - " + www.error + www.text + "LOGTYPE-8");
			}
			yield return new WaitForSeconds(5f - (Time.realtimeSinceStartup - timeBefore));
			www = null;
			www = null;
			wwwform = null;
			www = null;
		}
		yield break;
	}

	// Token: 0x0600081B RID: 2075 RVA: 0x000076C8 File Offset: 0x000058C8
	public static void TerminateProcess()
	{
		ServerStatic.isDedicated = false;
		Application.Quit();
	}

	// Token: 0x0600081C RID: 2076
	public string smParseName(string name, int players)
	{
		CustomNetworkManager customNetworkManager = (CustomNetworkManager)NetworkManager.singleton;
		GameObject host = GameObject.Find("Host");
		RoundSummary summary = null;
		if (host != null)
		{
			summary = host.GetComponent<RoundSummary>();
		}
		name = name.Replace("$player_count", players.ToString());
		name = name.Replace("$port", NetworkManager.singleton.networkPort.ToString());
		name = name.Replace("$ip", ServerConsole.ip);
		name = name.Replace("$number", (customNetworkManager.networkPort - 7776).ToString());
		name = name.Replace("$version", customNetworkManager.versionstring);
		name = name.Replace("$max_players", customNetworkManager.maxConnections.ToString());
		name = name.Replace("$full_player_count", (players == customNetworkManager.maxConnections) ? "FULL" : (players + "/" + customNetworkManager.maxConnections));
		if (summary != null)
		{
			summary.CheckForEnding();
			name = name.Replace("$scp_alive", summary.summary.scp_alive.ToString());
			name = name.Replace("$scp_start", summary.summary.scp_start.ToString());
			name = name.Replace("$scp_counter", summary.summary.scp_alive.ToString() + "/" + summary.summary.scp_start.ToString());
			name = name.Replace("$scp_dead", (summary.summary.scp_start - summary.summary.scp_alive).ToString());
			name = name.Replace("$scp_zombies", summary.summary.scp_nozombies.ToString());
			name = name.Replace("$classd_escape", summary.summary.classD_escaped.ToString());
			name = name.Replace("$classd_start", summary.summary.classD_start.ToString());
			name = name.Replace("$classd_counter", summary.summary.classD_escaped.ToString() + "/" + summary.summary.classD_start.ToString());
			name = name.Replace("$scientists_escape", summary.summary.scientists_escaped.ToString());
			name = name.Replace("$scientists_start", summary.summary.scientists_start.ToString());
			name = name.Replace("$scientists_counter", summary.summary.scientists_escaped.ToString() + "/" + summary.summary.scientists_start.ToString());
			name = name.Replace("$scp_kills", summary.summary.scp_frags.ToString());
			name = name.Replace("$warhead_detonated", summary.summary.warheadDetonated ? "☢ WARHEAD DETONATED ☢" : "");
		}
		else
		{
			name = name.Replace("$scp_alive", "-");
			name = name.Replace("$scp_start", "-");
			name = name.Replace("$scp_dead", "-");
			name = name.Replace("$scp_counter", "-");
			name = name.Replace("$classd_escape", "-");
			name = name.Replace("$classd_start", "-");
			name = name.Replace("$classd_counter", "-");
			name = name.Replace("$scp_zombies", "-");
			name = name.Replace("$scientists_escape", "-");
			name = name.Replace("$scientists_start", "-");
			name = name.Replace("$scientists_counter", "-");
			name = name.Replace("$scp_kills", "-");
			name = name.Replace("$warhead_detonated", "");
		}
		return string.Concat(new string[]
		{
			name,
			":[:BREAK:]:",
			ConfigFile.GetString("serverinfo_pastebin_id", "7wV681fT"),
			":[:BREAK:]:",
			customNetworkManager.versionstring
		});
	}

	// Token: 0x04000808 RID: 2056
	public static int logID;

	// Token: 0x04000809 RID: 2057
	public static Process consoleID;

	// Token: 0x0400080A RID: 2058
	public static string session;

	// Token: 0x0400080B RID: 2059
	public static string password;

	// Token: 0x0400080C RID: 2060
	public static string ip;
}
