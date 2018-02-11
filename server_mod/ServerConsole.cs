using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public partial class ServerConsole : MonoBehaviour
{
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
		}
		yield break;
	}


	public string smParseName(string name, int players)
	{
		CustomNetworkManager customNetworkManager = (CustomNetworkManager)CustomNetworkManager.singleton;
		GameObject host = GameObject.Find("Host");
		RoundSummary summary = null;
		if (host != null)
		{
			summary = host.GetComponent<RoundSummary>();
		}
		name = name.Replace("$player_count", players.ToString());
		name = name.Replace("$port", customNetworkManager.networkPort.ToString());
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
}
