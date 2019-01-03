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
		public static readonly string MaVersion = "3.0.0";
		public readonly Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

		public readonly List<Feature> features = new List<Feature>();

		private readonly Thread printerThread;
		private readonly Thread readerThread;

		public readonly MultiAdminConfig serverConfig;

		public readonly string serverId;
		public readonly string configLocation;

		// we want a tick only list since its the only event that happens constantly, all the rest can be in a single list
		private readonly List<IEventTick> tick = new List<IEventTick>();

		private string currentLine = string.Empty;

		public bool hasServerMod;

		public bool initialRoundStarted;

		private int logId;
		public string serverModBuild;
		public string serverModVersion;

		public Server(string serverId = null, string configLocation = null)
		{
			readerThread = new Thread(() => InputThread.Write(this));
			printerThread = new Thread(() => OutputThread.Read(this));

			// Register all features
			RegisterFeatures();

			// Load config
			serverConfig = new MultiAdminConfig(FileManager.AppFolder + "scp_multiadmin.cfg");

			this.serverId = serverId;
			this.configLocation = configLocation ?? serverConfig.ConfigLocation;

			// Init features
			InitFeatures();
		}

		public bool Stopping
		{
			get;
			private set;
		}

		public string StartDateTime
		{
			get;
			private set;
		}

		public string ServerDir => "servers" + Path.DirectorySeparatorChar + serverId;
		public string LogDir => (string.IsNullOrEmpty(serverId) ? string.Empty : ServerDir + Path.DirectorySeparatorChar) + "logs";

		public string LogDirFile
		{
			get
			{
				if (string.IsNullOrEmpty(StartDateTime))
					throw new NullReferenceException("Server has not been started yet");

				return LogDir + Path.DirectorySeparatorChar + StartDateTime + "_{0}_output_log.txt";
			}
		}
		public string MaLogFile => string.Format(LogDirFile, "MA");
		public string ScpLogFile => string.Format(LogDirFile, "SCP");
		public string ModLogFile => string.Format(LogDirFile, "MODERATOR");

		public Process GameProcess { get; private set; }

		public string SessionId { get; private set; } = Utils.UnixTime;

		public string SessionDirectory => "SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" +
		                                  Path.DirectorySeparatorChar + SessionId;

		private static IEnumerable<Type> GetTypesWithHelpAttribute(Type attribute)
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
			Type[] assembly = GetTypesWithHelpAttribute(typeof(FeatureAttribute)).ToArray();
			foreach (Type type in assembly)
			{
				try
				{
					object featureInstance = Activator.CreateInstance(type, this);
					if (featureInstance is Feature feature)
					{
						RegisterFeature(feature);
					}
				}
				catch
				{
					// ignored
				}
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

		private void MainLoop()
		{
			while (!Stopping)
			{
				if (GameProcess != null && !GameProcess.HasExited)
				{
					foreach (IEventTick tickEvent in tick) tickEvent.OnTick();
				}
				else if (!Stopping)
				{
					foreach (Feature f in features)
						if (f is IEventCrash eventCrash)
							eventCrash.OnCrash();

					Write("Game engine exited/crashed/closed/restarting", ConsoleColor.Red);
					Write("Cleaning Session", ConsoleColor.Red);
					DeleteSession();
					SessionId = Utils.UnixTime;
					Write($"Restarting game with new session id ({SessionId})");
					StartServer();
					InitFeatures();
				}

				Thread.Sleep(1000);
			}

			CleanUp();
		}

		public void RegisterFeature(Feature feature)
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


		public void PrepareFiles()
		{
			try
			{
				Directory.CreateDirectory(SessionDirectory);
				Write("Started new session.", ConsoleColor.DarkGreen);
			}
			catch
			{
				Write($"Failed - Please close all open files in \"SCPSL_Data{Path.DirectorySeparatorChar}Dedicated\" and restart the server!",
					ConsoleColor.Red);
				Write("Press any key to close...", ConsoleColor.DarkGray);
				Console.ReadKey(true);
				Process.GetCurrentProcess().Kill();
			}
		}

		public void Write(string message, ConsoleColor color = ConsoleColor.Yellow, int height = 0)
		{
			Log(message);

			if (!Utils.IsProcessHandleZero)
			{
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
					message = TimeStamp(message);
					Console.WriteLine(message);
					Console.ForegroundColor = ConsoleColor.White;
					Console.BackgroundColor = ConsoleColor.Black;
				}
				catch (ArgumentOutOfRangeException e)
				{
					Console.WriteLine(TimeStamp("Value " + cursorTop + " exceeded buffer height " + bufferHeight +
					                            "."));
					Console.WriteLine(e.StackTrace);
				}
			}
		}

		public void WritePart(string part, ConsoleColor backgroundColor = ConsoleColor.Black,
			ConsoleColor textColor = ConsoleColor.Yellow, bool date = false, bool lineEnd = false)
		{
			string datepart = string.Empty;
			if (date)
			{
				DateTime now = DateTime.Now;
				datepart = "[" + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" +
				           now.Second.ToString("00") + "] ";
			}

			Console.ForegroundColor = textColor;
			Console.BackgroundColor = backgroundColor;
			if (lineEnd)
			{
				if (part.EndsWith(Environment.NewLine))
					Console.Write(datepart + part);
				else
					Console.Write(datepart + part + Environment.NewLine);
				currentLine += datepart + part;
				Log(currentLine);
				currentLine = string.Empty;
			}
			else
			{
				Console.Write(datepart + part);
				currentLine += datepart + part;
			}
		}

		public void Log(string message)
		{
			lock (this)
			{
				using (StreamWriter sw = File.AppendText(MaLogFile))
				{
					message = TimeStamp(message);
					sw.WriteLine(message);
				}
			}
		}

		public void SoftRestartServer()
		{
			SendMessage("RECONNECTRS");
			SessionId = Utils.UnixTime;
		}

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

		public void StopServer(bool killGame = true)
		{
			foreach (Feature f in features)
				if (f is IEventServerStop stopEvent)
					stopEvent.OnServerStop();

			if (killGame) GameProcess.Kill();
			Stopping = true;
		}

		public void CleanUp()
		{
			DeleteSession();
		}

		public void StartServer()
		{
			initialRoundStarted = false;
			try
			{
				StartDateTime = Utils.DateTime;

				PrepareFiles();

				string file;

				switch (Environment.OSVersion.Platform)
				{
					case PlatformID.Unix:
						file = "SCPSL.x86_64";
						break;

					case PlatformID.Win32S:
					case PlatformID.Win32Windows:
					case PlatformID.Win32NT:
					case PlatformID.WinCE:
						file = "SCPSL.exe";
						break;

					default:
						throw new FileNotFoundException("Invalid OS, can't run executable");
				}

				if (!File.Exists(file))
					throw new FileNotFoundException($"Can't find game executable \"{file}\"");

				Write($"Executing \"{file}\"...", ConsoleColor.DarkGreen);

				List<string> args = new List<string>( new[]
				{
					"-batchmode",
					"-nographics",
					"-silent-crashes",
					"-nodedicateddelete",
					$"-key{SessionId}",
					$"-id{Process.GetCurrentProcess().Id}",
					$"-{(serverConfig.NoLog ? "nolog" : $"logFile \"{ScpLogFile}\"")}"
				});

				if (serverConfig.DisableConfigValidation)
					args.Add("-disableconfigvalidation");

				if (serverConfig.ShareNonConfigs)
					args.Add("-sharenonconfigs");

				if (!string.IsNullOrEmpty(configLocation))
					args.Add(configLocation);

				string argsString = string.Join(" ", args);

				Write("Starting server with the following parameters");
				Write(file + " " + argsString);

				ProcessStartInfo startInfo = new ProcessStartInfo(file) {Arguments = argsString};

				GameProcess = Process.Start(startInfo);

				foreach (Feature f in features)
					if (f is IEventServerPreStart eventPreStart)
						eventPreStart.OnServerPreStart();

				readerThread.Start();
				printerThread.Start();
				MainLoop();
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

		public static string TimeStamp(string message)
		{
			if (string.IsNullOrEmpty(message))
				return string.Empty;
			DateTime now = DateTime.Now;
			return "[" + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" +
			       now.Second.ToString("00") + "] " + message;
		}
	}
}