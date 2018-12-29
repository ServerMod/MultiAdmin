using MultiAdmin.MultiAdmin.Features.Attributes;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	internal class ConfigReload : Feature, ICommand
	{
		private bool pass;

		public ConfigReload(Server server) : base(server)
		{
		}

		public string GetCommand()
		{
			return "CONFIG";
		}

		public string GetCommandDescription()
		{
			return "Handles reloading the config";
		}

		public string GetUsage()
		{
			return "<reload>";
		}

		public void OnCall(string[] args)
		{
			if (args.Length == 0) return;
			if (args[0].ToLower().Equals("reload"))
			{
				Server.SwapConfigs();
				pass = true;
				Server.Write("Reloading config");
				Server.Write("if the config opens in notepad, dont worry, thats just the game. It should be reloaded.");
				Server.ServerConfig.Reload();
				foreach (Feature feature in Server.Features) feature.OnConfigReload();
			}
		}

		public bool PassToGame()
		{
			return pass;
		}

		public override string GetFeatureDescription()
		{
			return "Config reload will swap configs";
		}

		public override string GetFeatureName()
		{
			return "Config reload";
		}

		public override void Init()
		{
			pass = true;
		}

		public override void OnConfigReload()
		{
		}
	}
}