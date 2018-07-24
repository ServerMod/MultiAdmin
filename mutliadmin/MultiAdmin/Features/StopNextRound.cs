using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiAdmin.MultiAdmin.Features;

namespace MultiAdmin.MultiAdmin.Commands
{
    [Feature]
    class StopNextRound : Feature, ICommand, IEventRoundEnd
	{
		private Boolean stop;

		public StopNextRound(Server server) : base(server)
		{
			stop = false;
		}

		public string GetCommandDescription()
		{
			return "Stops the server at the end of this round";
		}

		public override void Init()
		{
			stop = false;
		}


		public override void OnConfigReload()
		{
		}

		public void OnCall(string[] args)
		{
			Server.Write("Server will stop next round");
			stop = true;
		}

		public void OnRoundEnd()
		{
			if (stop) base.Server.StopServer();
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
			return "Stops the server after the current round ends.";
		}

		public override string GetFeatureName()
		{
			return "Stop Next Round";
		}

		public string GetCommand()
		{
			return "STOPNEXTROUND";
		}

		public string GetUsage()
		{
			return "";
		}
	}
}
