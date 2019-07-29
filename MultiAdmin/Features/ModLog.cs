using System.IO;
using MultiAdmin.Features.Attributes;
using MultiAdmin.Utility;

namespace MultiAdmin.Features
{
	[Feature]
	internal class ModLog : Feature, IEventAdminAction
	{
		private bool logToOwnFile;

		public ModLog(Server server) : base(server)
		{
		}

		public void OnAdminAction(string message)
		{
			if (!logToOwnFile || string.IsNullOrEmpty(Server.ModLogFile) || Server.ServerConfig.NoLog.Value) return;

			lock (this)
			{
				Directory.CreateDirectory(Server.logDir);

				using (StreamWriter sw = File.AppendText(Server.ModLogFile))
				{
					message = Utils.TimeStampMessage(message);
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
		}

		public override void OnConfigReload()
		{
			logToOwnFile = Server.ServerConfig.LogModActionsToOwnFile.Value;
		}
	}
}
