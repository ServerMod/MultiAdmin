using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace MutliAdmin
{
    internal class Program
    {
        private static Thread readerThread = new Thread(new ThreadStart(Program.Reader));
        private static Thread printerThread = new Thread(new ThreadStart(Program.Printer));
        private static string verString = "18.01.b";
        private static Process gameProcess = (Process)null;
        private static int tooLowMemory = 0;
        private static Random random = new Random();
        private static string session = "";
        private static int logID = 0;
        private static string config;
        private static string config_location;
        private static Boolean restart;

        private static string RandomString(int length)
        {
            return new string(Enumerable.Repeat<string>("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", length).Select<string, char>((Func<string, char>)(s => s[Program.random.Next(s.Length)])).ToArray<char>());
        }

        private static void Write(string content, ConsoleColor color = ConsoleColor.White, int height = 0)
        {
            Thread.Sleep(100);
            Console.CursorTop += height;
            Console.ForegroundColor = color;
            DateTime now = DateTime.Now;
            string str = "[" + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + "] ";
            Console.WriteLine(content == "" ? "" : str + content);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void SwapConfigs()
        {
 
  
            if (File.Exists("servers\\" + config + "\\config.txt"))
            {
                var contents = File.ReadAllText(Directory.GetCurrentDirectory() + "\\servers\\" + config + "\\config.txt");
                File.WriteAllText(config_location, contents);
                Program.Write("Config file swapped", ConsoleColor.DarkYellow, 0);
            }
            else
            {
                Program.Write("Config file for server " + config + " does not exist! expected location:" + "servers\\" + config + "\\config.txt", ConsoleColor.DarkYellow, 0);
                throw new FileNotFoundException("config file not found");
            }

        }

        public static void LoadMultiAdminConfig()
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\multiadmin.cfg"))
            {
                foreach (String line in File.ReadAllLines("multiadmin.cfg"))
                {
                    if (line.Substring(0, 8).Equals("cfg_loc="))
                    {
                        config_location = Environment.ExpandEnvironmentVariables(line.Substring(8));
                        Program.Write("CFG LOCATION FROM FILE:" + config_location);
                    }
                }
               
            }
            else
            {
                FindConfig();
            }
        }

  
        public static void FindConfig()
        {
            var path = Environment.ExpandEnvironmentVariables("%appdata%\\SCP Secret Laboratory\\config.txt");
            var backup = Environment.ExpandEnvironmentVariables("%appdata%\\SCP Secret Laboratory\\config_backup.txt");
            if (File.Exists(path))
            {
                config_location = path;
                Program.Write("Config file located at: " + path, ConsoleColor.DarkYellow, 0);

                if (!File.Exists(backup))
                {
                    Program.Write("Config file has not been backed up, creating backup copy under: " + backup, ConsoleColor.DarkYellow, 0);
                    File.Copy(path, backup);
                }
            }
            else
            {
                throw new FileNotFoundException("Default config file not in expected location (%appdata%/SCP Secret Laboratory/config.txt), try runing LocalAdmin first");
            }
        }


        private static void WriteCrashLog(String reason)
        {
            var path = "servers/" + config + "/crash_reasons.txt";
        
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + reason);
            }

        }

        public static String GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

      
		
		public static void Main(string[] args)
		{
            LoadMultiAdminConfig();
            restart = false;
			String chain = "";
			if (args.Length > 0)
			{
				config = args[0];
				Program.Write("Starting this instance with config directory:" + config, ConsoleColor.DarkYellow, 0);
				// chain the rest
				string[] newArgs = args.Skip(1).Take(args.Length - 1).ToArray();
				chain = "\"" + string.Join("\" \"", newArgs).Trim() + "\"";
			}
			else
			{
				// start all servers, the first server will be this one
				bool first = true;
				if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\servers\\"))
				{
					Program.Write("Server directory not found, please make a new directory in the following format:", ConsoleColor.DarkYellow, 0);
					Program.Write(Directory.GetCurrentDirectory() + "\\servers\\<Server id>\\config.txt", ConsoleColor.Cyan, 0);
					Program.Write("Once corrected please restart this exe.", ConsoleColor.DarkYellow, 0);
					Thread.Sleep(10000);
					return;
				}

				String[] dirs = Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\servers\\");
				foreach (string file in dirs)
				{
					String name = new DirectoryInfo(file).Name;
					if (first)
					{
						config = name;
						Program.Write("Starting this instance with config directory: " + name, ConsoleColor.DarkYellow, 0);
						first = false;
					}
					else
					{
						if (!File.Exists(Directory.GetCurrentDirectory() + "\\servers\\" + name + "\\manual"))
						{
							chain += "\"" + name + "\" ";
						}
						else
						{
							Program.Write("Skipping auto start for: " + name, ConsoleColor.DarkYellow, 0);
						}
						
					}

                    // make log folder

                    if (!Directory.Exists(file + "\\logs"))
                    {
                        Directory.CreateDirectory(file + "\\logs");
                    }
				}

			}

			Console.Title = "SCP Server - Config: " + config + " Session ID:" + session;

			Program.session = Program.RandomString(20);
			Program.logID = 0;
			Program.ShowPresetByAlias("menu");
			Program.Write("Preparing files...", ConsoleColor.Gray, 0);
			try
			{
				Directory.CreateDirectory("SCPSL_Data/Dedicated/" + Program.session);
				Program.Write("Started new session.", ConsoleColor.DarkGreen, 0);
			}
			catch
			{
				Program.Write("Failed - Please close all open files in SCPSL_Data/Dedicated and restart the server!", ConsoleColor.Red, 0);
				Program.Write("Press any key to close...", ConsoleColor.DarkGray, 0);
				Console.ReadKey(true);
				Process.GetCurrentProcess().Kill();
			}
			Program.Write("Trying to start server...", ConsoleColor.Gray, 0);
			try
			{
				string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "SCPSL.*", SearchOption.TopDirectoryOnly);
				Program.Write("Executing: " + files[0], ConsoleColor.DarkGreen, 0);
				SwapConfigs();
				Program.gameProcess = Process.Start(files[0], "-batchmode -nographics -key" + Program.session + " -id" + (object)Process.GetCurrentProcess().Id + " -logFile \"servers\\" + config +  "\\logs\\" + GetDate() + "_output_log.txt\"");
			}
			catch (Exception e)
			{
				Program.Write("Failed - Executable file not found or config issue!", ConsoleColor.Red, 0);
				Program.Write(e.Message, ConsoleColor.Red, 0);
				Program.Write("Press any key to close...", ConsoleColor.DarkGray, 0);
				Console.ReadKey(true);
				Process.GetCurrentProcess().Kill();
			}

			Program.printerThread.Start();
			Program.readerThread.Start();



			// start other servers
			if (chain.Trim().Length > 0 && chain != "\"\"")
			{
				try
				{
					// give this server time to claim port
					Thread.Sleep(10000);
					Process.Start(Directory.GetFiles(Directory.GetCurrentDirectory(), "MultiAdmin.*")[0], chain);
				}
				catch
				{
				}
			}



			while (true)
			{
				if (Program.gameProcess != null && !Program.gameProcess.HasExited)
				{
					Program.gameProcess.Refresh();
					if ((int)(Program.gameProcess.WorkingSet64 / 1048576L) < 400 && Program.gameProcess.StartTime.AddMinutes(3.0) < DateTime.Now)
						++Program.tooLowMemory;
					else
						Program.tooLowMemory = 0;
				}
				if (Program.gameProcess == null || Program.gameProcess.HasExited || Program.tooLowMemory > 5 || Program.gameProcess.MainWindowTitle != "")
				{
					Program.Write("Session crashed. Trying to restart dedicated server...", ConsoleColor.Red, 0);

                    if (Program.gameProcess == null || Program.gameProcess.HasExited)
                    {
                        Program.Write("Game engine exited", ConsoleColor.Red, 0);
                        WriteCrashLog("Game enginge has exited or the process is gone, check AppData\\LocalLow\\Hubert Moszka\\SCP_ Secret Laboratory\\Crashes for unity logs or servers/<config>/logs/.");
                    }

					if (Program.tooLowMemory > 5)
					{
						Program.Write("Out of memory", ConsoleColor.Red, 0);
                        WriteCrashLog("The game engines working memory was too low for more than 5 ticks.");
                    }


					foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "MultiAdmin.*"))
					{
						try
						{
							Process.Start(file, config);
							try
							{
								Program.gameProcess.Kill();
							}
							catch
							{
							}
							bool flag = false;
							while (!flag)
							{
								flag = true;
								foreach (Process process in Process.GetProcesses())
								{
									if (process != null && process.ProcessName.Contains("SCPSL") && process.MainWindowTitle.Length > 2)
									{
										flag = false;
										process.Kill();
										break;
									}
								}
							}
							Process.GetCurrentProcess().Kill();
							return;
						}
						catch
						{
						}
					}
				}
				Thread.Sleep(2000);
			}
		}

		private static void ShowPresetByAlias(string alias)
		{
			if (alias == "menu")
			{
				Program.Write("SCP: Secret Laboratory - MutliAdmin based off LocalAdmin version - " + Program.verString, ConsoleColor.Cyan, 0);
				Program.Write("", ConsoleColor.White, 0);
				Program.Write("LocalAdmin Licensed under a CC-BY-SA 4.0 license: http://creativecommons.org/licenses/by-sa/4.0/", ConsoleColor.DarkGray, 0);
				Program.Write("You can request the source code at: moszka.hubert@gmail.com", ConsoleColor.DarkGray, 0);
				Program.Write("Created by Hubert Moszka. Special thanks to Ninjaboi8175 for help.", ConsoleColor.DarkGray, 0);
				Program.Write("MultiAdmin authored by Courtney - Grover_c13 (http://github.com/Grover-c13)", ConsoleColor.DarkGray, 0);
				Program.Write("", ConsoleColor.White, 0);
				Program.Write("Type 'help' to get list of available commands.", ConsoleColor.Cyan, 0);
				Program.Write("", ConsoleColor.White, 0);
			}
			else
			{
				if (!(alias == "help"))
					return;
				Program.Write("", ConsoleColor.White, 0);
				Program.Write("----HELP----", ConsoleColor.DarkGray, 0);
				Program.Write("NEW <config> - starts the next server on different port with a given config file.", ConsoleColor.White, 0);
				Program.Write("BAN - time-bans player using IP address or part of the nickname.", ConsoleColor.White, 0);
				Program.Write("FORCESTART - forces the round to start.", ConsoleColor.White, 0);
				Program.Write("ROUNDRESTART - forces the round to restart.", ConsoleColor.White, 0);
				Program.Write("HELLO - tests if server is responding.", ConsoleColor.White, 0);
				Program.Write("CONFIG - opens the server's configuration file. (will not work well with MutliAdmin)", ConsoleColor.White, 0);
				Program.Write("CONFIG RELOAD - applies config changes. (will swap before reload)", ConsoleColor.White, 0);
				Program.Write("EXIT - stops the server.", ConsoleColor.White, 0);
				Program.Write("SEED - shows the current map seed in order to re-generate level in the future.", ConsoleColor.White, 0);
				Program.Write("BANREFRESH - forces ban database to refresh.", ConsoleColor.White, 0);
                Program.Write("RESTARTNEXTROUND - restarts the server after this round completes.", ConsoleColor.White, 0);
                Program.Write("------------" + Environment.NewLine, ConsoleColor.DarkGray, 0);
			}
		}

		private static void Printer()
		{
			while (true)
			{
				string[] strArray = new string[0];
				try
				{
					strArray = Directory.GetFiles("SCPSL_Data/Dedicated/" + Program.session, "sl*.mapi", SearchOption.TopDirectoryOnly);
				}
				catch
				{
					Program.Write("Message printer warning: 'SCPSL_Data/Dedicated' directory not found.", ConsoleColor.Yellow, 0);
					Program.Write("Press any key to ignore...", ConsoleColor.DarkGray, 0);
					Console.ReadKey();
				}
				foreach (string path in strArray)
				{
					string str1 = "";
					int num = 9;
					string str2 = "open";
					try
					{
						StreamReader streamReader = new StreamReader(path);
						str1 = streamReader.ReadToEnd();
						str2 = "close";
						streamReader.Close();
						str2 = "delete";
						File.Delete(path);
					}
					catch
					{
						Program.Write("Message printer warning: Could not " + str2 + " file " + path + ". Make sure that MultiAdmin.exe has all necessary read-write permissions.", ConsoleColor.Yellow, 0);
						Program.Write("Press any key to ignore...", ConsoleColor.DarkGray, 0);
						Console.ReadKey();
					}

                    if (str1.Contains("Waiting for players") && restart)
                    {
                        restart = false;
                        Program.gameProcess.Kill();
                        Process.Start(Directory.GetFiles(Directory.GetCurrentDirectory(), "MultiAdmin.*")[0], config);
                        Process.GetCurrentProcess().Kill();
                    }

					if (str1.Contains("LOGTYPE"))
					{
						try
						{
							string str3 = str1.Remove(0, str1.IndexOf("LOGTYPE") + 7);
							num = int.Parse(str3.Contains("-") ? str3.Remove(0, 1) : str3);
							str1 = str1.Remove(str1.IndexOf("LOGTYPE") + 9);
						}
						catch
						{
							num = 9;
						}
					}
					if (!string.IsNullOrEmpty(str1))
					{
						while ((uint)Console.CursorLeft > 0U)
							Thread.Sleep(100);
						Program.Write(str1.Contains("LOGTYPE") ? str1.Substring(0, str1.Length - 9) : str1, (ConsoleColor)num, 0);
					}
				}
				Thread.Sleep(300);
			}
		}

		private static void SendMessage(string message)
		{
			StreamWriter streamWriter = new StreamWriter("SCPSL_Data/Dedicated/" + Program.session + "/cs" + (object)Program.logID + ".mapi");
			++Program.logID;
			streamWriter.WriteLine(message + "terminator");
			streamWriter.Close();
			Program.Write("Sending request to SCP: Secret Laboratory...", ConsoleColor.White, 0);
		}

		private static void Reader()
		{
			while (true)
			{
				string message = Console.ReadLine();
				int cursorTop = Console.CursorTop;
				Console.SetCursorPosition(0, Console.CursorTop - 1);
				Console.Write(new string(' ', Console.WindowWidth));
				Program.Write(">>> " + message, ConsoleColor.DarkMagenta, -1);
				Console.SetCursorPosition(0, cursorTop);
				string[] strArray = message.ToUpper().Split(' ');
				if (strArray.Length > 0)
				{
					string str = strArray[0];
					if (str == "HELP")
					{
						Program.ShowPresetByAlias("help");
					}
					else if (str == "NEW")
					{
						try
						{
							if (strArray.Length == 2)
							{
								// dont start unless config is provided
								Process.Start(Directory.GetFiles(Directory.GetCurrentDirectory(), "MultiAdmin.*")[0], strArray[1]);
							}
							else
							{
								Program.Write("Please provide a config folder name", ConsoleColor.DarkYellow, 0);
							}

						}
						catch
						{
						}

					}
					else if (str == "CONFIG RELOAD")
					{
						SwapConfigs();
						Program.SendMessage(message);
					}
                    else if (str == "RESTARTNEXTROUND")
                    {
                        restart = true;
                        Program.Write("Server will restart at end of next round");
                    }
                    else
					{
						Program.SendMessage(message);
					}
				}
			}
		}
	}
}
