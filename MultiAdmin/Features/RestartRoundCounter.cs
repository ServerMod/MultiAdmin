namespace MultiAdmin.Features
{
	internal class RestartRoundCounter : Feature, IEventRoundEnd
	{
		private int count;
		private int restartAfter;

		public RestartRoundCounter(Server server) : base(server)
		{
		}

		public void OnRoundEnd()
		{
			// If the config value is set to an invalid value, disable this feature
			if (restartAfter <= 0)
				return;

			// If the count is less than the set number of rounds to go through
			if (++count < restartAfter)
			{
				if (Server.ServerConfig.RestartEveryNumRoundsCounting.Value)
					Server.Write($"{count}/{restartAfter} rounds have passed...");
			}
			else
			{
				Server.Write($"{count}/{restartAfter} rounds have passed, restarting...");

				Server.RestartServer();
				count = 0;
			}
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
			return "Restarts the server after a number rounds completed [Requires Modding]";
		}

		public override string GetFeatureName()
		{
			return "Restart After a Number of Rounds";
		}
	}
}
