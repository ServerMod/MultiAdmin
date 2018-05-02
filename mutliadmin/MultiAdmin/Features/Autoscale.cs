using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Commands
{
	class Autoscale : Feature, IEventServerFull
	{
		private String config;

		public Autoscale(Server server) : base(server)
		{
		}

		public override void Init()
		{
		}

		public override void OnConfigReload()
		{
			config = Server.ServerConfig.GetValue("START_CONFIG_ON_FULL", "disabled");
		}

		public override string GetFeatureDescription()
		{
			return "Auto-starts a new server once this one becomes full. (Requires servermod to function fully)";
		}

		public override string GetFeatureName()
		{
			return "Autoscale";
		}

		public void OnServerFull()
		{
			if (!config.Equals("disabled"))
			{
				if (!Server.IsConfigRunning(config))
				{
					Server.NewInstance(config);
				}
			}
		}
	}
}
