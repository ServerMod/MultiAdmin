using System.Collections.Generic;
using System.IO;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	internal class GithubGenerator : Feature, ICommand
	{
		public GithubGenerator(Server server) : base(server)
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

		public string GetUsage()
		{
			return "[FILE LOCATION]";
		}

		public void OnCall(string[] args)
		{
			if (args.Length == 0)
			{
				Server.Write("You must specify the location of the file.");
				return;
			}

			string dir = string.Join(" ", args);

			List<string> lines = new List<string>
			{
				"# MultiAdmin",
				string.Empty,
				"## Features"
			};

			foreach (Feature feature in Server.features)
			{
				if (feature.Equals(this)) continue;
				lines.Add("- " + feature.GetFeatureName() + ": " + feature.GetFeatureDescription());
			}

			lines.Add(string.Empty);
			lines.Add("## MultiAdmin Commands");
			lines.Add(
				"This does not include ServerMod or in-game commands, for a full list type `HELP` in MultiAdmin which will produce all commands.");
			lines.Add(string.Empty);
			foreach (ICommand comm in Server.commands.Values)
			{
				string commandString = (comm.GetCommand() + " " + comm.GetUsage()).Trim();
				lines.Add("- " + commandString + ": " + comm.GetCommandDescription());
			}

			File.WriteAllLines(dir, lines);
		}

		public bool PassToGame()
		{
			return false;
		}

		public override void OnConfigReload()
		{
		}

		public override string GetFeatureDescription()
		{
			return "NOT INCLUDED IN FILE";
		}

		public override string GetFeatureName()
		{
			return "GITHUB GEN";
		}

		public override void Init()
		{
		}
	}
}