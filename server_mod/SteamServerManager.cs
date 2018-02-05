{
	// Token: 0x06000859 RID: 2137
	public IEnumerator CreateLobby()
	{
		ServerConsole.AddLog("Creating lobby...");
		yield return new WaitForEndOfFrame();
		int num = 1;
		int value = IPAddress.HostToNetworkOrder((int)SteamGameServer.GetPublicIP());
		string ip = new IPAddress(BitConverter.GetBytes(value)).ToString();
		WWWForm wwwform = new WWWForm();
		wwwform.AddField("ip", ip);
		WWW www = new WWW("https://hubertmoszka.pl/server_authenticator.php", wwwform);
		yield return www;
		if (!string.IsNullOrEmpty(www.text))
		{
			if (int.TryParse(www.text.Remove(1), out num))
			{
				this.console.AddLog("Your public server is now visible on the list!", new Color32(128, 128, 128, byte.MaxValue), false);
				ServerConsole.AddLog("Your public server is now visible on the list! LOGTYPE-2");
			}
			else
			{
				ServerConsole.AddLog("Your server is not verified - it won't be visible on the server list.LOGTYPE14");
				ServerConsole.AddLog("If you are 100% sure that the server is working correctly send a screenshot of the following informations:LOGTYPE-8");
				ServerConsole.AddLog(ip + " - not verified. Error: " + www.text + "LOGTYPE-8");
				ServerConsole.AddLog("Connect to: " + ConfigFile.GetString("server_ip", string.Empty) + "LOGTYPE-8");
				ServerConsole.AddLog("Email: server.verification@hubertmoszka.pl LOGTYPE-8");
				this.console.AddLog("Your server is now ready for your friends.", new Color32(128, 128, 128, byte.MaxValue), false);
			}
		}
		else
		{
			ServerConsole.AddLog("Database error: " + www.error + "LOGTYPE14");
		}
		SteamMatchmaking.CreateLobby((ELobbyType)num, 20);
		yield break;
	}
}
