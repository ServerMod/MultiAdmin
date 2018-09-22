using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MultiAdmin;
using MultiAdmin.MultiAdmin;
//using YamlDotNet.Serialization;

namespace MutliAdmin
{
	public static class Program
	{
		private static string configKey;
		private static string configLocation;
		private static string configChain;
		private static Config multiadminConfig;
		private static Server server;
		private static bool multiMode = false;

		public static void Write(String message, ConsoleColor color = ConsoleColor.DarkYellow)
		{
			if (Server.SkipProcessHandle() || Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
			{
				Console.ForegroundColor = color;
				DateTime now = DateTime.Now;
				string str = "[" + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + "] ";
				Console.WriteLine(message == "" ? "" : str + message);
				Console.ForegroundColor = ConsoleColor.White;
				Console.BackgroundColor = ConsoleColor.Black;
			}
		}

		public static bool FindConfig()
		{
			var defaultLoc = FileManager.AppFolder + "config_gameplay.txt";
			var path = Program.multiadminConfig.config.GetString("cfg_loc", defaultLoc);
			var backup = path.Replace(".txt", "_backup.txt");

			// Copy config template
			if (!Directory.Exists(FileManager.AppFolder))
			{
				Directory.CreateDirectory(FileManager.AppFolder);
			}
			if (!File.Exists(path))
			{
				try
				{
					Write("Default config file not in expected location (" + path + "), copying \"MiscData/gameconfig_template.txt\"...");
					File.Copy("MiscData/gameconfig_template.txt", path);
					Write("Copied default config!");
				}
				catch
				{
					Write("Error copying template file \"MiscData/gameconfig_template.txt\"!");
				}
			}

			if (File.Exists(path))
			{
				configLocation = path;
				Program.Write("Config file located at: " + path, ConsoleColor.DarkYellow);

				if (!File.Exists(backup))
				{
					Program.Write("Config file has not been backed up, creating backup copy under: " + backup, ConsoleColor.DarkYellow);
					File.Copy(path, backup);
				}
			}
			else
			{
				Write("No default config found, no backup needed.");
				//throw new FileNotFoundException("Config.txt file not found! something has gone wrong with initial setup, try running LocalAdmin.exe first");
			}

			return true;
		}

		public static bool StartHandleConfigs(string[] args)
		{
			Boolean hasServerToStart = false;
			if (args.Length > 0)
			{
				configKey = args[0];
				hasServerToStart = true;
				multiMode = true;
				// This shouldnt be the server specific config, it should be the global one scp_config?
				//multiadminConfig = new MultiAdmin.Config(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + configKey + Path.DirectorySeparatorChar + "config.txt");
				Write("Starting this instance with config directory:" + configKey, ConsoleColor.DarkYellow);
				// chain the rest
				string[] newArgs = args.Skip(1).Take(args.Length - 1).ToArray();
				configChain = "\"" + string.Join("\" \"", newArgs).Trim() + "\"";
			}
			else
			{
				// The first check sees if the "servers" directory exists, and if it does, 
				//  the second check will see if it is empty.
				if (Directory.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers") &&
					HasSubdirs(Directory.GetDirectories(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers")))
				{
					Write("Using multiple server mode", ConsoleColor.Green);
					multiMode = true;
					hasServerToStart = LoadserverFolders();
				}
				else
				{
					// Either there is no "servers" folder or it is empty, and starting a normal server
					multiMode = false;
					hasServerToStart = true;
					Write("Using default server mode", ConsoleColor.Green);
					Write("Server directory not found or it is empty, if you want to use multiple server mode, please make a new directory in the following format:", ConsoleColor.Yellow);
					Write(Directory.GetCurrentDirectory() + "\\servers\\<Server id>\\config.txt", ConsoleColor.Yellow);
				}
			}

			if (!hasServerToStart)
			{
				Write("All servers are set to manual start! you should have at least one config that auto starts", ConsoleColor.Red);
			}

			return hasServerToStart;
		}

		public static bool HasSubdirs(string[] dirs) => dirs.Length > 0;


		public static bool LoadserverFolders()
		{
			bool hasServerToStart = false;
			bool first = true;
			String[] dirs = Directory.GetDirectories(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar);
			foreach (string file in dirs)
			{
				String name = new DirectoryInfo(file).Name;
				if (first)
				{
					var serverConfig = new MultiAdmin.Config(file + Path.DirectorySeparatorChar + "config.txt");
					Program.Write(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "config.txt");
					if (serverConfig.config.GetBool("manual_start", false))
					{
						Write("Skipping auto start for: " + name, ConsoleColor.DarkYellow);
					}
					else
					{
						hasServerToStart = true;
						configKey = name;
						Write("Starting this instance with config directory: " + name, ConsoleColor.DarkYellow);
						first = false;
					}

				}
				else
				{
					var other_config = new Config(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "config.txt");
					Write(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "config.txt");
					if (other_config.config.GetBool("manual_start", false))
					{
						Write("Skipping auto start for: " + name, ConsoleColor.DarkYellow);
					}
					else
					{
						configChain += "\"" + name + "\" ";
					}

				}
			}

			return hasServerToStart;
		}

		public static void ConvertConfigs()
		{
			String[] dirs = Directory.GetDirectories(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar);
			foreach (string file in dirs)
			{
				var name = file + Path.DirectorySeparatorChar + "config.txt";
				var backup = file + Path.DirectorySeparatorChar + "config.backup";
				Write("Converting old config to YAML: " + file, ConsoleColor.Green);
				OldConfig config = new OldConfig(file + Path.DirectorySeparatorChar + "config.txt");
				//var serializer = new SerializerBuilder().Build();
				//var yaml = serializer.Serialize(config.values);
				//Write(yaml, ConsoleColor.White);

				string yaml = "";

				foreach (string line in config.GetRaw())
				{
					try
					{
						string commentText = "//"; // Dunno if everyone uses that comment style, whatever

						if (line.Contains("=") && line.Contains(";") && line.IndexOf("=") < line.IndexOf(";")) // Matches format "key = value; //comment"
						{
							string keyandEquals = line.Substring(0, line.IndexOf("=")).Trim(); // "key " -> "key"
							string value = line.Substring(line.IndexOf("=") + 1, line.IndexOf(";") - (line.IndexOf("=") + 1)).Trim(); // " value" -> "value"
							string followingContent = line.Substring(line.IndexOf(";") + 1, line.Length - (line.IndexOf(";") + 1)).Trim(); // " //comment" -> "//comment"

							if (followingContent.StartsWith(commentText)) // "//comment" -> "comment"
							{
								followingContent = followingContent.Substring(commentText.Length).Trim();
							}

							string newLine = keyandEquals + ": " + value;

							// Write any comments
							if (!string.IsNullOrEmpty(followingContent))
							{
								yaml += (followingContent.StartsWith("#") ? "" : "#") + followingContent + Environment.NewLine;
							}

							yaml += newLine + Environment.NewLine;
						}
						else
						{
							string newLine = line.Trim();

							if (newLine.StartsWith(commentText)) // "//comment" -> "comment"
							{
								newLine = newLine.Substring(commentText.Length).Trim();
							}

							newLine = (string.IsNullOrEmpty(newLine) ? "" : (newLine.StartsWith("#") ? "" : "#") + newLine);

							yaml += newLine + Environment.NewLine;
						}
					}
					catch (Exception e)
					{
						Write(e.Message);
						Write(e.Source);
						Write(e.StackTrace);
					}
				}

				File.Copy(name, backup);
				File.WriteAllText(name, yaml);

				Write(yaml, ConsoleColor.White);

				Write("Success!", ConsoleColor.Green);
			}
		}

		public static String GetServerDirectory()
		{
			return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers";
		}

		static void OnExit(object sender, EventArgs e)
		{
			Console.WriteLine("exit");
			Debug.Write("exit");
			Console.ReadKey();
		}

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);
			multiadminConfig = new Config("scp_multiadmin.cfg");
			if (!FindConfig())
			{
				Console.ReadKey();
				return;
			}
			if (args.Length == 1)
			{
				if (args[0].Equals("--convert-config"))
				{
					ConvertConfigs();
					Console.ReadKey();
					return;
				}
			}


			configChain = "";
			if (StartHandleConfigs(args))
			{
				server = new Server(GetServerDirectory(), configKey, multiadminConfig, configLocation, configChain, multiMode);
			}
			else
			{
				Console.ReadKey();
			}

		}
	}
}