﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MultiAdmin
{
	public static class Program
	{
		private static readonly List<Server> InstantiatedServers = new List<Server>();

		#region Server Properties

		public static Server[] Servers => ServerDirectories.Select(serverDir => new Server(Path.GetFileName(serverDir), serverDir)).ToArray();

		public static string[] ServerDirectories
		{
			get
			{
				string globalServersFolder = MultiAdminConfig.GlobalServersFolder;
				return !Directory.Exists(globalServersFolder) ? new string[] { } : Directory.GetDirectories(globalServersFolder);
			}
		}

		public static string[] ServerIds => Servers.Select(server => server.serverId).ToArray();

		#endregion

		#region Auto-Start Server Properties

		public static Server[] AutoStartServers => Servers.Where(server => !server.ServerConfig.ManualStart).ToArray();

		public static string[] AutoStartServerDirectories => AutoStartServers.Select(autoStartServer => autoStartServer.serverDir).ToArray();

		public static string[] AutoStartServerIds => AutoStartServers.Select(autoStartServer => autoStartServer.serverId).ToArray();

		#endregion

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

			foreach (Server server in InstantiatedServers) server.StopServer();

			//Console.WriteLine("exit");
			//Console.ReadKey();
		}

		public static void Main()
		{
			AppDomain.CurrentDomain.ProcessExit += OnExit;

			string serverIdArg = GetParamFromArgs("server-id", "id");
			string configArg = GetParamFromArgs("config", "c");

			Server server = null;

			if (!string.IsNullOrEmpty(serverIdArg) || !string.IsNullOrEmpty(configArg))
			{
				server = new Server(serverIdArg, configArg);

				InstantiatedServers.Add(server);
			}
			else
			{
				Server[] autoStartServers = AutoStartServers;

				if (autoStartServers.Length <= 0)
				{
					server = new Server();

					InstantiatedServers.Add(server);
				}
				else
				{
					Write("Starting this instance in multi server mode...");

					for (int i = 0; i < autoStartServers.Length; i++)
						if (i == 0)
						{
							server = autoStartServers[i];

							InstantiatedServers.Add(server);
						}
						else
						{
							StartServer(autoStartServers[i]);
						}
				}
			}

			if (server != null)
			{
				if (!string.IsNullOrEmpty(server.serverId) && !string.IsNullOrEmpty(server.configLocation))
					Write($"Starting this instance with Server ID: \"{server.serverId}\" and config directory: \"{server.configLocation}\"...");

				else if (!string.IsNullOrEmpty(server.serverId))
					Write($"Starting this instance with Server ID: \"{server.serverId}\"...");

				else if (!string.IsNullOrEmpty(server.configLocation))
					Write($"Starting this instance with config directory: \"{server.configLocation}\"...");

				else
					Write("Starting this instance in single server mode...");

				while (true)
				{
					server.StartServer();

					if (!server.Crashed)
						break;

					Write("Game engine exited/crashed/closed/restarting", ConsoleColor.Red);
					Write("Restarting game with new session id...");
				}
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

		public static void StartServer(Server server)
		{
			string assemblyLocation = Assembly.GetEntryAssembly().Location;

			List<string> args = new List<string>();

			if (!string.IsNullOrEmpty(server.serverId))
				args.Add($"-id \"{server.serverId}\"");

			if (!string.IsNullOrEmpty(server.configLocation))
				args.Add($"-c \"{server.configLocation}\"");

			args.RemoveAll(string.IsNullOrEmpty);

			string stringArgs = string.Join(" ", args);

			ProcessStartInfo startInfo = new ProcessStartInfo(assemblyLocation, stringArgs);

			//Write($"Launching \"{startInfo.FileName}\" with arguments \"{startInfo.Arguments}\"...");

			Process.Start(startInfo);
		}
	}
}