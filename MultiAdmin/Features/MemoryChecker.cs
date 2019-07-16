using System;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class MemoryChecker : Feature, IEventTick, IEventRoundEnd
	{
		private const long BytesInMegabyte = 1048576L;

		private const int OutputPrecision = 2;

		private uint tickCount;
		private uint tickCountSoft;

		private const uint MaxTicks = 10;
		private const uint MaxTicksSoft = 10;

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
			get => LowBytes / new decimal(BytesInMegabyte);
			set => LowBytes = (long)(value * BytesInMegabyte);
		}

		public decimal LowMbSoft
		{
			get => LowBytesSoft / new decimal(BytesInMegabyte);
			set => LowBytesSoft = (long)(value * BytesInMegabyte);
		}

		public decimal MaxMb
		{
			get => MaxBytes / new decimal(BytesInMegabyte);
			set => MaxBytes = (long)(value * BytesInMegabyte);
		}

		public decimal MemoryUsedMb => new decimal(MemoryUsedBytes) / new decimal(BytesInMegabyte);
		public decimal MemoryLeftMb => new decimal(MemoryLeftBytes) / new decimal(BytesInMegabyte);

		#endregion

		public void OnRoundEnd()
		{
			if (!restart || Server.Status == ServerStatus.Restarting) return;

			Server.Write("Restarting due to low memory (Round End)...", ConsoleColor.Red);

			Server.SoftRestartServer();

			Init();
		}

		public void OnTick()
		{
			float.MaxValue

			if (LowBytes < 0 && LowBytesSoft < 0 || MaxBytes < 0) return;

			if (tickCount < MaxTicks && LowBytes >= 0 && MemoryLeftBytes <= LowBytes)
			{
				Server.Write($"Warning: Program is running low on memory ({decimal.Round(MemoryLeftMb, OutputPrecision)} MB left), the server will restart if it continues",
					ConsoleColor.Red);
				tickCount++;
			}
			else
			{
				tickCount = 0;
			}

			if (!restart && tickCountSoft < MaxTicksSoft && LowBytesSoft >= 0 && MemoryLeftBytes <= LowBytesSoft)
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

			if (tickCount >= MaxTicks)
			{
				Server.Write("Restarting due to low memory...", ConsoleColor.Red);
				Server.SoftRestartServer();

				restart = false;
			}
			else if (!restart && tickCountSoft >= MaxTicksSoft)
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
			LowMb = new decimal(Server.ServerConfig.RestartLowMemory.Value);
			LowMbSoft = new decimal(Server.ServerConfig.RestartLowMemoryRoundEnd.Value);
			MaxMb = new decimal(Server.ServerConfig.MaxMemory.Value);
		}
	}
}
