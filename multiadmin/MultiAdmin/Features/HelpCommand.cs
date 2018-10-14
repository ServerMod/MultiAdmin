﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin.MultiAdmin.Features
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

		public override void OnConfigReload()
		{
		}

		public string GetCommandDescription()
		{
			return "Prints out available commands and their function.";
		}

		public override string GetFeatureDescription()
		{
			return "Display a full list of multiadmin commands and in game commands.";
		}

		public override string GetFeatureName()
		{
			return "Help";
		}

		public override void Init()
		{
		}

		public void OnCall(string[] args)
		{
			Server.Write("Commands from MultiAdmin:");
			List<string> helpOutput = new List<string>();
			foreach (KeyValuePair<string, ICommand> command in base.Server.Commands)
			{
				string usage = command.Value.GetUsage();
				if (usage.Length > 0) usage = " " + usage;
				string output = string.Format("{0}{1}: {2}", command.Key.ToUpper(), usage, command.Value.GetCommandDescription());
				helpOutput.Add(output);
			}

			helpOutput.Sort();

			foreach (string line in helpOutput)
			{
				Server.Write(line, ConsoleColor.Green);
			}

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
	}
}
