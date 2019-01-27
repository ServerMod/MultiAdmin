using System;
using System.Collections.Generic;
using System.IO;

namespace MultiAdmin
{
	public static class Program
	{
		public static List<Server> servers = new List<Server>();

		public static string[] ServerDirectories => Directory.GetDirectories(MultiAdminConfig.GlobalServersFolder);

		public static void Write(string message, ConsoleColor color = ConsoleColor.DarkYellow)
		{
			if (Utils.IsProcessHandleZero) return;

			Console.ForegroundColor = color;
			message = Server.TimeStamp(message);
			Console.WriteLine(message);
			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.Black;
		}

		private static void OnExit(object sender, EventArgs e)
		{
			// TODO: Cleanup server on exit
			//Console.WriteLine("exit");
			//Console.ReadKey();
		}

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.ProcessExit += OnExit;

			string configArg = GetParamFromArgs("");

			servers.Add(!string.IsNullOrEmpty(configArg) ? new Server(configLocation: configArg) : new Server());

			servers[0].StartServer();
		}

		public static string GetParamFromArgs(string key = null, string alias = null)
		{
			string[] args = Environment.GetCommandLineArgs();

			for (int i = 0; i < args.Length - 1; i++)
			{
				string arg = args[i].ToLower();

				if (!string.IsNullOrEmpty(key) && arg == $"--{key.ToLower()}")
				{
					return args[i + 1];
				}

				if (!string.IsNullOrEmpty(alias) && arg == $"-{alias.ToLower()}")
				{
					return args[i + 1];
				}
			}

			return null;
		}
	}
}