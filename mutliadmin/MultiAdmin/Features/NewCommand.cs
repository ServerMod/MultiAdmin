using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Commands
{
	class NewCommand : Feature, ICommand
	{
		private String config;

		public NewCommand(Server server) : base(server)
		{
		}

		public override void Init()
		{
		}

		public override void OnConfigReload()
		{
			config = Server.ServerConfig.config.GetString("start_config_on_full", "disabled");
		}

		public override string GetFeatureDescription()
		{
			return "Adds a command to start a new server given a config folder.";
		}

		public override string GetFeatureName()
		{
			return "New";
		}


		public void OnCall(string[] args)
		{
			if (args.Length == 0)
			{
				Server.Write("Must provide a config ID", ConsoleColor.Magenta);
			}
			else
			{
				// maybe check if the config exists?
				Server.NewInstance(args[0].ToLower());
			}
		}

		public string GetCommand()
		{
			return "NEW";
		}

		public bool PassToGame()
		{
			return false;
		}

		public string GetCommandDescription()
		{
			return "Starts a new server with the given config id.";
		}


		public string GetUsage()
		{
			return "<config_id>";
		}
	}
}
