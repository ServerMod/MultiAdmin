using System;
using System.Linq;

namespace MultiAdmin
{
	internal static class InputThread
	{
		private static readonly char[] separator = {' '};

		public static void Write(Server server)
		{
			while (!server.Stopping)
			{
				string message = Console.ReadLine();
				server.Write(">>> " + message, ConsoleColor.DarkMagenta);

				string[] messageSplit = message.ToUpper().Split(separator, StringSplitOptions.RemoveEmptyEntries);
				if (messageSplit.Length == 0) continue;

				bool callServer = true;
				server.commands.TryGetValue(messageSplit[0].ToLower().Trim(), out ICommand command);
				if (command != null)
				{
					command.OnCall(messageSplit.Skip(1).Take(messageSplit.Length - 1).ToArray());
					callServer = command.PassToGame();
				}

				if (callServer) server.SendMessage(message);
			}
		}
	}
}