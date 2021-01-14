using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using MultiAdmin.Config;
using MultiAdmin.ConsoleTools;
using MultiAdmin.NativeExitSignal;
using MultiAdmin.ServerIO;
using MultiAdmin.Utility;

namespace MultiAdmin
{
	public static class Program
	{
		public const string MaVersion = "3.4.0.0";
		public const string RecommendedMonoVersion = "5.18";

		private static readonly List<Server> InstantiatedServers = new List<Server>();

		private static readonly string MaDebugLogDir =
			Utils.GetFullPathSafe(MultiAdminConfig.GlobalConfig.LogLocation.Value);

		private static readonly string MaDebugLogFile = !string.IsNullOrEmpty(MaDebugLogDir)
			? Utils.GetFullPathSafe(Path.Combine(MaDebugLogDir, $"{Utils.DateTime}_MA_{MaVersion}_debug_log.txt"))
			: null;

		private static StreamWriter debugLogStream = null;

		private static uint? portArg;
		public static readonly string[] Args = Environment.GetCommandLineArgs();

		private static IExitSignal exitSignalListener;

		private static bool exited = false;
		private static readonly object ExitLock = new object();

		#region Server Properties

		public static Server[] Servers => ServerDirectories
			.Select(serverDir => new Server(Path.GetFileName(serverDir), serverDir, portArg, Args)).ToArray();

		public static string[] ServerDirectories
		{
			get
			{
				string globalServersFolder = MultiAdminConfig.GlobalConfig.ServersFolder.Value;
				return !Directory.Exists(globalServersFolder)
					? new string[] { }
					: Directory.GetDirectories(globalServersFolder);
			}
		}

		public static string[] ServerIds => Servers.Select(server => server.serverId).ToArray();

		#endregion

		#region Auto-Start Server Properties

		public static Server[] AutoStartServers =>
			Servers.Where(server => !server.ServerConfig.ManualStart.Value).ToArray();

		public static string[] AutoStartServerDirectories =>
			AutoStartServers.Select(autoStartServer => autoStartServer.serverDir).ToArray();

		public static string[] AutoStartServerIds =>
			AutoStartServers.Select(autoStartServer => autoStartServer.serverId).ToArray();

		#endregion

		public static bool Headless { get; private set; }

		#region Output Printing & Logging

		public static void Write(string message, ConsoleColor color = ConsoleColor.DarkYellow)
		{
			lock (ColoredConsole.WriteLock)
			{
				if (Headless) return;

				new ColoredMessage(Utils.TimeStampMessage(message), color).WriteLine((!MultiAdminConfig.GlobalConfig?.HideInput?.Value ?? false) &&
					(MultiAdminConfig.GlobalConfig?.UseNewInputSystem?.Value ?? false));
			}
		}

		private static bool IsDebugLogTagAllowed(string tag)
		{
			return (!MultiAdminConfig.GlobalConfig?.DebugLogBlacklist?.Value?.Contains(tag) ?? true) &&
			       ((MultiAdminConfig.GlobalConfig?.DebugLogWhitelist?.Value?.IsEmpty() ?? true) ||
			        MultiAdminConfig.GlobalConfig.DebugLogWhitelist.Value.Contains(tag));
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
				try
				{
					if ((!MultiAdminConfig.GlobalConfig?.DebugLog?.Value ?? true) ||
					    string.IsNullOrEmpty(MaDebugLogFile) || tag == null || !IsDebugLogTagAllowed(tag)) return;

					Directory.CreateDirectory(MaDebugLogDir);

					// Assign debug log stream as needed
					if (debugLogStream == null)
						debugLogStream = File.AppendText(MaDebugLogFile);

					message = Utils.TimeStampMessage($"[{tag}] {message}");
					debugLogStream.Write(message);
					if (!message.EndsWith(Environment.NewLine)) debugLogStream.WriteLine();

					debugLogStream.Flush();
				}
				catch (Exception e)
				{
					new ColoredMessage[]
					{
						new ColoredMessage("Error while logging for MultiAdmin debug:", ConsoleColor.Red),
						new ColoredMessage(e.ToString(), ConsoleColor.Red)
					}.WriteLines();
				}
			}
		}

		#endregion

		private static void OnExit(object sender, EventArgs e)
		{
			lock (ExitLock)
			{
				if (exited)
					return;

				if (MultiAdminConfig.GlobalConfig.SafeServerShutdown.Value)
				{
					Write("Stopping servers and exiting MultiAdmin...", ConsoleColor.DarkMagenta);

					foreach (Server server in InstantiatedServers)
					{
						if (!server.IsGameProcessRunning)
							continue;

						try
						{
							Write(
								string.IsNullOrEmpty(server.serverId)
									? "Stopping the default server..."
									: $"Stopping server with ID \"{server.serverId}\"...", ConsoleColor.DarkMagenta);

							server.StopServer();

							// Wait for server to exit
							int timeToWait = Math.Max(server.ServerConfig.SafeShutdownCheckDelay.Value, 0);
							int timeWaited = 0;

							while (server.IsGameProcessRunning)
							{
								Thread.Sleep(timeToWait);
								timeWaited += timeToWait;

								if (timeWaited >= server.ServerConfig.SafeShutdownTimeout.Value)
								{
									Write(
										string.IsNullOrEmpty(server.serverId)
											? $"Failed to stop the default server within {timeWaited} ms, giving up..."
											: $"Failed to stop server with ID \"{server.serverId}\" within {timeWaited} ms, giving up...",
										ConsoleColor.Red);
									break;
								}
							}
						}
						catch (Exception ex)
						{
							LogDebugException(nameof(OnExit), ex);
						}
					}
				}

				debugLogStream?.Close();
				debugLogStream = null;

				exited = true;
			}
		}

		public static void Main()
		{
			if (MultiAdminConfig.GlobalConfig.SafeServerShutdown.Value)
			{
				AppDomain.CurrentDomain.ProcessExit += OnExit;

				if (Utils.IsUnix)
					exitSignalListener = new UnixExitSignal();
				else if (Utils.IsWindows)
					exitSignalListener = new WinExitSignal();

				if (exitSignalListener != null)
					exitSignalListener.Exit += OnExit;
			}

			// Remove executable path
			if (Args.Length > 0)
				Args[0] = null;

			Headless = GetFlagFromArgs(Args, "headless", "h");

			if (!Headless)
				CheckMonoVersion();

			string serverIdArg = GetParamFromArgs(Args, "server-id", "id");
			string configArg = GetParamFromArgs(Args, "config", "c");
			portArg = uint.TryParse(GetParamFromArgs(Args, "port", "p"), out uint port) ? (uint?)port : null;

			Server server = null;

			if (!string.IsNullOrEmpty(serverIdArg) || !string.IsNullOrEmpty(configArg))
			{
				server = new Server(serverIdArg, configArg, portArg, Args);

				InstantiatedServers.Add(server);
			}
			else
			{
				if (Servers.IsEmpty())
				{
					server = new Server(port: portArg, args: Args);

					InstantiatedServers.Add(server);
				}
				else
				{
					Server[] autoStartServers = AutoStartServers;

					if (autoStartServers.IsEmpty())
					{
						Write("No servers are set to automatically start, please enter a Server ID to start:");
						InputHandler.InputPrefix?.Write();

						server = new Server(Console.ReadLine(), port: portArg, args: Args);

						InstantiatedServers.Add(server);
					}
					else
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
				}
			}

			if (server != null)
			{
				if (!string.IsNullOrEmpty(server.serverId) && !string.IsNullOrEmpty(server.configLocation))
					Write(
						$"Starting this instance with Server ID: \"{server.serverId}\" and config directory: \"{server.configLocation}\"...");

				else if (!string.IsNullOrEmpty(server.serverId))
					Write($"Starting this instance with Server ID: \"{server.serverId}\"...");

				else if (!string.IsNullOrEmpty(server.configLocation))
					Write($"Starting this instance with config directory: \"{server.configLocation}\"...");

				else
					Write("Starting this instance in single server mode...");

				server.StartServer();
			}
		}

		public static string GetParamFromArgs(string[] args, string[] keys = null, string[] aliases = null)
		{
			bool hasKeys = !keys.IsNullOrEmpty();
			bool hasAliases = !aliases.IsNullOrEmpty();

			if (!hasKeys && !hasAliases) return null;

			for (int i = 0; i < args.Length - 1; i++)
			{
				string lowArg = args[i]?.ToLower();

				if (string.IsNullOrEmpty(lowArg)) continue;

				if (hasKeys)
				{
					if (keys.Any(key => lowArg == $"--{key?.ToLower()}"))
					{
						string value = args[i + 1];

						args[i] = null;
						args[i + 1] = null;

						return value;
					}
				}

				if (hasAliases)
				{
					if (aliases.Any(alias => lowArg == $"-{alias?.ToLower()}"))
					{
						string value = args[i + 1];

						args[i] = null;
						args[i + 1] = null;

						return value;
					}
				}
			}

			return null;
		}

		public static bool ArgsContainsParam(string[] args, string[] keys = null, string[] aliases = null)
		{
			bool hasKeys = !keys.IsNullOrEmpty();
			bool hasAliases = !aliases.IsNullOrEmpty();

			if (!hasKeys && !hasAliases) return false;

			for (int i = 0; i < args.Length; i++)
			{
				string lowArg = args[i]?.ToLower();

				if (string.IsNullOrEmpty(lowArg)) continue;

				if (hasKeys)
				{
					if (keys.Any(key => lowArg == $"--{key?.ToLower()}"))
					{
						args[i] = null;
						return true;
					}
				}

				if (hasAliases)
				{
					if (aliases.Any(alias => lowArg == $"-{alias?.ToLower()}"))
					{
						args[i] = null;
						return true;
					}
				}
			}

			return false;
		}

		public static bool GetFlagFromArgs(string[] args, string[] keys = null, string[] aliases = null)
		{
			if (keys.IsNullOrEmpty() && aliases.IsNullOrEmpty()) return false;

			return bool.TryParse(GetParamFromArgs(args, keys, aliases), out bool result)
				? result
				: ArgsContainsParam(args, keys, aliases);
		}

		public static string GetParamFromArgs(string[] args, string key = null, string alias = null)
		{
			return GetParamFromArgs(args, new string[] {key}, new string[] {alias});
		}

		public static bool ArgsContainsParam(string[] args, string key = null, string alias = null)
		{
			return ArgsContainsParam(args, new string[] {key}, new string[] {alias});
		}

		public static bool GetFlagFromArgs(string[] args, string key = null, string alias = null)
		{
			return GetFlagFromArgs(args, new string[] {key}, new string[] {alias});
		}

		public static Process StartServer(Server server)
		{
			string assemblyLocation = Assembly.GetEntryAssembly()?.Location;

			if (string.IsNullOrEmpty(assemblyLocation))
			{
				Write("Error while starting new server: Could not find the executable location!", ConsoleColor.Red);
			}

			List<string> args = new List<string>(server.args);

			if (!string.IsNullOrEmpty(server.serverId))
			{
				args.Add("-id");
				args.Add(server.serverId);
			}

			if (!string.IsNullOrEmpty(server.configLocation))
			{
				args.Add("-c");
				args.Add(server.configLocation);
			}

			if (Headless)
				args.Add("-h");

			ProcessStartInfo startInfo = new ProcessStartInfo(assemblyLocation, args.JoinArgs());

			Write($"Launching \"{startInfo.FileName}\" with arguments \"{startInfo.Arguments}\"...");

			Process serverProcess = Process.Start(startInfo);

			InstantiatedServers.Add(server);

			return serverProcess;
		}

		private static bool IsVersionFormat(string input, char separator = '.')
		{
			foreach (char character in input)
			{
				if (!char.IsNumber(character) && character != separator)
					return false;
			}

			return true;
		}

		public static void CheckMonoVersion()
		{
			try
			{
				string monoVersionRaw = Type.GetType("Mono.Runtime")
					?.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null)
					?.ToString();
				string monoVersion = monoVersionRaw?.Split(' ').FirstOrDefault(version => IsVersionFormat(version));

				if (string.IsNullOrEmpty(monoVersion))
					return;

				int versionDifference = Utils.CompareVersionStrings(monoVersion, RecommendedMonoVersion);

				if (versionDifference >= 0)
					return;

				Write(
					$"Warning: Your Mono version ({monoVersion}) is below the minimum recommended version ({RecommendedMonoVersion})",
					ConsoleColor.Red);
				Write("Please update your Mono installation: https://www.mono-project.com/download/stable/",
					ConsoleColor.Red);
			}
			catch (Exception e)
			{
				LogDebugException(nameof(CheckMonoVersion), e);
			}
		}
	}
}
