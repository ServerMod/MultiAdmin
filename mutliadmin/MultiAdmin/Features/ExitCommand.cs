using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	class ExitCommand : Feature, ICommand
	{
		bool pass;

		public ExitCommand(Server server) : base(server)
		{
		}

		public string GetCommand()
		{
			return "EXIT";
		}

		public override void OnConfigReload()
		{
		}

		public string GetCommandDescription()
		{
			return "Exits the server";
		}

		public override string GetFeatureDescription()
		{
			return "Adds a graceful exit command.";
		}

		public override string GetFeatureName()
		{
			return "Exit command";
		}

		public string GetUsage()
		{
			return string.Empty;
		}

		public override void Init()
		{
			pass = true;
		}

		public void OnCall(string[] args)
		{
			Server.StopServer(false);
		}

		public bool PassToGame()
		{
			return pass;
		}
	}
}
