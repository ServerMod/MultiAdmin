using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiAdmin.MultiAdmin.Features;

namespace MultiAdmin.MultiAdmin.Commands
{
	[Feature]
	class RestartNextRound : Feature, ICommand, IEventRoundEnd
	{
		private bool restart;

		public RestartNextRound(Server server) : base(server)
		{
		}

		public override void Init()
		{
			restart = false;
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

		public void OnRoundEnd()
		{
			if (restart) base.Server.SoftRestartServer();
		}

		public bool PassToGame()
		{
			return false;
		}

		public bool RequiresServerMod()
		{
			return false;
		}

		public override string GetFeatureDescription()
		{
			return "Restarts the server after the current round ends.";
		}

		public override string GetFeatureName()
		{
			return "Restart Next Round";
		}

		public string GetCommand()
		{
			return "RESTARTNEXTROUND";
		}

		public string GetUsage()
		{
			return String.Empty;
		}

		public override void OnConfigReload()
		{
		}
	}
}
