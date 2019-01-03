using System;
using System.Collections.Generic;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
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
			return "TitleBar";
		}

		public override void Init()
		{
			maxPlayers = Server.serverConfig.MaxPlayers;
			playerCount = -1; // -1 for the "server" player, once the server starts this will increase to 0.
			UpdateTitlebar();
		}

		public override void OnConfigReload()
		{
			maxPlayers = Server.serverConfig.MaxPlayers;
		}

		public void UpdateTitlebar()
		{
			if (Utils.IsProcessHandleZero) return;

			string smod = string.Empty;
			if (Server.hasServerMod) smod = "SMod " + Server.serverModVersion;
			int displayPlayerCount = playerCount;
			if (playerCount < 0) displayPlayerCount = 0;
			string processId = Server.GameProcess == null
				? string.Empty
				: Server.GameProcess.Id.ToString();

			List<string> titleBar = new List<string>(new[]
			{
				$"MultiAdmin {Server.MaVersion}",
				"Config: NOT YET IMPLEMENTED", // TODO: Add config key to title bar
				$"Session: {Server.SessionId} PID: {processId}",
				$"{displayPlayerCount}/{maxPlayers}"
			});

			if (Server.hasServerMod)
				titleBar.Add(smod);

			Console.Title = string.Join(" | ", titleBar);
		}
	}
}