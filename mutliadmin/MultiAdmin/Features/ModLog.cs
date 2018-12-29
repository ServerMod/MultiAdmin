using System.IO;
using MultiAdmin.MultiAdmin.Features.Attributes;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	internal class ModLog : Feature, IEventAdminAction
	{
		private bool logToOwnFile;
		private string modLogLocation;

		public ModLog(Server server) : base(server)
		{
		}

		public void OnAdminAction(string message)
		{
			if (logToOwnFile)
				lock (this)
				{
					using (StreamWriter sw = File.AppendText(modLogLocation))
					{
						message = Server.Timestamp(message);
						sw.WriteLine(message);
					}
				}
		}

		public override string GetFeatureDescription()
		{
			return "Logs adming messages to seperate file, or prints them";
		}

		public override string GetFeatureName()
		{
			return "ModLog";
		}

		public override void Init()
		{
			logToOwnFile = false;
			modLogLocation = Server.LogFolder + Server.StartDateTime + "_MODERATOR_output_log.txt";
		}

		public override void OnConfigReload()
		{
			logToOwnFile = Server.ServerConfig.config.GetBool("log_mod_actions_to_own_file", false);
		}
	}
}