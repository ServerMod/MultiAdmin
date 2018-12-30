using System;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class MemoryCheckerSoft : Feature, IEventTick, IEventRoundEnd
	{
		private const long BytesInMegabyte = 1048576L;
		private long lowBytes;

		private long maxBytes;

		private bool restart;
		private int tickCount;
		private bool warn;

		public MemoryCheckerSoft(Server server) : base(server)
		{
		}

		private long LowMb
		{
			get => lowBytes / BytesInMegabyte;
			set => lowBytes = value * BytesInMegabyte;
		}

		private long MaxMb
		{
			get => maxBytes / BytesInMegabyte;
			set => maxBytes = value * BytesInMegabyte;
		}

		public void OnRoundEnd()
		{
			if (restart) Server.SoftRestartServer();
		}

		public void OnTick()
		{
			if (lowBytes < 0 || maxBytes < 0) return;

			Server.GameProcess.Refresh();
			long workingMemory = Server.GameProcess.WorkingSet64; // process memory in bytes
			long memoryLeft = maxBytes - workingMemory;

			if (memoryLeft <= lowBytes)
			{
				if (!warn)
					Server.Write(
						$"Warning: program is running low on memory ({memoryLeft / BytesInMegabyte} MB left) the server will restart at the end of the round if it continues",
						ConsoleColor.Red);
				warn = true;
				tickCount++;
			}
			else
			{
				warn = false;
				tickCount = 0;
			}

			if (tickCount == 10)
			{
				restart = true;
				Server.Write("Restarting the server at end of the round due to low memory");
			}
		}

		public override void Init()
		{
			tickCount = 0;
			restart = false;
			warn = false;
		}

		public override string GetFeatureDescription()
		{
			return "Restarts the server if the working memory becomes too low at the end of the round";
		}

		public override string GetFeatureName()
		{
			return "Restart On Low Memory at Round End";
		}

		public override void OnConfigReload()
		{
			LowMb = Server.serverConfig.RestartLowMemoryRoundEnd;
			MaxMb = Server.serverConfig.MaxMemory;
		}
	}
}