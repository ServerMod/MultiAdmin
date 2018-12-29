using MultiAdmin.MultiAdmin.Features.Attributes;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	internal class AutoScale : Feature, IEventServerFull
	{
		private string config;

		public AutoScale(Server server) : base(server)
		{
		}

		public void OnServerFull()
		{
			if (!config.Equals("disabled") && !Server.IsConfigRunning(config))
				Server.NewInstance(config);
		}

		public override void Init()
		{
		}

		public override void OnConfigReload()
		{
			config = Server.ServerConfig.config.GetString("start_config_on_full", "disabled");
		}

		public override string GetFeatureDescription()
		{
			return "Auto-starts a new server once this one becomes full. (Requires ServerMod to function fully)";
		}

		public override string GetFeatureName()
		{
			return "AutoScale";
		}
	}
}