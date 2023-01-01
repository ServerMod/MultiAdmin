using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MultiAdmin.Config;
using MultiAdmin.ConsoleTools;
using MultiAdmin.Features;
using MultiAdmin.ServerIO;
using MultiAdmin.Utility;

namespace MultiAdmin
{
	public class Server
	{
		public readonly Dictionary<string, ICommand> commands = new();

		public readonly List<Feature> features = new();

		// We want a tick only list since its the only event that happens constantly, all the rest can be in a single list
		private readonly List<IEventTick> tick = new();

		private readonly MultiAdminConfig serverConfig;
		public MultiAdminConfig ServerConfig => serverConfig ?? MultiAdminConfig.GlobalConfig;

		public readonly string? serverId;
		public readonly string? configLocation;
		private readonly uint? port;
		public readonly string?[]? args;
		public readonly string? serverDir;
		public readonly string logDir;

		public uint Port => port ?? ServerConfig.Port.Value;

		private DateTime initStopTimeoutTime;
		private DateTime initRestartTimeoutTime;

		public ModFeatures supportedModFeatures = ModFeatures.None;

		public Server(string? serverId = null, string? configLocation = null, uint? port = null, string?[]? args = null)
		{
			this.serverId = serverId;
			serverDir = string.IsNullOrEmpty(serverId)
				? null
				: Utils.GetFullPathSafe(Path.Combine(MultiAdminConfig.GlobalConfig.ServersFolder.Value, serverId));

			this.configLocation = Utils.GetFullPathSafe(configLocation) ??
								  Utils.GetFullPathSafe(MultiAdminConfig.GlobalConfig.ConfigLocation.Value) ??
								  Utils.GetFullPathSafe(serverDir);

			// Load config
			serverConfig = MultiAdminConfig.GlobalConfig;

			// Load config hierarchy
			string? serverConfigLocation = this.configLocation;
			while (!string.IsNullOrEmpty(serverConfigLocation))
			{
				// Update the Server object's config location with the valid config location
				this.configLocation = serverConfigLocation;

				// Load the child MultiAdminConfig
				serverConfig = new MultiAdminConfig(Path.Combine(serverConfigLocation, MultiAdminConfig.ConfigFileName),
					serverConfig);

				// Set the server config location to the value from the config, this should be empty or null if there is no valid value
				serverConfigLocation = Utils.GetFullPathSafe(serverConfig.ConfigLocation.Value);

				// If the config hierarchy already contains the MultiAdmin config from the target path, stop looping
				// Without this, a user could unintentionally cause a lockup when their server starts up due to infinite looping
				if (serverConfig.ConfigHierarchyContainsPath(serverConfigLocation))
					break;
			}

			// Set port
			this.port = port;

			// Set args
			this.args = args;

			logDir = Utils.GetFullPathSafe(Path.Combine(string.IsNullOrEmpty(serverDir) ? "" : serverDir,
				serverConfig.LogLocation.Value)) ?? throw new FileNotFoundException($"Log file \"{nameof(logDir)}\" was not set");

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

		public bool IsStopped => Status == ServerStatus.NotStarted || Status == ServerStatus.Stopped ||
								 Status == ServerStatus.StoppedUnexpectedly;

		public bool IsRunning => !IsStopped;
		public bool IsStarted => !IsStopped && !IsStarting;

		public bool IsStarting => Status == ServerStatus.Starting;

		public bool IsStopping => Status == ServerStatus.Stopping || Status == ServerStatus.ForceStopping ||
								  Status == ServerStatus.Restarting;

		public bool IsLoading { get; set; }

		public bool SetServerRequestedStatus(ServerStatus status)
		{
			// Don't override the console's own requests
			if (IsStopping)
			{
				return false;
			}

			Status = status;

			return true;
		}

		#endregion

		private string? startDateTime;

		public string? StartDateTime
		{
			get => startDateTime;

			private set
			{
				startDateTime = value;

				// Update related variables
				LogDirFile = string.IsNullOrEmpty(value) || string.IsNullOrEmpty(logDir)
					? null
					: $"{Path.Combine(logDir.EscapeFormat(), value)}_{{0}}_log_{Port}.txt";

				lock (this)
				{
					MaLogFile = string.IsNullOrEmpty(LogDirFile) ? null : string.Format(LogDirFile, "MA");
					ScpLogFile = string.IsNullOrEmpty(LogDirFile) ? null : string.Format(LogDirFile, "SCP");
				}
			}
		}

		public bool CheckStopTimeout =>
			(DateTime.Now - initStopTimeoutTime).Seconds > ServerConfig.ServerStopTimeout.Value;

		public bool CheckRestartTimeout =>
			(DateTime.Now - initRestartTimeoutTime).Seconds > ServerConfig.ServerRestartTimeout.Value;

		public string? LogDirFile { get; private set; }
		public string? MaLogFile { get; private set; }
		public string? ScpLogFile { get; private set; }

		private StreamWriter? maLogStream;

		public Process? GameProcess { get; private set; }

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


		public static readonly string? DedicatedDir = Utils.GetFullPathSafe(Path.Combine("SCPSL_Data", "Dedicated"));

		public ServerSocket? SessionSocket { get; private set; }

		#region Server Core

		private void MainLoop()
		{
			// Creates and starts a timer
			Stopwatch timer = new();
			timer.Restart();

			while (IsGameProcessRunning)
			{
				foreach (IEventTick tickEvent in tick) tickEvent.OnTick();

				timer.Stop();

				// Wait the delay per tick (calculating how long the tick took and compensating)
				Thread.Sleep(Math.Max(ServerConfig.MultiAdminTickDelay.Value - timer.Elapsed.Milliseconds, 0));

				timer.Restart();

				if (Status == ServerStatus.Restarting && CheckRestartTimeout)
				{
					Write("Server restart timed out, killing the server process...", ConsoleColor.Red);
					RestartServer(true);
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
		public bool SendMessage(string message)
		{
			if (SessionSocket == null || !SessionSocket.Connected)
			{
				Write("Unable to send command to server, the console is disconnected", ConsoleColor.Red);
				return false;
			}

			SessionSocket.SendMessage(message);
			return true;
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
					if (!string.IsNullOrEmpty(config?.Config?.ConfigPath) &&
						MultiAdminConfig.GlobalConfigFilePath != config.Config.ConfigPath)
						Write($"Using server config \"{config.Config.ConfigPath}\"...");
				}
			}
		}

		public static string GetExecutablePath()
		{
			string scpslExe;

			if (OperatingSystem.IsLinux())
				scpslExe = "SCPSL.x86_64";
			else if (OperatingSystem.IsWindows())
				scpslExe = "SCPSL.exe";
			else
				throw new FileNotFoundException("Invalid OS, can't run executable");

			if (!File.Exists(scpslExe))
				throw new FileNotFoundException(
					$"Can't find game executable \"{scpslExe}\", the working directory must be the game directory");

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

				StartDateTime = Utils.DateTime;

				try
				{
					// Set up logging
					maLogStream?.Close();
					Directory.CreateDirectory(logDir);
					maLogStream = File.AppendText(MaLogFile ?? throw new FileNotFoundException($"Log file \"{nameof(MaLogFile)}\" was not set"));

					#region Startup Info Printing & Logging

					WriteConfigInformation();

					#endregion

					// Reload the config immediately as server is starting
					ReloadConfig();

					// Init features
					InitFeatures();

					string scpslExe = GetExecutablePath();

					Write($"Executing \"{scpslExe}\"...", ConsoleColor.DarkGreen);

					// Start the console socket connection to the game server
					ServerSocket consoleSocket = new();
					// Start the connection before the game to find an open port for communication
					consoleSocket.Connect();

					SessionSocket = consoleSocket;

					List<string?> scpslArgs = new()
					{
						$"-multiadmin:{Program.MaVersion}:{(int)ModFeatures.All}",
						"-batchmode",
						"-nographics",
						"-silent-crashes",
						"-nodedicateddelete",
						$"-id{Environment.ProcessId}",
						$"-console{consoleSocket.Port}",
						$"-port{Port}"
					};

					if (string.IsNullOrEmpty(ScpLogFile) || ServerConfig.NoLog.Value)
					{
						scpslArgs.Add("-nolog");

						if (OperatingSystem.IsLinux())
						{
							scpslArgs.Add("-logFile");
							scpslArgs.Add("/dev/null");
						}
						else if (OperatingSystem.IsWindows())
						{
							scpslArgs.Add("-logFile");
							scpslArgs.Add("NUL");
						}
					}
					else
					{
						scpslArgs.Add("-logFile");
						scpslArgs.Add(ScpLogFile);
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
						scpslArgs.Add("-configpath");
						scpslArgs.Add(configLocation);
					}

					string? appDataPath = Utils.GetFullPathSafe(ServerConfig.AppDataLocation.Value);
					if (!string.IsNullOrEmpty(appDataPath))
					{
						scpslArgs.Add("-appdatapath");
						scpslArgs.Add(appDataPath);
					}

					// Add custom arguments
					if (args != null) scpslArgs.AddRange(args);

					ProcessStartInfo startInfo = new(scpslExe, scpslArgs.JoinArgs())
					{
						CreateNoWindow = true,
						UseShellExecute = false
					};

					Write($"Starting server with the following parameters:\n{scpslExe} {startInfo.Arguments}");

					if (ServerConfig.ActualConsoleInputSystem == InputHandler.ConsoleInputSystem.Original)
						Write("You are using the original input system. It may prevent MultiAdmin from closing and/or cause ghost game processes", ConsoleColor.Red);

					// Reset the supported mod features
					supportedModFeatures = ModFeatures.None;

					ForEachHandler<IEventServerPreStart>(eventPreStart => eventPreStart.OnServerPreStart());

					// Start the input reader
					CancellationTokenSource inputHandlerCancellation = new();
					Task? inputHandler = null;

					if (!Program.Headless)
					{
						inputHandler = Task.Run(() => InputHandler.Write(this, inputHandlerCancellation.Token), inputHandlerCancellation.Token);
					}

					// Start the output reader
					OutputHandler outputHandler = new(this);
					// Assign the socket events to the OutputHandler
					consoleSocket.OnReceiveMessage += outputHandler.HandleMessage;
					consoleSocket.OnReceiveAction += outputHandler.HandleAction;

					// Finally, start the game
					GameProcess = Process.Start(startInfo);

					Status = ServerStatus.Running;

					MainLoop();

					try
					{
						switch (Status)
						{
							case ServerStatus.Stopping:
							case ServerStatus.ForceStopping:
							case ServerStatus.ExitActionStop:
								Status = ServerStatus.Stopped;

								shouldRestart = false;
								break;

							case ServerStatus.Restarting:
							case ServerStatus.ExitActionRestart:
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
						GameProcess?.Dispose();
						GameProcess = null;

						// Stop the input handler if it's running
						if (inputHandler != null)
						{
							inputHandlerCancellation.Cancel();
							try
							{
								inputHandler.Wait();
							}
							catch (Exception)
							{
								// Task was cancelled or disposed, this is fine since we're waiting for that
							}
							inputHandler.Dispose();
							inputHandlerCancellation.Dispose();
						}

						consoleSocket.Disconnect();

						// Remove the socket events for OutputHandler
						consoleSocket.OnReceiveMessage -= outputHandler.HandleMessage;
						consoleSocket.OnReceiveAction -= outputHandler.HandleAction;

						SessionSocket = null;
						StartDateTime = null;
					}
					catch (Exception e)
					{
						Write(e.Message, ConsoleColor.Red);
						Program.LogDebugException(nameof(StartServer), e);
						Write("Shutdown failed...", ConsoleColor.Red);
					}

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
			} while (shouldRestart);

			// Finish server instance
			maLogStream?.Close();
			maLogStream = null;
		}

		public void SetStopStatus(bool killGame = false)
		{
			if (!IsRunning) throw new Exceptions.ServerNotRunningException();

			initStopTimeoutTime = DateTime.Now;
			Status = killGame ? ServerStatus.ForceStopping : ServerStatus.Stopping;

			ForEachHandler<IEventServerStop>(stopEvent => stopEvent.OnServerStop());
		}

		public void StopServer(bool killGame = false)
		{
			if (!IsRunning) throw new Exceptions.ServerNotRunningException();

			SetStopStatus(killGame);

			if ((killGame || !SendMessage("QUIT")) && IsGameProcessRunning)
				GameProcess?.Kill();
		}

		public void SetRestartStatus()
		{
			if (!IsRunning) throw new Exceptions.ServerNotRunningException();

			initRestartTimeoutTime = DateTime.Now;
			Status = ServerStatus.Restarting;
		}

		public void RestartServer(bool killGame = false)
		{
			if (!IsRunning) throw new Exceptions.ServerNotRunningException();

			SetRestartStatus();

			if ((killGame || !SendMessage("SOFTRESTART")) && IsGameProcessRunning)
				GameProcess?.Kill();
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
							string message =
								$"Warning, {nameof(MultiAdmin)} tried to register duplicate command \"{commandKey}\"";

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

		private void RegisterFeatures()
		{
			RegisterFeature(new ConfigGenerator(this));
			RegisterFeature(new ConfigReload(this));
			RegisterFeature(new ExitCommand(this));
			RegisterFeature(new FileCopyRoundQueue(this));
			RegisterFeature(new GithubGenerator(this));
			RegisterFeature(new HelpCommand(this));
			RegisterFeature(new MemoryChecker(this));
			RegisterFeature(new MultiAdminInfo(this));
			RegisterFeature(new NewCommand(this));
			RegisterFeature(new Restart(this));
			RegisterFeature(new RestartRoundCounter(this));
			RegisterFeature(new Titlebar(this));
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

		#region Console Output and Logging

		public void Write(ColoredMessage?[] messages, ConsoleColor? timeStampColor = null)
		{
			lock (ColoredConsole.WriteLock)
			{
				if (messages == null) return;

				Log(messages.GetText());

				if (Program.Headless) return;

				ColoredMessage?[] timeStampedMessage = Utils.TimeStampMessage(messages, timeStampColor);

				timeStampedMessage.WriteLine(ServerConfig.ActualConsoleInputSystem == InputHandler.ConsoleInputSystem.New);

				if (ServerConfig.ActualConsoleInputSystem == InputHandler.ConsoleInputSystem.New)
					InputHandler.WriteInputAndSetCursor(true);
			}
		}

		public void Write(ColoredMessage message, ConsoleColor? timeStampColor = null)
		{
			lock (ColoredConsole.WriteLock)
			{
				Write(new ColoredMessage[] { message }, timeStampColor ?? message.textColor);
			}
		}

		public void Write(string message, ConsoleColor? color = ConsoleColor.Yellow,
			ConsoleColor? timeStampColor = null)
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
				if (maLogStream == null || string.IsNullOrEmpty(MaLogFile) || ServerConfig.NoLog.Value) return;

				try
				{
					message = Utils.TimeStampMessage(message);
					maLogStream.Write(message);
					if (!message.EndsWith(Environment.NewLine)) maLogStream.WriteLine();
					maLogStream.Flush();
				}
				catch (Exception e)
				{
					Program.LogDebugException(nameof(Log), e);

					new ColoredMessage[]
					{
						new ColoredMessage("Error while logging for MultiAdmin:", ConsoleColor.Red),
						new ColoredMessage(e.ToString(), ConsoleColor.Red)
					}.WriteLines();
				}
			}
		}

		#endregion

		public void ReloadConfig(bool copyFiles = true, bool runEvent = true)
		{
			ServerConfig.ReloadConfig();

			// Handle directory copying
			string copyFromDir;
			if (copyFiles && !string.IsNullOrEmpty(configLocation) &&
				!string.IsNullOrEmpty(copyFromDir = ServerConfig.CopyFromFolderOnReload.Value))
			{
				CopyFromDir(copyFromDir, ServerConfig.FolderCopyWhitelist.Value,
					ServerConfig.FolderCopyBlacklist.Value);
			}

			// Handle each config reload event
			if (runEvent)
				foreach (Feature feature in features)
					feature.OnConfigReload();
		}

		public bool CopyFromDir(string? sourceDir, string[]? fileWhitelist = null, string[]? fileBlacklist = null)
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
		ExitActionStop,
		ForceStopping,
		Restarting,
		ExitActionRestart,
		Stopped,
		StoppedUnexpectedly
	}
}
