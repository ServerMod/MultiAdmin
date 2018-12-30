using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class ExitCommand : Feature, ICommand
	{
		private bool pass;

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
			return string.Empty;
		}

		public void OnCall(string[] args)
		{
			Server.StopServer(false);
		}

		public bool PassToGame()
		{
			return pass;
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
			pass = true;
		}
	}
}