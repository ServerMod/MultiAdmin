using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class RestartRoundCounter : Feature, IEventRoundEnd
	{
		private int count;
		private int restartAfter;

		public RestartRoundCounter(Server server) : base(server)
		{
		}

		public void OnRoundEnd()
		{
			if (restartAfter < 0) return;
			count++;

			if (count <= restartAfter) return;

			Server.Write($"{count}/{restartAfter} rounds have passed, restarting...");
			Server.SoftRestartServer();
		}

		public override void Init()
		{
			count = 0;
		}

		public override void OnConfigReload()
		{
			restartAfter = Server.ServerConfig.RestartEveryNumRounds;
		}


		public override string GetFeatureDescription()
		{
			return "Restarts the server after a number rounds completed";
		}

		public override string GetFeatureName()
		{
			return "Restart After a Number of Rounds";
		}
	}
}