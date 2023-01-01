using System;
using System.Collections.Generic;

namespace MultiAdmin.Features
{
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
			return "Updates the title bar with instance based information";
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

			List<string> titleBar = new() { $"MultiAdmin {Program.MaVersion}" };

			if (!string.IsNullOrEmpty(Server.serverId))
			{
				titleBar.Add($"Config: {Server.serverId}");
			}

			if (Server.IsGameProcessRunning)
			{
				titleBar.Add($"Port: {Server.Port}");
				titleBar.Add($"PID: {ServerProcessId}");
			}

			if (Server.SessionSocket != null)
			{
				titleBar.Add($"Console Port: {Server.SessionSocket.Port}");
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
