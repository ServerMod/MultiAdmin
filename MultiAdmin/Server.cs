using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin
{
	public class Server
	{
		public const string MaVersion = "3.0.0";

		public readonly Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();
		public readonly List<Feature> features = new List<Feature>();
		// we want a tick only list since its the only event that happens constantly, all the rest can be in a single list
		private readonly List<IEventTick> tick = new List<IEventTick>();

		private readonly MultiAdminConfig serverConfig;
		public MultiAdminConfig ServerConfig => serverConfig ?? new MultiAdminConfig();

		public readonly string serverId;
		public readonly string configLocation;
		public readonly string serverDir;
		public readonly string logDir;

		public bool hasServerMod;

		public string serverModBuild;
		public string serverModVersion;

		private int logId;

		public Server(string serverId = null, string configLocation = null)
		{
			this.serverId = serverId;
			serverDir = string.IsNullOrEmpty(this.serverId) ? null : MultiAdminConfig.GlobalServersFolder + Path.DirectorySeparatorChar + this.serverId;

			this.configLocation = configLocation ?? MultiAdminConfig.GlobalConfigLocation ?? serverDir;
			logDir = (string.IsNullOrEmpty(serverDir) ? string.Empty : serverDir + Path.DirectorySeparatorChar) + "logs";

			// Load config
			serverConfig = string.IsNullOrEmpty(this.configLocation) ? new MultiAdminConfig() : new MultiAdminConfig(this.configLocation + Path.DirectorySeparatorChar + MultiAdminConfig.ConfigFileName);

			// Register all features
			RegisterFeatures();

			// Init features
			InitFeatures();
		}

		public bool InitialRoundStarted { get; set; }
		public bool Running { get; private set; }
		public bool Stopping { get; private set; }
		public bool Crashed { get; private set; }

		private string startDateTime;
		public string StartDateTime
		{
			get => startDateTime;

			private set
			{
				startDateTime = value;

				// Update related variables
				LogDirFile = string.IsNullOrEmpty(value) ? null : logDir + Path.DirectorySeparatorChar + value + "_{0}_output_log.txt";

				lock (this)
				{
					MaLogFile = string.IsNullOrEmpty(LogDirFile) ? null : string.Format(LogDirFile, "MA");
					ScpLogFile = string.IsNullOrEmpty(LogDirFile) ? null : string.Format(LogDirFile, "SCP");
					ModLogFile = string.IsNullOrEmpty(LogDirFile) ? null : string.Format(LogDirFile, "MODERATOR");
				}
			}
		}

		public string LogDirFile { get; private set; }
		public string MaLogFile { get; private set; }
		public string ScpLogFile { get; private set; }
		public string ModLogFile { get; private set; }

		public Process GameProcess { get; private set; }

		private string sessionId;
		public string SessionId
		{
			get => sessionId;

			private set
			{
				sessionId = value;

				// Update related variables
				SessionDirectory = string.IsNullOrEmpty(value) ? null : OutputHandler.DedicatedDir + Path.DirectorySeparatorChar + value;
			}
		}

		public string SessionDirectory { get; private set; }

		#region Server Core

		private void MainLoop()
		{
			if (!Running) throw new Exceptions.ServerNotRunningException();

			while (!Stopping)
				if (GameProcess != null && !GameProcess.HasExited)
				{
					foreach (IEventTick tickEvent in tick) tickEvent.OnTick();

					Thread.Sleep(1000);
				}
				else if (!Stopping)
				{
					Crashed = true;
					Stopping = true;

					foreach (Feature f in features)
						if (f is IEventCrash eventCrash)
							eventCrash.OnCrash();
				}
		}

		public void SendMessage(string message)
		{
			if (!Directory.Exists(SessionDirectory))
			{
				Write($"Send Message error: Sending {message} failed. \"{SessionDirectory}\" does not exist!");
				Write("Skipping...");
				return;
			}

			string file = SessionDirectory + Path.DirectorySeparatorChar + "cs" + logId + ".mapi";
			if (File.Exists(file))
			{
				Write($"Send Message error: Sending {message} failed. \"{file}\" already exists!");
				Write("Skipping...");
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

		public void StartServer()
		{
			if (Running) throw new Exceptions.ServerAlreadyRunningException();

			Running = true;
			Stopping = false;
			Crashed = false;
			InitialRoundStarted = false;

			SessionId = DateTime.UtcNow.Ticks.ToString();
			StartDateTime = Utils.DateTime;

			try
			{
				PrepareSession();

				string scpslExe;

				if (Utils.IsUnix)
					scpslExe = "SCPSL.x86_64";
				else if (Utils.IsWindows)
					scpslExe = "SCPSL.exe";
				else
					throw new FileNotFoundException("Invalid OS, can't run executable");

				if (!File.Exists(scpslExe))
					throw new FileNotFoundException($"Can't find game executable \"{scpslExe}\"");

				Write($"Executing \"{scpslExe}\"...", ConsoleColor.DarkGreen);

				List<string> scpslArgs = new List<string>(new[]
				{
					"-batchmode",
					"-nographics",
					"-silent-crashes",
					"-nodedicateddelete",
					$"-key{SessionId}",
					$"-id{Process.GetCurrentProcess().Id}",
					$"-{(string.IsNullOrEmpty(ScpLogFile) || ServerConfig.NoLog ? "nolog" : $"logFile \"{ScpLogFile}\"")}"
				});

				if (ServerConfig.DisableConfigValidation)
					scpslArgs.Add("-disableconfigvalidation");

				if (ServerConfig.ShareNonConfigs)
					scpslArgs.Add("-sharenonconfigs");

				if (!string.IsNullOrEmpty(configLocation))
					scpslArgs.Add($"-configpath \"{configLocation}\"");

				string argsString = string.Join(" ", scpslArgs);

				Write("Starting server with the following parameters");
				Write(scpslExe + " " + argsString);

				ProcessStartInfo startInfo = new ProcessStartInfo(scpslExe) { Arguments = argsString };

				foreach (Feature f in features)
					if (f is IEventServerPreStart eventPreStart)
						eventPreStart.OnServerPreStart();

				// Start the input reader
				Thread inputReaderThread = new Thread(() => InputThread.Write(this));
				inputReaderThread.Start();

				// Start the output reader
				OutputHandler outputHandler = new OutputHandler(this);

				// Finally, start the game
				GameProcess = Process.Start(startInfo);

				MainLoop();

				// Cleanup after exit from MainLoop
				inputReaderThread.Abort();
				outputHandler.Dispose();

				DeleteSession();

				Running = false;
				Stopping = false;

				SessionId = null;
				StartDateTime = null;
			}
			catch (Exception e)
			{
				Write("Failed - Executable file not found or config issue!", ConsoleColor.Red);
				Write(e.Message, ConsoleColor.Red);
				Write("Press any key to close...", ConsoleColor.DarkGray);
				Console.ReadKey(true);
				Process.GetCurrentProcess().Kill();
			}
		}

		public void StopServer(bool killGame = true)
		{
			if (Stopping || !Running) throw new Exceptions.ServerNotRunningException();

			foreach (Feature f in features)
				if (f is IEventServerStop stopEvent)
					stopEvent.OnServerStop();

			if (killGame) GameProcess.Kill();
			Stopping = true;
		}

		public void SoftRestartServer()
		{
			if (!Running) throw new Exceptions.ServerNotRunningException();

			SendMessage("RECONNECTRS");
		}

		#endregion

		#region Feature Registration and Initialization

		private void RegisterFeature(Feature feature)
		{
			switch (feature)
			{
				case IEventTick eventTick:
					tick.Add(eventTick);
					break;

				case ICommand command:
					commands.Add(command.GetCommand().ToLower().Trim(), command);
					break;
			}

			features.Add(feature);
		}

		private static IEnumerable<Type> GetTypesWithAttribute(Type attribute)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			foreach (Type type in assembly.GetTypes())
			{
				object[] attribs = type.GetCustomAttributes(attribute, false);
				if (attribs.Length > 0) yield return type;
			}
		}

		private void RegisterFeatures()
		{
			Type[] assembly = GetTypesWithAttribute(typeof(FeatureAttribute)).ToArray();
			foreach (Type type in assembly)
				try
				{
					object featureInstance = Activator.CreateInstance(type, this);
					if (featureInstance is Feature feature) RegisterFeature(feature);
				}
				catch
				{
					// ignored
				}

			//RegisterFeature(new ConfigReload(this));
			//RegisterFeature(new ExitCommand(this));
			////RegisterFeature(new EventTest(this));
			//RegisterFeature(new GithubGenerator(this));
			//RegisterFeature(new HelpCommand(this));
			//RegisterFeature(new InactivityShutdown(this));
			//RegisterFeature(new MemoryChecker(this));
			//RegisterFeature(new MemoryCheckerSoft(this));
			//RegisterFeature(new ModLog(this));
			//RegisterFeature(new MultiAdminInfo(this));
			//RegisterFeature(new NewCommand(this));
			//RegisterFeature(new Restart(this));
			//RegisterFeature(new RestartNextRound(this));
			//RegisterFeature(new RestartRoundCounter(this));
			//RegisterFeature(new StopNextRound(this));
			//RegisterFeature(new Titlebar(this));
		}

		private void InitFeatures()
		{
			foreach (Feature feature in features)
			{
				feature.Init();
				feature.OnConfigReload();
			}
		}

		#endregion

		#region Session Directory Management

		private void PrepareSession()
		{
			try
			{
				Directory.CreateDirectory(SessionDirectory);
				Write("Started new session.", ConsoleColor.DarkGreen);
			}
			catch
			{
				Write($"Failed - Please close all open files in \"{OutputHandler.DedicatedDir}\" and restart the server!",
					ConsoleColor.Red);
				Write("Press any key to close...", ConsoleColor.DarkGray);
				Console.ReadKey(true);
				Process.GetCurrentProcess().Kill();
			}
		}

		private void CleanSession()
		{
			if (!Directory.Exists(SessionDirectory)) return;

			foreach (string file in Directory.GetFiles(SessionDirectory))
				File.Delete(file);
		}

		private void DeleteSession()
		{
			CleanSession();
			if (Directory.Exists(SessionDirectory)) Directory.Delete(SessionDirectory);
		}

		#endregion

		#region Console Output and Logging

		public void Write(string message, ConsoleColor color = ConsoleColor.Yellow, int height = 0)
		{
			Log(message);

			if (Utils.IsProcessHandleZero) return;

			int cursorTop = 0, bufferHeight = 0;
			try
			{
				cursorTop = Console.CursorTop + height;
				bufferHeight = Console.BufferHeight;
				if (cursorTop < 0)
					cursorTop = 0;
				else if (cursorTop >= Console.BufferHeight) cursorTop = Console.BufferHeight - 1;
				Console.CursorTop = cursorTop;
				Console.ForegroundColor = color;
				message = Utils.TimeStamp(message);
				Console.WriteLine(message);
				Console.ForegroundColor = ConsoleColor.White;
				Console.BackgroundColor = ConsoleColor.Black;
			}
			catch (ArgumentOutOfRangeException e)
			{
				Console.WriteLine(Utils.TimeStamp("Value " + cursorTop + " exceeded buffer height " + bufferHeight +
											"."));
				Console.WriteLine(e.StackTrace);
			}
		}

		public static void WritePart(string part, ConsoleColor backgroundColor = ConsoleColor.Black,
			ConsoleColor textColor = ConsoleColor.Yellow, bool date = false, bool lineEnd = false)
		{
			Console.ForegroundColor = textColor;
			Console.BackgroundColor = backgroundColor;

			if (date)
			{
				DateTime now = DateTime.Now;
				string datePart = "[" + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + "] ";
				Console.Write(datePart);
			}

			Console.Write(part);

			if (lineEnd && !part.EndsWith(Environment.NewLine)) Console.WriteLine();
		}

		public void Log(string message)
		{
			if (string.IsNullOrEmpty(MaLogFile)) return;

			lock (this)
			{
				Directory.CreateDirectory(logDir);

				using (StreamWriter sw = File.AppendText(MaLogFile))
				{
					message = Utils.TimeStamp(message);
					sw.Write(message);
					if (!message.EndsWith(Environment.NewLine)) sw.WriteLine();
				}
			}
		}

		#endregion

		public bool ServerModCheck(int major, int minor, int fix)
		{
			if (serverModVersion == null) return false;

			string[] parts = serverModVersion.Split('.');
			int verMajor;
			int verMinor;
			int verFix = 0;
			switch (parts.Length)
			{
				case 3:
					int.TryParse(parts[0], out verMajor);
					int.TryParse(parts[1], out verMinor);
					int.TryParse(parts[2], out verFix);
					break;
				case 2:
					int.TryParse(parts[0], out verMajor);
					int.TryParse(parts[1], out verMinor);
					break;
				default:
					return false;
			}

			if (major == 0 && minor == 0 && verFix == 0) return false;

			return verMajor > major || verMajor >= major && verMinor > minor ||
				   verMajor >= major && verMinor >= minor && verFix >= fix;
		}
	}
}