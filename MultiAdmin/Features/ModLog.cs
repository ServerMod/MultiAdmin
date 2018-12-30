using System.IO;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
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
			if (!logToOwnFile) return;

			lock (this)
			{
				using (StreamWriter sw = File.AppendText(modLogLocation))
				{
					message = Server.TimeStamp(message);
					sw.WriteLine(message);
				}
			}
		}

		public override string GetFeatureDescription()
		{
			return "Logs admin messages to separate file, or prints them";
		}

		public override string GetFeatureName()
		{
			return "ModLog";
		}

		public override void Init()
		{
			logToOwnFile = false;
			modLogLocation = Server.LogFolder + Server.startDateTime + "_MODERATOR_output_log.txt";
		}

		public override void OnConfigReload()
		{
			logToOwnFile = Server.serverConfig.LogModActionsToOwnFile;
		}
	}
}