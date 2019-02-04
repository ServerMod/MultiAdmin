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
				"Updates the title bar with instance based information, such as session id and player count (Requires ServerMod to function fully)";
		}

		public override string GetFeatureName()
		{
			return "TitleBar";
		}

		public override void Init()
		{
			maxPlayers = Server.ServerConfig.MaxPlayers;
			playerCount = -1; // -1 for the "server" player, once the server starts this will increase to 0.
			UpdateTitlebar();
		}

		public override void OnConfigReload()
		{
			maxPlayers = Server.ServerConfig.MaxPlayers;
			UpdateTitlebar();
		}

		private void UpdateTitlebar()
		{
			if (Utils.IsProcessHandleZero) return;

			int displayPlayerCount = playerCount < 0 ? 0 : playerCount;

			List<string> titleBar = new List<string>();

			titleBar.Add($"MultiAdmin {Server.MaVersion}");

			if (!string.IsNullOrEmpty(Server.serverId))
				titleBar.Add($"Config: {Server.serverId}");

			if (!string.IsNullOrEmpty(Server.SessionId))
				titleBar.Add($"Session: {Server.SessionId}");

			if (Server.GameProcess != null)
				titleBar.Add($"PID: {Server.GameProcess.Id}");

			titleBar.Add($"{displayPlayerCount}/{maxPlayers}");

			if (Server.hasServerMod)
				titleBar.Add("SMod " + Server.serverModVersion);

			Console.Title = string.Join(" | ", titleBar);
		}
	}
}