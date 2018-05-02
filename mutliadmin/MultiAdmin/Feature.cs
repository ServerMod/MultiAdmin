using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin
{
	public abstract class Feature
	{
		public Server Server { get; }
		public Feature(Server server)
		{
			this.Server = server;
		}

		public abstract String GetFeatureDescription();
		public abstract void OnConfigReload();
		public abstract String GetFeatureName();
		public abstract void Init();


	}
}
