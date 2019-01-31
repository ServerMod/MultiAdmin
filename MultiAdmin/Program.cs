using System;
using System.Collections.Generic;
using System.IO;

namespace MultiAdmin
{
	public static class Program
	{
		private static readonly List<Server> Servers = new List<Server>();

		public static string[] ServerDirectories
		{
			get
			{
				string globalServersFolder = MultiAdminConfig.GlobalServersFolder;
				return Directory.Exists(globalServersFolder) ? new string[] { } : Directory.GetDirectories(globalServersFolder);
			}
		}

		public static void Write(string message, ConsoleColor color = ConsoleColor.DarkYellow)
		{
			if (Utils.IsProcessHandleZero) return;

			Console.ForegroundColor = color;
			message = Utils.TimeStamp(message);
			Console.WriteLine(message);
			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.Black;
		}

		private static void OnExit(object sender, EventArgs e)
		{
			// TODO: Cleanup server on exit

			foreach (Server server in Servers) server.StopServer();

			//Console.WriteLine("exit");
			//Console.ReadKey();
		}

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.ProcessExit += OnExit;

			string configArg = GetParamFromArgs("");

			Servers.Add(!string.IsNullOrEmpty(configArg) ? new Server(configLocation: configArg) : new Server());

			while (true)
			{
				Servers[0].StartServer();

				if (!Servers[0].Crashed)
					break;

				Write("Game engine exited/crashed/closed/restarting", ConsoleColor.Red);
				Write("Restarting game with new session id...");
			}
		}

		public static string GetParamFromArgs(string key = null, string alias = null)
		{
			if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(alias)) return null;

			string[] args = Environment.GetCommandLineArgs();

			for (int i = 0; i < args.Length - 1; i++)
			{
				string arg = args[i].ToLower();

				if (!string.IsNullOrEmpty(key) && arg == $"--{key.ToLower()}") return args[i + 1];

				if (!string.IsNullOrEmpty(alias) && arg == $"-{alias.ToLower()}") return args[i + 1];
			}

			return null;
		}
	}
}