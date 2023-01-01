namespace MultiAdmin.Features
{
	internal class ExitCommand : Feature, ICommand
	{
		public ExitCommand(Server server) : base(server)
		{
		}

		public string GetCommand()
		{
			return "EXIT";
		}

		public string GetCommandDescription()
		{
			return "Exits the server";
		}

		public string GetUsage()
		{
			return "";
		}

		public void OnCall(string[] args)
		{
			Server.StopServer();
		}

		public bool PassToGame()
		{
			return false;
		}

		public override void OnConfigReload()
		{
		}

		public override string GetFeatureDescription()
		{
			return "Adds a graceful exit command";
		}

		public override string GetFeatureName()
		{
			return "Exit Command";
		}

		public override void Init()
		{
		}
	}
}
