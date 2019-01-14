using System;
using System.Linq;
using System.Threading;

namespace MultiAdmin.MultiAdmin
{
	class InputThread
	{
		private static readonly char[] separator = { ' ' };

		public static void Write(Server server)
		{
			while (!server.IsStopping())
			{
				string message = Console.ReadLine();
				server.Write(">>> " + message, ConsoleColor.DarkMagenta);

				string[] messageSplit = message.ToUpper().Split(InputThread.separator, StringSplitOptions.RemoveEmptyEntries);
				if (messageSplit.Length == 0)
				{
					continue;
				}

				ICommand command;
				Boolean callServer = true;
				server.Commands.TryGetValue(messageSplit[0].ToLower().Trim(), out command);
				if (command != null)
				{
					command.OnCall(messageSplit.Skip(1).Take(messageSplit.Length - 1).ToArray());
					callServer = command.PassToGame();
				}

				if (callServer)
				{
					server.SendMessage(message);
				}
			}
		}
	}
}
