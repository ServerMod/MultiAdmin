using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using MultiAdmin.Config;
using MultiAdmin.ConsoleTools;
using MultiAdmin.Features.Attributes;
using MultiAdmin.ServerIO;
using MultiAdmin.Utility;

namespace MultiAdmin
{
	public class Server
	{
		public readonly Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

		public readonly List<Feature> features = new List<Feature>();

		// We want a tick only list since its the only event that happens constantly, all the rest can be in a single list
		private readonly List<IEventTick> tick = new List<IEventTick>();

		private readonly MultiAdminConfig serverConfig;
		public MultiAdminConfig ServerConfig => serverConfig ?? MultiAdminConfig.GlobalConfig;

		public readonly string serverId;
		public readonly string configLocation;
		public readonly uint? port;
		public readonly string serverDir;
		public readonly string logDir;

		public bool hasServerMod;

		public string serverModBuild;
		public string serverModVersion;

		private int logId;

		private DateTime initStopTimeoutTime;
		private DateTime initRestartTimeoutTime;

		public Server(string serverId = null, string configLocation = null, uint? port = null)
		{
			this.serverId = serverId;
			serverDir = string.IsNullOrEmpty(this.serverId) ? null : Utils.GetFullPathSafe(Path.Combine(MultiAdminConfig.GlobalConfig.ServersFolder.Value, this.serverId));

			this.configLocation = Utils.GetFullPathSafe(configLocation) ?? Utils.GetFullPathSafe(MultiAdminConfig.GlobalConfig.ConfigLocation.Value) ?? Utils.GetFullPathSafe(serverDir);

			// Load config
			serverConfig = MultiAdminConfig.GlobalConfig;

			// Load config hierarchy
			string serverConfigLocation = this.configLocation;
			while (!string.IsNullOrEmpty(serverConfigLocation))
			{
				// Update the Server object's config location with the valid config location
				this.configLocation = serverConfigLocation;

				// Load the child MultiAdminConfig
				serverConfig = new MultiAdminConfig(Path.Combine(serverConfigLocation, MultiAdminConfig.ConfigFileName), serverConfig);

				// Set the server config location to the value from the config, this should be empty or null if there is no valid value
				serverConfigLocation = Utils.GetFullPathSafe(serverConfig.ConfigLocation.Value);

				// If the config hierarchy already contains the MultiAdmin config from the target path, stop looping
				// Without this, a user could unintentionally cause a lockup when their server starts up due to infinite looping
				if (serverConfig.ConfigHierarchyContainsPath(serverConfigLocation))
					break;
			}

			// Set port
			this.port = port;

			logDir = Utils.GetFullPathSafe(Path.Combine(string.IsNullOrEmpty(serverDir) ? string.Empty : serverDir, "logs"));

			// Register all features
			RegisterFeatures();
		}

		#region Server Status

		public ServerStatus LastStatus { get; private set; } = ServerStatus.NotStarted;

		private ServerStatus status = ServerStatus.NotStarted;

		public ServerStatus Status
		{
			get => status;
			private set
			{
				LastStatus = status;
				status = value;
			}
		}

		public bool IsStopped => Status == ServerStatus.NotStarted || Status == ServerStatus.Stopped || Status == ServerStatus.StoppedUnexpectedly;
		public bool IsRunning => !IsStopped;
		public bool IsStarted => !IsStopped && !IsStarting;

		public bool IsStarting => Status == ServerStatus.Starting;
		public bool IsStopping => Status == ServerStatus.Stopping || Status == ServerStatus.ForceStopping || Status == ServerStatus.Restarting;

		public bool IsLoading { get; set; }

		#endregion

		private string startDateTime;

		public string StartDateTime
		{
			get => startDateTime;

			private set
			{
				startDateTime = value;

				// Update related variables
				LogDirFile = string.IsNullOrEmpty(value) || string.IsNullOrEmpty(logDir) ? null : $"{Path.Combine(logDir, value)}_{{0}}_output_log.txt";

				lock (this)
				{
					MaLogFile = string.IsNullOrEmpty(LogDirFile) ? null : string.Format(LogDirFile, "MA");
					ScpLogFile = string.IsNullOrEmpty(LogDirFile) ? null : string.Format(LogDirFile, "SCP");
					ModLogFile = string.IsNullOrEmpty(LogDirFile) ? null : string.Format(LogDirFile, "MODERATOR");
				}
			}
		}

		public bool CheckStopTimeout => (DateTime.Now - initStopTimeoutTime).Seconds > ServerConfig.ServerStopTimeout.Value;
		public bool CheckRestartTimeout => (DateTime.Now - initRestartTimeoutTime).Seconds > ServerConfig.ServerRestartTimeout.Value;

		public string LogDirFile { get; private set; }
		public string MaLogFile { get; private set; }
		public string ScpLogFile { get; private set; }
		public string ModLogFile { get; private set; }

		public Process GameProcess { get; private set; }

		public bool IsGameProcessRunning
		{
			get
			{
				if (GameProcess == null)
					return false;

				GameProcess.Refresh();

				return !GameProcess.HasExited;
			}
		}


		public static readonly string DedicatedDir = Utils.GetFullPathSafe(Path.Combine("SCPSL_Data", "Dedicated"));

		private string sessionId;

		public string SessionId
		{
			get => sessionId;

			private set
			{
				sessionId = value;

				// Update related variables
				SessionDirectory = string.IsNullOrEmpty(value) ? null : Path.Combine(DedicatedDir, value);
			}
		}

		public string SessionDirectory { get; private set; }

		#region Server Core

		private void MainLoop()
		{
			Stopwatch timer = new Stopwatch();
			while (IsGameProcessRunning)
			{
				timer.Reset();
				timer.Start();

				foreach (IEventTick tickEvent in tick) tickEvent.OnTick();

				timer.Stop();

				// Wait 1 second per tick (calculating how long the tick took and compensating)
				Thread.Sleep(Math.Max(1000 - timer.Elapsed.Milliseconds, 0));

				if (Status == ServerStatus.Restarting && CheckRestartTimeout)
				{
					Write("Server restart timed out, killing the server process...", ConsoleColor.Red);
					if (IsGameProcessRunning)
						GameProcess.Kill();
				}

				if (Status == ServerStatus.Stopping && CheckStopTimeout)
				{
					Write("Server exit timed out, killing the server process...", ConsoleColor.Red);
					StopServer(true);
				}

				if (Status == ServerStatus.ForceStopping)
				{
					Write("Force stopping the server process...", ConsoleColor.Red);
					StopServer(true);
				}
			}
		}

		/// <summary>
		/// Sends the string <paramref name="message" /> to the SCP: SL server process.
		/// </summary>
		/// <param name="message"></param>
		public void SendMessage(string message)
		{
			if (!Directory.Exists(SessionDirectory))
			{
				Write($"Send Message error: Sending {message} failed. \"{SessionDirectory}\" does not exist!\nSkipping...");
				return;
			}

			string file = Path.Combine(SessionDirectory, $"cs{logId}.mapi");
			if (File.Exists(file))
			{
				Write($"Send Message error: Sending {message} failed. \"{file}\" already exists!\nSkipping...");
				logId++;
				return;
			}

			StreamWriter streamWriter = new StreamWriter(file);
			logId++;
			streamWriter.WriteLine(message + "terminator");
			streamWriter.Close();
			Write("Sending request to SCP: Secret Laboratory...", ConsoleColor.White);
		}

		#endregion

		#region Server Execution Controls

		public void WriteConfigInformation()
		{
			if (!string.IsNullOrEmpty(MultiAdminConfig.GlobalConfigFilePath))
				Write($"Using global config \"{MultiAdminConfig.GlobalConfigFilePath}\"...");

			if (ServerConfig != null)
			{
				foreach (MultiAdminConfig config in ServerConfig.GetConfigHierarchy())
				{
					if (!string.IsNullOrEmpty(config?.Config?.ConfigPath) && MultiAdminConfig.GlobalConfigFilePath != config.Config.ConfigPath)
						Write($"Using server config \"{config.Config.ConfigPath}\"...");
				}
			}
		}

		public string GetExecutablePath()
		{
			string scpslExe;

			if (Utils.IsUnix)
				scpslExe = "SCPSL.x86_64";
			else if (Utils.IsWindows)
				scpslExe = "SCPSL.exe";
			else
				throw new FileNotFoundException("Invalid OS, can't run executable");

			if (!File.Exists(scpslExe))
				throw new FileNotFoundException($"Can't find game executable \"{scpslExe}\", the working directory must be the game directory");

			return scpslExe;
		}

		public void StartServer(bool restartOnCrash = true)
		{
			if (!IsStopped) throw new Exceptions.ServerAlreadyRunningException();

			bool shouldRestart = false;

			do
			{
				Status = ServerStatus.Starting;
				IsLoading = true;

				SessionId = DateTime.Now.Ticks.ToString();
				StartDateTime = Utils.DateTime;

				try
				{
					#region Startup Info Printing & Logging

					WriteConfigInformation();

					#endregion

					// Reload the config immediately as server is starting
					ReloadConfig();

					// Create session directory
					PrepareSession();

					// Init features
					InitFeatures();

					string scpslExe = GetExecutablePath();

					Write($"Executing \"{scpslExe}\"...", ConsoleColor.DarkGreen);

					List<string> scpslArgs = new List<string>
					{
						"-batchmode",
						"-nographics",
						"-silent-crashes",
						"-nodedicateddelete",
						$"-key{SessionId}",
						$"-id{Process.GetCurrentProcess().Id}",
						$"-port{port ?? ServerConfig.Port.Value}"
					};

					if (string.IsNullOrEmpty(ScpLogFile) || ServerConfig.NoLog.Value)
					{
						scpslArgs.Add("-nolog");

						if (Utils.IsUnix)
							scpslArgs.Add("-logFile \"/dev/null\"");
						else if (Utils.IsWindows)
							scpslArgs.Add("-logFile \"NUL\"");
					}
					else
					{
						scpslArgs.Add($"-logFile \"{ScpLogFile}\"");
					}

					if (ServerConfig.DisableConfigValidation.Value)
					{
						scpslArgs.Add("-disableconfigvalidation");
					}

					if (ServerConfig.ShareNonConfigs.Value)
					{
						scpslArgs.Add("-sharenonconfigs");
					}

					if (!string.IsNullOrEmpty(configLocation))
					{
						scpslArgs.Add($"-configpath \"{configLocation}\"");
					}

					string appDataPath = Utils.GetFullPathSafe(ServerConfig.AppDataLocation.Value);
					if (!string.IsNullOrEmpty(appDataPath))
					{
						scpslArgs.Add($"-appdatapath \"{appDataPath}\"");
					}

					scpslArgs.RemoveAll(string.IsNullOrEmpty);

					string argsString = string.Join(" ", scpslArgs);

					Write($"Starting server with the following parameters:\n{scpslExe} {argsString}");

					ProcessStartInfo startInfo = new ProcessStartInfo(scpslExe, argsString)
					{
						CreateNoWindow = true,
						UseShellExecute = false
					};

					ForEachHandler<IEventServerPreStart>(eventPreStart => eventPreStart.OnServerPreStart());

					// Start the input reader
					Thread inputHandlerThread = new Thread(() => InputHandler.Write(this));

					if (!Program.Headless)
						inputHandlerThread.Start();

					// Start the output reader
					OutputHandler outputHandler = new OutputHandler(this);

					// Finally, start the game
					GameProcess = Process.Start(startInfo);

					Status = ServerStatus.Running;

					MainLoop();

					switch (Status)
					{
						case ServerStatus.Stopping:
						case ServerStatus.ForceStopping:
							Status = ServerStatus.Stopped;

							shouldRestart = false;
							break;

						case ServerStatus.Restarting:
							shouldRestart = true;
							break;

						default:
							Status = ServerStatus.StoppedUnexpectedly;

							ForEachHandler<IEventCrash>(eventCrash => eventCrash.OnCrash());

							Write("Game engine exited unexpectedly", ConsoleColor.Red);

							shouldRestart = restartOnCrash;
							break;
					}

					// Cleanup after exit from MainLoop
					GameProcess.Dispose();
					GameProcess = null;

					// Stop the input handler if it's running
					if (inputHandlerThread.IsAlive)
					{
						inputHandlerThread.Abort();
					}

					outputHandler.Dispose();

					DeleteSession();

					SessionId = null;
					StartDateTime = null;

					if (shouldRestart) Write("Restarting server...");
				}
				catch (Exception e)
				{
					Write(e.Message, ConsoleColor.Red);
					Program.LogDebugException(nameof(StartServer), e);

					// If the server should try to start up again
					if (ServerConfig.ServerStartRetry.Value)
					{
						shouldRestart = true;

						int waitDelayMs = ServerConfig.ServerStartRetryDelay.Value;
						if (waitDelayMs > 0)
						{
							Write($"Startup failed! Waiting for {waitDelayMs} ms before retrying...", ConsoleColor.Red);
							Thread.Sleep(waitDelayMs);
						}
						else
						{
							Write("Startup failed! Retrying...", ConsoleColor.Red);
						}
					}
					else
					{
						Write("Startup failed! Exiting...", ConsoleColor.Red);
					}
				}
				finally
				{
					DeleteSession();
				}
			} while (shouldRestart);
		}

		public void StopServer(bool killGame = false)
		{
			if (!IsRunning) throw new Exceptions.ServerNotRunningException();

			// If the game is loading, it won't accept the safe quit command
			if (IsLoading)
				killGame = true;

			initStopTimeoutTime = DateTime.Now;
			Status = killGame ? ServerStatus.ForceStopping : ServerStatus.Stopping;

			ForEachHandler<IEventServerStop>(stopEvent => stopEvent.OnServerStop());

			if (killGame && IsGameProcessRunning)
				GameProcess.Kill();
			else SendMessage("QUIT");
		}

		public void SoftRestartServer()
		{
			if (!IsRunning) throw new Exceptions.ServerNotRunningException();

			initRestartTimeoutTime = DateTime.Now;
			Status = ServerStatus.Restarting;

			if (hasServerMod)
			{
				SendMessage("RECONNECTRS");
			}
			else
			{
				SendMessage("ROUNDRESTART");
				SendMessage("QUIT");
			}
		}

		#endregion

		#region Feature Registration, Initialization, and Execution

		private void RegisterFeature(Feature feature)
		{
			switch (feature)
			{
				case IEventTick eventTick:
					tick.Add(eventTick);
					break;

				case ICommand command:
				{
					string commandKey = command.GetCommand().ToLower().Trim();

					// If the command was already registered
					if (commands.ContainsKey(commandKey))
					{
						string message = $"Warning, {nameof(MultiAdmin)} tried to register duplicate command \"{commandKey}\"";

						Program.LogDebug(nameof(RegisterFeature), message);
						Write(message);
					}
					else
					{
						commands.Add(commandKey, command);
					}

					break;
				}
			}

			features.Add(feature);
		}

		private static IEnumerable<Type> GetTypesWithAttribute(Type attribute)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					object[] attributes = type.GetCustomAttributes(attribute, true);
					if (!attributes.IsEmpty()) yield return type;
				}
			}
		}

		private void RegisterFeatures()
		{
			Type[] assembly = GetTypesWithAttribute(typeof(FeatureAttribute)).ToArray();
			foreach (Type type in assembly)
			{
				try
				{
					object featureInstance = Activator.CreateInstance(type, this);
					if (featureInstance is Feature feature) RegisterFeature(feature);
				}
				catch (Exception e)
				{
					Program.LogDebugException(nameof(RegisterFeatures), e);
				}
			}
		}

		private void InitFeatures()
		{
			foreach (Feature feature in features)
			{
				feature.Init();
				feature.OnConfigReload();
			}
		}

		public void ForEachHandler<T>(Action<T> action) where T : IMAEvent
		{
			foreach (Feature feature in features)
				if (feature is T eventHandler)
					action.Invoke(eventHandler);
		}

		#endregion

		#region Session Directory Management

		public void PrepareSession()
		{
			try
			{
				Directory.CreateDirectory(SessionDirectory);
				Write($"Started new session \"{SessionId}\"", ConsoleColor.DarkGreen);
			}
			catch (Exception e)
			{
				throw new UnauthorizedAccessException($"Unable to create directory \"{SessionDirectory}\", make sure that {nameof(MultiAdmin)} has access to \"{DedicatedDir}\"\n{e}");
			}
		}

		public void CleanSession()
		{
			if (!Directory.Exists(SessionDirectory)) return;

			foreach (string file in Directory.GetFiles(SessionDirectory))
			{
				for (int i = 0; i < 20; i++)
				{
					try
					{
						File.Delete(file);
						break;
					}
					catch (UnauthorizedAccessException e)
					{
						Program.LogDebugException(nameof(CleanSession), e);
						Thread.Sleep(8);
					}
					catch (Exception e)
					{
						Program.LogDebugException(nameof(CleanSession), e);
						Thread.Sleep(5);
					}
				}
			}
		}

		public void DeleteSession()
		{
			try
			{
				CleanSession();

				if (!Directory.Exists(SessionDirectory)) return;

				for (int i = 0; i < 20; i++)
				{
					try
					{
						Directory.Delete(SessionDirectory);
						break;
					}
					catch (UnauthorizedAccessException e)
					{
						Program.LogDebugException(nameof(DeleteSession), e);
						Thread.Sleep(8);
					}
					catch (Exception e)
					{
						Program.LogDebugException(nameof(DeleteSession), e);
						Thread.Sleep(5);
					}
				}
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(DeleteSession), e);
			}
		}

		#endregion

		#region Console Output and Logging

		public void Write(ColoredMessage[] messages, ConsoleColor? timeStampColor = null)
		{
			lock (ColoredConsole.WriteLock)
			{
				if (messages == null) return;

				Log(messages.GetText());

				if (Program.Headless) return;

				ColoredMessage[] timeStampedMessage = Utils.TimeStampMessage(messages, timeStampColor);

				timeStampedMessage.WriteLine(ServerConfig.UseNewInputSystem.Value);

				if (ServerConfig.UseNewInputSystem.Value)
					InputHandler.WriteInputAndSetCursor();
			}
		}

		public void Write(ColoredMessage message, ConsoleColor? timeStampColor = null)
		{
			lock (ColoredConsole.WriteLock)
			{
				Write(new ColoredMessage[] {message}, timeStampColor ?? message.textColor);
			}
		}

		public void Write(string message, ConsoleColor? color = ConsoleColor.Yellow, ConsoleColor? timeStampColor = null)
		{
			lock (ColoredConsole.WriteLock)
			{
				Write(new ColoredMessage(message, color), timeStampColor);
			}
		}

		public void Log(string message)
		{
			lock (ColoredConsole.WriteLock)
			{
				if (message == null || string.IsNullOrEmpty(MaLogFile) || ServerConfig.NoLog.Value) return;

				try
				{
					Directory.CreateDirectory(logDir);

					using (StreamWriter sw = File.AppendText(MaLogFile))
					{
						message = Utils.TimeStampMessage(message);
						sw.Write(message);
						if (!message.EndsWith(Environment.NewLine)) sw.WriteLine();
					}
				}
				catch (Exception e)
				{
					Program.LogDebugException(nameof(Log), e);

					new ColoredMessage[] {new ColoredMessage("Error while logging for MultiAdmin:", ConsoleColor.Red), new ColoredMessage(e.ToString(), ConsoleColor.Red)}.WriteLines();
				}
			}
		}

		#endregion

		public bool ServerModCheck(int major, int minor, int fix)
		{
			if (string.IsNullOrEmpty(serverModVersion))
				return false;

			string[] parts = serverModVersion.Split('.');

			if (parts.IsEmpty())
				return false;

			int.TryParse(parts[0], out int verMajor);

			int verMinor = 0;
			if (parts.Length >= 2)
				int.TryParse(parts[1], out verMinor);

			int verFix = 0;
			if (parts.Length >= 3)
				int.TryParse(parts[2], out verFix);

			return verMajor > major || verMajor >= major && verMinor > minor ||
			       verMajor >= major && verMinor >= minor && verFix >= fix;
		}

		public void ReloadConfig(bool copyFiles = true, bool runEvent = true)
		{
			ServerConfig.ReloadConfig();

			// Handle directory copying
			string copyFromDir;
			if (copyFiles && !string.IsNullOrEmpty(configLocation) && !string.IsNullOrEmpty(copyFromDir = ServerConfig.CopyFromFolderOnReload.Value))
			{
				CopyFromDir(copyFromDir, ServerConfig.FolderCopyWhitelist.Value, ServerConfig.FolderCopyBlacklist.Value);
			}

			// Handle each config reload event
			if (runEvent)
				foreach (Feature feature in features)
					feature.OnConfigReload();
		}

		public bool CopyFromDir(string sourceDir, string[] fileWhitelist = null, string[] fileBlacklist = null)
		{
			if (string.IsNullOrEmpty(configLocation) || string.IsNullOrEmpty(sourceDir)) return false;

			try
			{
				sourceDir = Utils.GetFullPathSafe(sourceDir);

				if (!string.IsNullOrEmpty(sourceDir))
				{
					Write($"Copying files and folders from \"{sourceDir}\" into \"{configLocation}\"...");
					Utils.CopyAll(sourceDir, configLocation, fileWhitelist, fileBlacklist);
					Write("Done copying files and folders!");

					return true;
				}
			}
			catch (Exception e)
			{
				Write($"Error while copying files and folders:\n{e}", ConsoleColor.Red);
			}

			return false;
		}
	}

	public enum ServerStatus
	{
		NotStarted,
		Starting,
		Running,
		Stopping,
		ForceStopping,
		Restarting,
		Stopped,
		StoppedUnexpectedly
	}
}
