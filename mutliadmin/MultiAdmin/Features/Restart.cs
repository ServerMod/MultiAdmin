using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Features
{
	class Restart : Feature, ICommand
	{
		public Restart(Server server) : base(server)
		{
		}

		public string GetCommand()
		{
			return "restart";
		}

		public string GetCommandDescription()
		{
			return "Restarts the game server (multiadmin will not restart, just the game)";
		}

		public override string GetFeatureDescription()
		{
			return "Allows the game to be restarted without restarting multiadmin";
		}

		public override string GetFeatureName()
		{
			return "Restart command";
		}

		public string GetUsage()
		{
			return "";
		}

		public override void Init()
		{
		}

		public void OnCall(string[] args)
		{
			this.Server.SoftRestartServer();
		}

		public override void OnConfigReload()
		{
		}

		public bool PassToGame()
		{
			return false;
		}
	}
}
