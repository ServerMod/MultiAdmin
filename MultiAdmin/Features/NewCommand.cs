using System.Diagnostics;
using MultiAdmin.Utility;

namespace MultiAdmin.Features
{
	internal class NewCommand : Feature, ICommand, IEventServerFull
	{
		private string? onFullServerId;
		private Process? onFullServerInstance;

		public NewCommand(Server server) : base(server)
		{
		}

		public void OnCall(string[] args)
		{
			if (args.IsEmpty())
			{
				Server.Write("Error: Missing Server ID!");
			}
			else
			{
				string serverId = string.Join(" ", args);

				if (string.IsNullOrEmpty(serverId)) return;

				Server.Write($"Launching new server with Server ID: \"{serverId}\"...");

				Program.StartServer(new Server(serverId, args: Program.Args));
			}
		}

		public string GetCommand()
		{
			return "NEW";
		}

		public bool PassToGame()
		{
			return false;
		}

		public string GetCommandDescription()
		{
			return "Starts a new server with the given Server ID";
		}

		public string GetUsage()
		{
			return "<SERVER ID>";
		}

		public override void Init()
		{
		}

		public override void OnConfigReload()
		{
			onFullServerId = Server.ServerConfig.StartConfigOnFull.Value;
		}

		public override string GetFeatureDescription()
		{
			return
				"Adds a command to start a new server given a config folder and a config to start a new server when one is full [Config Requires Modding]";
		}

		public override string GetFeatureName()
		{
			return "New Server";
		}

		public void OnServerFull()
		{
			if (string.IsNullOrEmpty(onFullServerId)) return;

			// If a server instance has been started
			if (onFullServerInstance != null)
			{
				onFullServerInstance.Refresh();

				if (!onFullServerInstance.HasExited) return;
			}

			Server.Write($"Launching new server with Server ID: \"{onFullServerId}\" due to this server being full...");

			onFullServerInstance = Program.StartServer(new Server(onFullServerId, args: Program.Args));
		}
	}
}
