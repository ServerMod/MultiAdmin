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
			count++;

			// If the config value is set to an invalid value, disable this feature
			// Or if the count is less than the set number of rounds to go through
			if (restartAfter <= 0 || count < restartAfter) return;

			Server.Write($"{count}/{restartAfter} rounds have passed, restarting...");

			Server.SoftRestartServer();
			count = 0;
		}

		public override void Init()
		{
			count = 0;
		}

		public override void OnConfigReload()
		{
			restartAfter = Server.ServerConfig.RestartEveryNumRounds.Value;
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
