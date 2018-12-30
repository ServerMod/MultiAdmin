using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class RestartNextRound : Feature, ICommand, IEventRoundEnd
	{
		private bool restart;

		public RestartNextRound(Server server) : base(server)
		{
		}

		public string GetCommandDescription()
		{
			return "Restarts the server at the end of this round";
		}


		public void OnCall(string[] args)
		{
			Server.Write("Server will restart next round");
			restart = true;
		}

		public bool PassToGame()
		{
			return false;
		}

		public string GetCommand()
		{
			return "RESTARTNEXTROUND";
		}

		public string GetUsage()
		{
			return string.Empty;
		}

		public void OnRoundEnd()
		{
			if (restart) Server.SoftRestartServer();
		}

		public override void Init()
		{
			restart = false;
		}

		public bool RequiresServerMod()
		{
			return false;
		}

		public override string GetFeatureDescription()
		{
			return "Restarts the server after the current round ends";
		}

		public override string GetFeatureName()
		{
			return "Restart Next Round";
		}

		public override void OnConfigReload()
		{
		}
	}
}