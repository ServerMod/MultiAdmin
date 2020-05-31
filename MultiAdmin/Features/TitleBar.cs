using System;
using System.Collections.Generic;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class Titlebar : Feature, IEventServerStart
	{
		private int ServerProcessId
		{
			get
			{
				if (Server.GameProcess == null)
					return -1;

				Server.GameProcess.Refresh();

				return Server.GameProcess.Id;
			}
		}

		public Titlebar(Server server) : base(server)
		{
		}

		public void OnServerStart()
		{
			UpdateTitlebar();
		}

		public override string GetFeatureDescription()
		{
			return
				"Updates the title bar with instance based information, such as console port and player count [Requires ServerMod to function fully]";
		}

		public override string GetFeatureName()
		{
			return "TitleBar";
		}

		public override void Init()
		{
			UpdateTitlebar();
		}

		public override void OnConfigReload()
		{
			UpdateTitlebar();
		}

		private void UpdateTitlebar()
		{
			if (Program.Headless || !Server.ServerConfig.SetTitleBar.Value) return;

			List<string> titleBar = new List<string> {$"MultiAdmin {Program.MaVersion}"};

			if (!string.IsNullOrEmpty(Server.serverId))
			{
				titleBar.Add($"Config: {Server.serverId}");
			}

			if (Server.SessionSocket != null)
			{
				titleBar.Add($"Console Port: {Server.SessionSocket.Port}");
			}

			if (Server.IsGameProcessRunning)
			{
				titleBar.Add($"PID: {ServerProcessId}");
			}

			try
			{
				Console.Title = string.Join(" | ", titleBar);
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(UpdateTitlebar), e);
			}
		}
	}
}
