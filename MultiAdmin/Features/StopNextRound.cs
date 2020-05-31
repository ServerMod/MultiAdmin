using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class StopNextRound : Feature, ICommand, IEventRoundEnd
	{
		private bool stop;

		public StopNextRound(Server server) : base(server)
		{
			stop = false;
		}

		public string GetCommandDescription()
		{
			return "Stops the server at the end of this round [Requires ServerMod]";
		}

		public void OnCall(string[] args)
		{
			Server.Write("Server will stop next round");
			stop = true;
		}

		public bool PassToGame()
		{
			return false;
		}

		public string GetCommand()
		{
			return "STOPNEXTROUND";
		}

		public string GetUsage()
		{
			return string.Empty;
		}

		public void OnRoundEnd()
		{
			if (!stop) return;

			Server.StopServer();
			stop = false;
		}

		public override void Init()
		{
			stop = false;
		}

		public override void OnConfigReload()
		{
		}

		public override string GetFeatureDescription()
		{
			return "Stops the server after the current round ends [Requires ServerMod]";
		}

		public override string GetFeatureName()
		{
			return "Stop Next Round";
		}
	}
}
