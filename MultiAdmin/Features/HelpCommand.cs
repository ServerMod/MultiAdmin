using System;
using System.Collections.Generic;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
	public class HelpCommand : Feature, ICommand
	{
		public HelpCommand(Server server) : base(server)
		{
		}

		public string GetCommand()
		{
			return "HELP";
		}

		public string GetCommandDescription()
		{
			return "Prints out available commands and their function";
		}

		public void OnCall(string[] args)
		{
			Server.Write("Commands from MultiAdmin:");
			List<string> helpOutput = new List<string>();
			foreach (KeyValuePair<string, ICommand> command in Server.commands)
			{
				string usage = command.Value.GetUsage();
				if (usage.Length > 0) usage = " " + usage;
				string output = $"{command.Key.ToUpper()}{usage}: {command.Value.GetCommandDescription()}";
				helpOutput.Add(output);
			}

			helpOutput.Sort();

			foreach (string line in helpOutput) Server.Write(line, ConsoleColor.Green);

			Server.Write("Commands from game:");
		}

		public bool PassToGame()
		{
			return true;
		}

		public string GetUsage()
		{
			return string.Empty;
		}

		public override void OnConfigReload()
		{
		}

		public override string GetFeatureDescription()
		{
			return "Display a full list of MultiAdmin commands and in game commands";
		}

		public override string GetFeatureName()
		{
			return "Help";
		}

		public override void Init()
		{
		}
	}
}