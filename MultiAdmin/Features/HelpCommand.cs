using System;
using System.Collections.Generic;
using MultiAdmin.ConsoleTools;
using MultiAdmin.Utility;

namespace MultiAdmin.Features
{
	public class HelpCommand : Feature, ICommand
	{
		private static readonly ColoredMessage helpPrefix = new("Commands from MultiAdmin:\n", ConsoleColor.Yellow);

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
			ColoredMessage[] message = new ColoredMessage[2];

			message[0] = helpPrefix;

			List<string> helpOutput = new();
			foreach (KeyValuePair<string, ICommand> command in Server.commands)
			{
				string usage = command.Value.GetUsage();
				if (!usage.IsEmpty()) usage = " " + usage;
				string output = $"{command.Key.ToUpper()}{usage}: {command.Value.GetCommandDescription()}";
				helpOutput.Add(output);
			}

			helpOutput.Sort();
			message[1] = new ColoredMessage(string.Join('\n', helpOutput), ConsoleColor.Green);

			Server.Write(message, helpPrefix.textColor);
			Server.Write("Commands from game:");
		}

		public bool PassToGame()
		{
			return true;
		}

		public string GetUsage()
		{
			return "";
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
