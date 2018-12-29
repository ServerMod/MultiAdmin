using MultiAdmin.MultiAdmin.Features.Attributes;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	internal class ChainStart : Feature, IEventServerStart
	{
		private bool once;

		public ChainStart(Server server) : base(server)
		{
			once = true;
		}


		public void OnServerStart()
		{
			if (string.IsNullOrWhiteSpace(Server.ConfigChain) || Server.ConfigChain.Trim().Equals("\"\"") ||
			    !once) return;

			once = false;
			Server.Write("Starting next with chained config:" + Server.ConfigChain);
			Server.NewInstance(Server.ConfigChain);
		}

		public override void Init()
		{
		}

		public override string GetFeatureDescription()
		{
			return "Automatically starts the next server after the first one is done loading.";
		}

		public override string GetFeatureName()
		{
			return "ChainStart";
		}

		public override void OnConfigReload()
		{
		}
	}
}