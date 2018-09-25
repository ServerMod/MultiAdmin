using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Features
{
	[Feature]
	class GithubGenerator : Feature, ICommand
	{
		public GithubGenerator(Server server) : base(server)
		{
		}

		public override void OnConfigReload()
		{
		}

		public string GetCommand()
		{
			return "GITHUBGEN";
		}

		public string GetCommandDescription()
		{
			return "Generates a github .md file outlining all the features/commands";
		}

		public override string GetFeatureDescription()
		{
			return "NOT INCLUDED IN FILE";
		}

		public override string GetFeatureName()
		{
			return "GITHUB GEN";
		}

		public string GetUsage()
		{
			return "[filelocation]";
		}

		public override void Init()
		{
		}

		public void OnCall(string[] args)
		{
			if (args.Length == 0)
			{
				Server.Write("You must specify the location of the file.");
				return;
			}
			var dir = String.Join(" ", args);

			List<String> lines = new List<String>();
			lines.Add("# MultiAdmin");
			lines.Add(String.Empty);
			lines.Add("## Features");

			foreach (Feature feature in Server.Features)
			{
				if (feature.Equals(this)) continue;
				lines.Add("- " + feature.GetFeatureName() + ": " + feature.GetFeatureDescription());
			}
			lines.Add("## MultiAdmin Commands");
			lines.Add("This does not include ServerMod or ingame commands, for a full list type HELP in multiadmin which will produce all commands.");
			lines.Add(String.Empty);
			foreach (ICommand comm in Server.Commands.Values)
			{
				var commandString = (comm.GetCommand() + " " + comm.GetUsage()).Trim();
				lines.Add("- " + commandString + ": " + comm.GetCommandDescription());
			}
			File.WriteAllLines(dir, lines);
		}

		public bool PassToGame()
		{
			return false;
		}
	}
}
