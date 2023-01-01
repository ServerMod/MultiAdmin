using MultiAdmin.Utility;

namespace MultiAdmin.Features
{
	internal class ConfigReload : Feature, ICommand
	{
		public ConfigReload(Server server) : base(server)
		{
		}

		public string GetCommand()
		{
			return "CONFIG";
		}

		public string GetCommandDescription()
		{
			return "Reloads the configuration file";
		}

		public string GetUsage()
		{
			return "<RELOAD>";
		}

		public void OnCall(string[] args)
		{
			if (args.IsNullOrEmpty() || !args[0].ToLower().Equals("reload")) return;

			Server.Write("Reloading configs...");

			Server.ReloadConfig();

			Server.Write("MultiAdmin config has been reloaded!");
		}

		public bool PassToGame()
		{
			return true;
		}

		public override string GetFeatureDescription()
		{
			return "Reloads the MultiAdmin configuration file";
		}

		public override string GetFeatureName()
		{
			return "Config Reload";
		}

		public override void Init()
		{
		}

		public override void OnConfigReload()
		{
		}
	}
}
