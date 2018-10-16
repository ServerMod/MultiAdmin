using System;
using MultiAdmin.MultiAdmin.Features;

namespace MultiAdmin.MultiAdmin.Commands
{
	[Feature]
	class ChainStart : Feature, IEventServerStart
	{
		private bool once;

		public ChainStart(Server server) : base(server)
		{
			this.once = true;
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


		public void OnServerStart()
		{
			if (String.IsNullOrWhiteSpace(Server.ConfigChain) || Server.ConfigChain.Trim().Equals("\"\"") || !this.once) {
				return;
			}

			this.once = false;
			Server.Write("Starting next with chained config:" + Server.ConfigChain);
			Server.NewInstance(Server.ConfigChain);
		}
	}
}
