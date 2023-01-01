using System;

namespace MultiAdmin.Features
{
	internal class MemoryChecker : Feature, IEventTick, IEventRoundEnd
	{
		private const decimal BytesInMegabyte = 1048576;

		private const int OutputPrecision = 2;

		private uint tickCount;
		private uint tickCountSoft;

		private uint maxTicks = 10;
		private uint maxTicksSoft = 10;

		private bool restart;

		public MemoryChecker(Server server) : base(server)
		{
		}

		#region Memory Values

		public long LowBytes { get; set; }
		public long LowBytesSoft { get; set; }

		public long MaxBytes { get; set; }

		public long MemoryUsedBytes
		{
			get
			{
				if (Server.GameProcess == null)
					return 0;

				Server.GameProcess.Refresh();

				return Server.GameProcess.WorkingSet64;
			}
		}

		public long MemoryLeftBytes => MaxBytes - MemoryUsedBytes;

		public decimal LowMb
		{
			get => decimal.Divide(LowBytes, BytesInMegabyte);
			set => LowBytes = (long)decimal.Multiply(value, BytesInMegabyte);
		}

		public decimal LowMbSoft
		{
			get => decimal.Divide(LowBytesSoft, BytesInMegabyte);
			set => LowBytesSoft = (long)decimal.Multiply(value, BytesInMegabyte);
		}

		public decimal MaxMb
		{
			get => decimal.Divide(MaxBytes, BytesInMegabyte);
			set => MaxBytes = (long)decimal.Multiply(value, BytesInMegabyte);
		}

		public decimal MemoryUsedMb => decimal.Divide(MemoryUsedBytes, BytesInMegabyte);
		public decimal MemoryLeftMb => decimal.Divide(MemoryLeftBytes, BytesInMegabyte);

		#endregion

		public void OnRoundEnd()
		{
			if (!restart || Server.IsStopping) return;

			Server.Write("Restarting due to low memory (Round End)...", ConsoleColor.Red);

			Server.RestartServer();

			Init();
		}

		public void OnTick()
		{
			if (LowBytes < 0 && LowBytesSoft < 0 || MaxBytes < 0) return;

			if (tickCount < maxTicks && LowBytes >= 0 && MemoryLeftBytes <= LowBytes)
			{
				Server.Write(
					$"Warning: Program is running low on memory ({decimal.Round(MemoryLeftMb, OutputPrecision)} MB left), the server will restart if it continues",
					ConsoleColor.Red);
				tickCount++;
			}
			else
			{
				tickCount = 0;
			}

			if (!restart && tickCountSoft < maxTicksSoft && LowBytesSoft >= 0 && MemoryLeftBytes <= LowBytesSoft)
			{
				Server.Write(
					$"Warning: Program is running low on memory ({decimal.Round(MemoryLeftMb, OutputPrecision)} MB left), the server will restart at the end of the round if it continues",
					ConsoleColor.Red);
				tickCountSoft++;
			}
			else
			{
				tickCountSoft = 0;
			}

			if (Server.Status == ServerStatus.Restarting) return;

			if (tickCount >= maxTicks)
			{
				Server.Write("Restarting due to low memory...", ConsoleColor.Red);
				Server.RestartServer();

				restart = false;
			}
			else if (!restart && tickCountSoft >= maxTicksSoft)
			{
				Server.Write("Server will restart at the end of the round due to low memory");

				restart = true;
			}
		}

		public override void Init()
		{
			tickCount = 0;
			tickCountSoft = 0;

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
			maxTicks = Server.ServerConfig.RestartLowMemoryTicks.Value;
			maxTicksSoft = Server.ServerConfig.RestartLowMemoryRoundEndTicks.Value;

			LowMb = Server.ServerConfig.RestartLowMemory.Value;
			LowMbSoft = Server.ServerConfig.RestartLowMemoryRoundEnd.Value;
			MaxMb = Server.ServerConfig.MaxMemory.Value;
		}
	}
}
