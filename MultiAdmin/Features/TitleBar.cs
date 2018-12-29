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
			if (!Utils.SkipProcessHandle() && Process.GetCurrentProcess().MainWindowHandle == IntPtr.Zero) return;

			string smod = string.Empty;
			if (Server.hasServerMod) smod = "SMod " + Server.serverModVersion;
			int displayPlayerCount = playerCount;
			if (playerCount == -1) displayPlayerCount = 0;
			string processId = Server.GameProcess == null
				? string.Empty
				: Server.GameProcess.Id.ToString();

			// TODO: Add config key to title bar
			Console.Title =
				$"MultiAdmin {Server.MaVersion} | Config: NOT YET IMPLEMENTED | Session: {Server.SessionId} PID: {processId} | {displayPlayerCount}/{maxPlayers}{(Server.hasServerMod ? " |" + smod : "")}";
		}
	}
}