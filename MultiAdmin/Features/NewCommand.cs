using MultiAdmin.MultiAdmin.Features.Attributes;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	internal class NewCommand : Feature, ICommand
	{
		private string config;

		public NewCommand(Server server) : base(server)
		{
		}


		public void OnCall(string[] args)
		{
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
			return "Starts a new server with the given config id";
		}


		public string GetUsage()
		{
			return "<CONFIG ID>";
		}

		public override void Init()
		{
		}

		public override void OnConfigReload()
		{
			config = Server.serverConfig.StartConfigOnFull;
		}

		public override string GetFeatureDescription()
		{
			return "Adds a command to start a new server given a config folder";
		}

		public override string GetFeatureName()
		{
			return "New";
		}
	}
}