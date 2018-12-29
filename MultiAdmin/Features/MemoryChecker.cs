using System;
using MultiAdmin.MultiAdmin.Features.Attributes;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	internal class MemoryChecker : Feature, IEventTick
	{
		private const long BytesInMegabyte = 1048576L;
		private long lowBytes;

		private long maxBytes;

		private int tickCount;

		public MemoryChecker(Server server) : base(server)
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

		public void OnTick()
		{
			if (lowBytes < 0 || maxBytes < 0) return;

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

			if (tickCount == 10)
			{
				Server.Write("Restarting due to lower memory", ConsoleColor.Red);
				Server.SoftRestartServer();
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
			LowMb = Server.serverConfig.RestartLowMemory;
			MaxMb = Server.serverConfig.MaxMemory;
		}
	}
}