using System;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class MemoryChecker : Feature, IEventTick, IEventRoundEnd
	{
		private const long BytesInMegabyte = 1048576L;

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

		#region Memory Values

		public long LowBytes { get; set; }
		public long LowBytesSoft { get; set; }

		public long MaxBytes { get; set; }

		public long MemoryUsedBytes => Server.GameProcess.WorkingSet64;
		public long MemoryLeftBytes => MaxBytes - MemoryUsedBytes;

		public float LowMb
		{
			get => LowBytes / (float) BytesInMegabyte;
			set => LowBytes = (long) (value * BytesInMegabyte);
		}

		public float LowMbSoft
		{
			get => LowBytesSoft / (float) BytesInMegabyte;
			set => LowBytesSoft = (long) (value * BytesInMegabyte);
		}

		public float MaxMb
		{
			get => MaxBytes / (float) BytesInMegabyte;
			set => MaxBytes = (long) (value * BytesInMegabyte);
		}

		public float MemoryUsedMb => MemoryUsedBytes / (float) BytesInMegabyte;
		public float MemoryLeftMb => MemoryLeftBytes / (float) BytesInMegabyte;

		//public decimal DecimalMemoryUsedMb => DecimalDivide(MemoryUsedBytes, BytesInMegabyte, 2);
		public decimal DecimalMemoryLeftMb => DecimalDivide(MemoryLeftBytes, BytesInMegabyte, 2);

		public decimal DecimalDivide(long numerator, long denominator, int decimals)
		{
			return decimal.Round(new decimal(numerator) / new decimal(denominator), decimals);
		}

		#endregion

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
			if (LowBytes < 0 && LowBytesSoft < 0 || MaxBytes < 0) return;

			Server.GameProcess.Refresh();

			if (LowBytes >= 0 && MemoryLeftBytes <= LowBytes)
			{
				Server.Write($"Warning: Program is running low on memory ({DecimalMemoryLeftMb} MB left), the server will restart if it continues",
					ConsoleColor.Red);
				tickCount++;
			}
			else
			{
				tickCount = 0;
			}

			if (LowBytesSoft >= 0 && MemoryLeftBytes <= LowBytesSoft)
			{
				Server.Write(
					$"Warning: Program is running low on memory ({DecimalMemoryLeftMb} MB left), the server will restart at the end of the round if it continues",
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