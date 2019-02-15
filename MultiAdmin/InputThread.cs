using System;
using System.Linq;
using System.Threading;

namespace MultiAdmin
{
	public static class InputThread
	{
		private static readonly char[] Separator = {' '};

		public static void Write(Server server)
		{
			while (server.IsRunning)
			{
				if (Program.Headless)
				{
					Thread.Sleep(5000);
					continue;
				}

				string message = Console.ReadLine();

				if (message == null)
				{
					Thread.Sleep(5000);
					continue;
				}

				server.Write(">>> " + message, ConsoleColor.DarkMagenta);

				string[] messageSplit = message.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
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