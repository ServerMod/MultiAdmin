using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using MultiAdmin.Config;
using MultiAdmin.ConsoleTools;
using MultiAdmin.ServerIO;

namespace MultiAdmin
{
	public static class Program
	{
		public const string MaVersion = "3.1.1";

		private static readonly List<Server> InstantiatedServers = new List<Server>();

		private static readonly string MaDebugLogDir = Utils.GetFullPathSafe("logs");
		private static readonly string MaDebugLogFile = !string.IsNullOrEmpty(MaDebugLogDir) ? Utils.GetFullPathSafe($"{MaDebugLogDir}{Path.DirectorySeparatorChar}{Utils.DateTime}_MA_{MaVersion}_debug_log.txt") : null;

		private static uint? portArg;

		#region Server Properties

		public static Server[] Servers => ServerDirectories.Select(serverDir => new Server(Path.GetFileName(serverDir), serverDir, portArg)).ToArray();

		public static string[] ServerDirectories
		{
			get
			{
				string globalServersFolder = MultiAdminConfig.GlobalConfig.ServersFolder;
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

		public static bool Headless { get; private set; }

		#region Output Printing & Logging

		public static void Write(string message, ConsoleColor color = ConsoleColor.DarkYellow)
		{
			lock (ColoredConsole.WriteLock)
			{
				if (Headless) return;

				new ColoredMessage(Utils.TimeStampMessage(message), color).WriteLine(MultiAdminConfig.GlobalConfig.UseNewInputSystem);
			}
		}

		private static bool IsDebugLogTagAllowed(string tag)
		{
			return !MultiAdminConfig.GlobalConfig.DebugLogBlacklist.Contains(tag) && (!MultiAdminConfig.GlobalConfig.DebugLogWhitelist.Any() || MultiAdminConfig.GlobalConfig.DebugLogWhitelist.Contains(tag));
		}

		public static void LogDebugException(string tag, Exception exception)
		{
			lock (MaDebugLogFile)
			{
				if (tag == null || !IsDebugLogTagAllowed(tag)) return;

				LogDebug(tag, $"Error in \"{tag}\":{Environment.NewLine}{exception}");
			}
		}

		public static void LogDebug(string tag, string message)
		{
			lock (MaDebugLogFile)
			{
				if (!MultiAdminConfig.GlobalConfig.DebugLog || string.IsNullOrEmpty(MaDebugLogFile) || tag == null || !IsDebugLogTagAllowed(tag)) return;

				Directory.CreateDirectory(MaDebugLogDir);

				using (StreamWriter sw = File.AppendText(MaDebugLogFile))
				{
					message = Utils.TimeStampMessage($"[{tag}] {message}");
					sw.Write(message);
					if (!message.EndsWith(Environment.NewLine)) sw.WriteLine();
				}
			}
		}

		#endregion

		private static void OnExit(object sender, EventArgs e)
		{
			foreach (Server server in InstantiatedServers)
			{
				try
				{
					while (server.IsRunning)
					{
						server.StopServer(true);
						Thread.Sleep(10);
					}
				}
				catch (Exception ex)
				{
					LogDebugException("OnExit", ex);
				}
			}
		}

		public static void Main()
		{
			AppDomain.CurrentDomain.ProcessExit += OnExit;

			Headless = GetFlagFromArgs("headless", "h");

			string serverIdArg = GetParamFromArgs("server-id", "id");
			string configArg = GetParamFromArgs("config", "c");
			portArg = uint.TryParse(GetParamFromArgs("port", "p"), out uint port) ? (uint?)port : null;

			Server server = null;

			if (!string.IsNullOrEmpty(serverIdArg) || !string.IsNullOrEmpty(configArg))
			{
				server = new Server(serverIdArg, configArg, portArg);

				InstantiatedServers.Add(server);
			}
			else
			{
				if (Servers.Any())
				{
					Server[] autoStartServers = AutoStartServers;

					if (autoStartServers.Any())
					{
						Write("Starting this instance in multi server mode...");

						for (int i = 0; i < autoStartServers.Length; i++)
						{
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
					else
					{
						Write("No servers are set to automatically start, please enter a Server ID to start:");
						InputThread.InputPrefix?.Write();

						server = new Server(Console.ReadLine(), port: portArg);

						InstantiatedServers.Add(server);
					}
				}
				else
				{
					server = new Server(port: portArg);

					InstantiatedServers.Add(server);
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

				server.StartServer();
			}
		}

		private static bool ArrayIsNullOrEmpty(ICollection<object> array)
		{
			return array == null || !array.Any();
		}

		public static string GetParamFromArgs(string[] keys = null, string[] aliases = null)
		{
			if (ArrayIsNullOrEmpty(keys) && ArrayIsNullOrEmpty(aliases)) return null;

			string[] args = Environment.GetCommandLineArgs();

			for (int i = 0; i < args.Length - 1; i++)
			{
				string lowArg = args[i]?.ToLower();

				if (string.IsNullOrEmpty(lowArg)) continue;

				if (!ArrayIsNullOrEmpty(keys))
				{
					if (keys.Any(key => !string.IsNullOrEmpty(key) && lowArg == $"--{key.ToLower()}"))
					{
						return args[i + 1];
					}
				}

				if (!ArrayIsNullOrEmpty(aliases))
				{
					if (aliases.Any(alias => !string.IsNullOrEmpty(alias) && lowArg == $"-{alias.ToLower()}"))
					{
						return args[i + 1];
					}
				}
			}

			return null;
		}

		public static bool ArgsContainsParam(string[] keys = null, string[] aliases = null)
		{
			foreach (string arg in Environment.GetCommandLineArgs())
			{
				string lowArg = arg?.ToLower();

				if (string.IsNullOrEmpty(lowArg)) continue;

				if (!ArrayIsNullOrEmpty(keys))
				{
					if (keys.Any(key => !string.IsNullOrEmpty(key) && lowArg == $"--{key.ToLower()}"))
					{
						return true;
					}
				}

				if (!ArrayIsNullOrEmpty(aliases))
				{
					if (aliases.Any(alias => !string.IsNullOrEmpty(alias) && lowArg == $"-{alias.ToLower()}"))
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool GetFlagFromArgs(string[] keys = null, string[] aliases = null)
		{
			if (ArrayIsNullOrEmpty(keys) && ArrayIsNullOrEmpty(aliases)) return false;

			return bool.TryParse(GetParamFromArgs(keys, aliases), out bool result) ? result : ArgsContainsParam(keys, aliases);
		}

		public static string GetParamFromArgs(string key = null, string alias = null)
		{
			return GetParamFromArgs(new string[] {key}, new string[] {alias});
		}

		public static bool ArgsContainsParam(string key = null, string alias = null)
		{
			return ArgsContainsParam(new string[] {key}, new string[] {alias});
		}

		public static bool GetFlagFromArgs(string key = null, string alias = null)
		{
			return GetFlagFromArgs(new string[] {key}, new string[] {alias});
		}

		public static void StartServer(Server server)
		{
			string assemblyLocation = Assembly.GetEntryAssembly().Location;

			List<string> args = new List<string>();

			if (!string.IsNullOrEmpty(server.serverId))
				args.Add($"-id \"{server.serverId}\"");

			if (!string.IsNullOrEmpty(server.configLocation))
				args.Add($"-c \"{server.configLocation}\"");

			if (Headless)
				args.Add("-h");

			args.RemoveAll(string.IsNullOrEmpty);

			string stringArgs = string.Join(" ", args);

			ProcessStartInfo startInfo = new ProcessStartInfo(assemblyLocation, stringArgs);

			Write($"Launching \"{startInfo.FileName}\" with arguments \"{startInfo.Arguments}\"...");

			Process.Start(startInfo);
		}
	}
}
