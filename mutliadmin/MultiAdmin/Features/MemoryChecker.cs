using System;
using MultiAdmin.MultiAdmin.Features.Attributes;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	internal class MemoryChecker : Feature, IEventTick
	{
		private int lowMb;
		private int maxMb;
		private int tickCount;

		public MemoryChecker(Server server) : base(server)
		{
		}

		public void OnTick()
		{
			if (lowMb >= 0 && maxMb >= 0)
			{
				Server.GetGameProcess().Refresh();
				long workingMemory = Server.GetGameProcess().WorkingSet64 / 1048576L; // process memory in MB
				long memoryLeft = maxMb - workingMemory; // 32 bit limited to 2GB

				if (memoryLeft < lowMb)
				{
					Server.Write("Warning: program is running low on memory (" + memoryLeft + " MB left)",
						ConsoleColor.Red);
					tickCount++;
				}
				else
				{
					tickCount = 0;
				}

				if (tickCount == 10)
				{
					Server.Write("Restarting due to lower memory", ConsoleColor.Red);
					Server.SoftRestartServer();
				}
			}
		}

		public override void Init()
		{
			tickCount = 0;
		}

		public override string GetFeatureDescription()
		{
			return "Restarts the server if the working memory becomes too low";
		}

		public override string GetFeatureName()
		{
			return "Restart On Low Memory";
		}

		public override void OnConfigReload()
		{
			lowMb = Server.ServerConfig.config.GetInt("restart_low_memory", 400);

			maxMb = Server.ServerConfig.config.GetInt("max_memory", 2048); // 32 bit limited to 2GB
		}
	}
}