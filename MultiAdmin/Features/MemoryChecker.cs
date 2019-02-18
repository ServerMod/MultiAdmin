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

		private uint tickCount;
		private uint tickCountSoft;

		private const uint MaxTicks = 10;
		private const uint MaxTicksSoft = 10;

		// Memory Checker Soft
		private bool restart;
		private bool warnedSoft;

		public MemoryChecker(Server server) : base(server)
		{
		}

		private float LowMb
		{
			get => lowBytes / (float) BytesInMegabyte;
			set => lowBytes = (long) (value * BytesInMegabyte);
		}

		private float LowMbSoft
		{
			get => lowBytesSoft / (float) BytesInMegabyte;
			set => lowBytesSoft = (long) (value * BytesInMegabyte);
		}

		private float MaxMb
		{
			get => maxBytes / (float) BytesInMegabyte;
			set => maxBytes = (long) (value * BytesInMegabyte);
		}

		public void OnRoundEnd()
		{
			if (!restart) return;

			Server.Write("Restarting due to low memory (Round End)...", ConsoleColor.Red);

			Server.SoftRestartServer();
			restart = false;
			warnedSoft = false;
		}

		public void OnTick()
		{
			if (lowBytes < 0 && lowBytesSoft < 0 || maxBytes < 0) return;

			Server.GameProcess.Refresh();
			long workingMemory = Server.GameProcess.WorkingSet64; // Process memory in bytes
			long memoryLeft = maxBytes - workingMemory;

			if (lowBytes >= 0 && memoryLeft <= lowBytes)
			{
				Server.Write($"Warning: Program is running low on memory ({memoryLeft / BytesInMegabyte} MB left), the server will restart if it continues",
					ConsoleColor.Red);
				tickCount++;
			}
			else
			{
				tickCount = 0;
			}

			if (lowBytesSoft >= 0 && memoryLeft <= lowBytesSoft)
			{
				Server.Write(
					$"Warning: Program is running low on memory ({memoryLeft / BytesInMegabyte} MB left), the server will restart at the end of the round if it continues",
					ConsoleColor.Red);
				tickCountSoft++;
			}
			else
			{
				tickCountSoft = 0;
			}

			if (tickCount >= MaxTicks)
			{
				Server.Write("Restarting due to low memory...", ConsoleColor.Red);
				Server.SoftRestartServer();

				restart = false;
			}
			else if (tickCountSoft >= MaxTicksSoft)
			{
				if (!warnedSoft)
					Server.Write("Server will restart at the end of the round due to low memory");

				restart = true;
				warnedSoft = true;
			}
		}

		public override void Init()
		{
			restart = false;
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