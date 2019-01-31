using System;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class MemoryChecker : Feature, IEventTick, IEventRoundEnd
	{
		private const long BytesInMegabyte = 1048576L;

		private long lowBytes;
		private long lowBytesSoft;

		private long maxBytes;

		private int tickCount;
		private int tickCountSoft;

		// Memory Checker Soft
		private bool restart;
		private bool warnedSoft;

		public MemoryChecker(Server server) : base(server)
		{
		}

		private long LowMb
		{
			get => lowBytes / BytesInMegabyte;
			set => lowBytes = value * BytesInMegabyte;
		}

		private long LowMbSoft
		{
			get => lowBytesSoft / BytesInMegabyte;
			set => lowBytesSoft = value * BytesInMegabyte;
		}

		private long MaxMb
		{
			get => maxBytes / BytesInMegabyte;
			set => maxBytes = value * BytesInMegabyte;
		}

		public void OnRoundEnd()
		{
			if (restart)
			{
				Server.Write("Restarting due to low memory...", ConsoleColor.Red);
				Server.SoftRestartServer();
			}
		}

		public void OnTick()
		{
			if (lowBytes < 0 && lowBytesSoft < 0 || maxBytes < 0) return;

			Server.GameProcess.Refresh();
			long workingMemory = Server.GameProcess.WorkingSet64; // Process memory in bytes
			long memoryLeft = maxBytes - workingMemory;

			if (memoryLeft <= lowBytes)
			{
				Server.Write($"Warning: Program is running low on memory ({memoryLeft / BytesInMegabyte} MB left)",
					ConsoleColor.Red);
				tickCount++;
			}
			else
			{
				tickCount = 0;
			}

			if (memoryLeft <= lowBytesSoft)
			{
				if (!warnedSoft)
					Server.Write(
						$"Warning: program is running low on memory ({memoryLeft / BytesInMegabyte} MB left) the server will restart at the end of the round if it continues",
						ConsoleColor.Red);
				warnedSoft = true;
				tickCountSoft++;
			}
			else
			{
				warnedSoft = false;
				tickCountSoft = 0;
			}

			if (tickCount >= 10)
			{
				restart = false;
				Server.Write("Restarting due to low memory...", ConsoleColor.Red);
				Server.SoftRestartServer();
			}
			else if (tickCountSoft >= 10)
			{
				restart = true;
				Server.Write("Restarting the server at end of the round due to low memory");
			}
		}

		public override void Init()
		{
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
			LowMb = Server.ServerConfig.RestartLowMemory;
			LowMbSoft = Server.ServerConfig.RestartLowMemoryRoundEnd;
			MaxMb = Server.ServerConfig.MaxMemory;
		}
	}
}