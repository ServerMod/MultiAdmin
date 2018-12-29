using System;
using System.Diagnostics;
using MultiAdmin.MultiAdmin.Features.Attributes;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	internal class Titlebar : Feature, IEventPlayerConnect, IEventPlayerDisconnect, IEventServerStart
	{
		private int maxPlayers;
		private int playerCount;

		public Titlebar(Server server) : base(server)
		{
		}

		public void OnPlayerConnect(string name)
		{
			playerCount++;
			UpdateTitlebar();
		}

		public void OnPlayerDisconnect(string name)
		{
			playerCount--;
			UpdateTitlebar();
		}

		public void OnServerStart()
		{
			UpdateTitlebar();
		}


		public override string GetFeatureDescription()
		{
			return
				"Updates the title bar with instance based information, such as session id and player count. (Requires ServerMod to function fully)";
		}

		public override string GetFeatureName()
		{
			return "Titlebar";
		}

		public override void Init()
		{
			maxPlayers = Server.ServerConfig.config.GetInt("max_players", 20);
			playerCount = -1; // -1 for the "server" player, once the server starts this will increase to 0.
			UpdateTitlebar();
		}

		public override void OnConfigReload()
		{
		}

		public void UpdateTitlebar()
		{
			if (Server.SkipProcessHandle() || Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
			{
				string smod = string.Empty;
				if (Server.HasServerMod) smod = "SMod " + Server.ServerModVersion;
				int displayPlayerCount = playerCount;
				if (playerCount == -1) displayPlayerCount = 0;
				string processId = Server.GetGameProcess() == null
					? string.Empty
					: Server.GetGameProcess().Id.ToString();
				Console.Title = "MultiAdmin " + Server.MaVersion + " | Config: " + Server.ConfigKey + " | Session:" +
				                Server.GetSessionId() + " PID: " + processId + " | " + displayPlayerCount + "/" +
				                maxPlayers + " | " + smod;
			}
		}
	}
}