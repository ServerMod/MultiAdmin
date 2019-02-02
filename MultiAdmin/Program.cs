using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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
				return !Directory.Exists(globalServersFolder) ? new string[] { } : Directory.GetDirectories(globalServersFolder);
			}
		}

		public static string[] AutoStartServerDirectories => ServerDirectories.Where(serverDirectory => !new MultiAdminConfig(serverDirectory + Path.DirectorySeparatorChar + MultiAdminConfig.ConfigFileName).ManualStart).ToArray();

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

		public static void Main()
		{
			AppDomain.CurrentDomain.ProcessExit += OnExit;

			string configArg = GetParamFromArgs("config", "c");
			string serverIdArg = GetParamFromArgs("id");

			if (!string.IsNullOrEmpty(configArg))
			{
				Write($"Starting this instance with config directory: \"{configArg}\"...");

				Servers.Add(new Server(configLocation: configArg));
			}
			else if (!string.IsNullOrEmpty(serverIdArg))
			{
				Write($"Starting this instance with Server ID: \"{serverIdArg}\"...");

				Servers.Add(new Server(serverIdArg));
			}
			else
			{
				string[] serverDirectories = ServerDirectories;
				string[] autoStartServerDirectories = AutoStartServerDirectories;

				if (serverDirectories.Length <= 0 || autoStartServerDirectories.Length <= 0)
				{
					Write("Starting this instance in single server mode...");

					Servers.Add(new Server());
				}
				else
				{
					Write("Starting this instance in multi server mode...");

					string assemblyLocation = Assembly.GetEntryAssembly().Location;

					for (int i = 0; i < autoStartServerDirectories.Length; i++)
					{
						string serverId = Path.GetFileName(autoStartServerDirectories[i]);

						if (i == 0)
						{
							Write($"Starting this instance with Server ID: \"{serverId}\"...");

							Servers.Add(new Server(serverId));
						}
						else
						{
							ProcessStartInfo startInfo = new ProcessStartInfo(assemblyLocation) { Arguments = $"--id {serverId}" };

							//Write($"Launching \"{startInfo.FileName}\" with arguments \"{startInfo.Arguments}\"...");

							Process.Start(startInfo);
						}
					}
				}
			}

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