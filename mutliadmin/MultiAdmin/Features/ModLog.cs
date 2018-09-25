using System;
using System.IO;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	class ModLog : Feature, IEventAdminAction
	{
		private bool logToOwnFile;
		private string modLogLocation;

		public ModLog(Server server) : base(server)
		{
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
			this.modLogLocation = Server.LogFolder + Server.StartDateTime + "_MODERATOR_output_log.txt";
		}

		public void OnAdminAction(string message)
		{
			if (logToOwnFile)
			{
				lock (this)
				{
					using (StreamWriter sw = File.AppendText(this.modLogLocation))
					{
						DateTime now = DateTime.Now;
						string date = "[" + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + "] ";
						sw.WriteLine(date + message);
					}
				}
			}
		}

		public override void OnConfigReload()
		{
			logToOwnFile = Server.ServerConfig.config.GetBool("log_mod_actions_to_own_file", false);
		}
	}
}
