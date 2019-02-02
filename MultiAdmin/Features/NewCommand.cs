using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class NewCommand : Feature, ICommand, IEventServerFull
	{
		private string onFullServerId;

		public NewCommand(Server server) : base(server)
		{
		}

		public void OnCall(string[] args)
		{
			if (args.Length > 0)
			{
				string serverId = string.Join(" ", args);

				if (string.IsNullOrEmpty(serverId)) return;

				Server.Write($"Launching new server with Server ID: \"{serverId}\"...");

				Program.StartServerFromId(serverId);
			}
			else
			{
				Server.Write("Error: Missing Server ID!");
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
			onFullServerId = Server.ServerConfig.StartConfigOnFull;
		}

		public override string GetFeatureDescription()
		{
			return "Adds a command to start a new server given a config folder";
		}

		public override string GetFeatureName()
		{
			return "New";
		}

		public void OnServerFull()
		{
			if (string.IsNullOrEmpty(onFullServerId)) return;

			Server.Write($"Launching new server with Server ID: \"{onFullServerId}\" due to this server being full...");

			Program.StartServerFromId(onFullServerId);
		}
	}
}