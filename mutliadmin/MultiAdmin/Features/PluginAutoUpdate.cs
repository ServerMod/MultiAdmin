using System.Reflection;
using System.Net;
using System;
using System.ServiceModel.Web;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	class PluginAutoUpdate : Feature
	{
		private bool assemblyVerCheck;
		private bool attributeVerCheck;

		public PluginAutoUpdate(Server server) : base(server)
		{

		}

		public override void OnConfigReload()
		{

		}

		public override void Init()
		{
			CollectOnlineInfo();

		}

		public void CollectOnlineInfo()
		{
			bool httpsError = false;
			string host = "raw.githubusercontent.com/lordofkhaos/smod-plugins-ext/master/plugins.json", http = @"http://", https = @"https://";
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create("");


			while (true)
			{
				try
				{
					// do stuff
					string myHost = https + host;
					if (httpsError)
						myHost = http + host;
					break;
				}
				catch (Exception e)
				{
					// do stuff
					Server.Write($"Error in updating plugins! Error Message: {e.Message}");
					break;
				}
			}
		}

		public void CollectLocalInfo()
		{

		}

		public void CheckInfo()
		{

		}

		public override string GetFeatureName()
		{
			return "Plugin AutoUpdater";
		}

		public override string GetFeatureDescription()
		{
			return "Automatically update plugins";
		}
	}
}