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
        private static string config_chain;
        private static long round_start_wait_time;
        private static long inactivity_shutoff;
        private static Boolean restart;
        private static Boolean shutdown;
        private static Boolean waiting_for_round;
        private static int round_count;
        private static MultiAdmin.ConfigParser config_file;

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
 
            if (File.Exists("servers" + Path.DirectorySeparatorChar + config + Path.DirectorySeparatorChar + "config.txt"))
            {
                var contents = File.ReadAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + config + Path.DirectorySeparatorChar + "config.txt");
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
            if (File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "spc_multiadmin.cfg"))
            {
                foreach (String line in File.ReadAllLines("spc_multiadmin.cfg"))
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
            var path = Environment.ExpandEnvironmentVariables(String.Format("%appdata%{0}SCP Secret Laboratory{0}config.txt", Path.DirectorySeparatorChar));
            var backup = Environment.ExpandEnvironmentVariables(String.Format("%appdata%{0}SCP Secret Laboratory{0}config_backup.txt", Path.DirectorySeparatorChar));
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
            var path = "servers" + Path.DirectorySeparatorChar + config + Path.DirectorySeparatorChar + "crash_reasons.txt";
        
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + reason);
            }

        }

        public static String GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_HH_mm");
        }


        public static void StartHandleConfigs(string[] args)
        {
            if (args.Length > 0)
            {
                config = args[0];
                config_file = new MultiAdmin.ConfigParser(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + config + Path.DirectorySeparatorChar + "config.txt");
                Program.Write("Starting this instance with config directory:" + config, ConsoleColor.DarkYellow, 0);
                // chain the rest
                string[] newArgs = args.Skip(1).Take(args.Length - 1).ToArray();
                config_chain = "\"" + string.Join("\" \"", newArgs).Trim() + "\"";
            }
            else
            {
                // start all servers, the first server will be this one
                bool first = true;
                if (!Directory.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers"))
                {
                    Program.Write("Server directory not found, please make a new directory in the following format:", ConsoleColor.DarkYellow, 0);
                    Program.Write(Directory.GetCurrentDirectory() + "servers\\<Server id>\\config.txt", ConsoleColor.Cyan, 0);
                    Program.Write("Once corrected please restart this exe.", ConsoleColor.DarkYellow, 0);
                    Thread.Sleep(10000);
                    return;
                }

                String[] dirs = Directory.GetDirectories(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar);
                foreach (string file in dirs)
                {
                    String name = new DirectoryInfo(file).Name;
                    if (first)
                    {
                        config_file = new MultiAdmin.ConfigParser(file + Path.DirectorySeparatorChar + "config.txt");
                        config = name;
                        Program.Write("Starting this instance with config directory: " + name, ConsoleColor.DarkYellow, 0);
                        first = false;
                    }
                    else
                    {
                        var other_config = new MultiAdmin.ConfigParser(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "config.txt");
                        Program.Write(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "config.txt");
                        if (other_config.GetValue("MANUAL_START", "false").Equals("true"))
                        {
                            Program.Write("Skipping auto start for: " + name, ConsoleColor.DarkYellow, 0);
                        }
                        else
                        {
                            config_chain += "\"" + name + "\" ";
                        }

                    }

                    // make log folder

                    if (!Directory.Exists(file + Path.DirectorySeparatorChar + "logs"))
                    {
                        Directory.CreateDirectory(file + Path.DirectorySeparatorChar + "logs");
                    }
                }

            }
        }

        public static void PrepareFiles()
        {
            try
            {
                Directory.CreateDirectory("SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" + Path.DirectorySeparatorChar + Program.session);
                Program.Write("Started new session.", ConsoleColor.DarkGreen, 0);
            }
            catch
            {
                Program.Write("Failed - Please close all open files in SCPSL_Data/Dedicated and restart the server!", ConsoleColor.Red, 0);
                Program.Write("Press any key to close...", ConsoleColor.DarkGray, 0);
                Console.ReadKey(true);
                Process.GetCurrentProcess().Kill();
            }
        }
		
		public static void Main(string[] args)
		{
            LoadMultiAdminConfig();
            restart = false;
            shutdown = false;
            config_chain = "";
            waiting_for_round = true;
            round_start_wait_time = GetUnixTime();
            Console.WriteLine(config_chain);
            StartHandleConfigs(args);

            inactivity_shutoff = long.Parse(config_file.GetValue("SHUTDOWN_ONCE_EMPTY_FOR", "-1"));
            Program.session = Program.RandomString(20);
            Console.Title = "SCP Server - Config: " + config + " Session ID:" + session;
            Program.logID = 0;
			Program.ShowPresetByAlias("menu");
            Program.Write("Preparing files...", ConsoleColor.Gray, 0);
            PrepareFiles();
            Program.Write("Trying to start server...", ConsoleColor.Gray, 0);
            StartServer();


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


                // INACTIVITY SHUTDOWN
                if (waiting_for_round && inactivity_shutoff != -1)
                {
                    Program.Write(GetUnixTime() - round_start_wait_time + "");
                    if (GetUnixTime() - round_start_wait_time >= inactivity_shutoff)
                    {
                        Program.SendMessage("Shutting down server due to inactviity");
                        ShutdownServer();
                    }
                }

                Thread.Sleep(2000);
			}
		}


        public static long GetUnixTime()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            return (long) t.TotalSeconds;
        }

        private static void StartServer()
        {
            try
            {
                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "SCPSL.*", SearchOption.TopDirectoryOnly);
                Program.Write("Executing: " + files[0], ConsoleColor.DarkGreen, 0);
                SwapConfigs();
                String logdir = "servers" + Path.DirectorySeparatorChar + config + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar + GetDate() + "_output_log.txt";
                Program.Write("Starting server with the following parameters");
                Program.Write(files[0] + " -batchmode -nographics -key" + Program.session + " -id" + (object)Process.GetCurrentProcess().Id + " -logFile \"" + logdir + "\"");
                Program.gameProcess = Process.Start(files[0], "-batchmode -nographics -key" + Program.session + " -id" + (object)Process.GetCurrentProcess().Id + " -logFile \"" + logdir + "\"");

                CreateRunFile();
            }
            catch (Exception e)
            {
                Program.Write("Failed - Executable file not found or config issue!", ConsoleColor.Red, 0);
                Program.Write(e.Message, ConsoleColor.Red, 0);
                Program.Write("Press any key to close...", ConsoleColor.DarkGray, 0);
                RemoveRunFile();
                Console.ReadKey(true);
                Process.GetCurrentProcess().Kill();
            }

            Program.printerThread.Start();
            Program.readerThread.Start();
        }


        private static void ShutdownServer()
        {
            RemoveRunFile();
            Program.gameProcess.Kill();
            Process.GetCurrentProcess().Kill();
        }

        private static void RestartServer()
        {
            Program.gameProcess.Kill();
            Process.Start(Directory.GetFiles(Directory.GetCurrentDirectory(), "MultiAdmin.*")[0], config);
            Process.GetCurrentProcess().Kill();

        }

        private static void RemoveRunFile()
        {
            File.Delete(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + config + Path.DirectorySeparatorChar + "running");
        }

        private static void CreateRunFile()
        {
            File.Create(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + config + Path.DirectorySeparatorChar + "running").Close();
        }

        private static Boolean IsConfigRunning(String config)
        {
            return File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + config + Path.DirectorySeparatorChar + "running");
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
				Program.Write("MultiAdmin authored by Courtney - Grover_c13 (http://github.com/Grover-c13/MutliAdmin)", ConsoleColor.DarkGray, 0);
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
				string[] strArray = null;
				try
				{
					strArray = Directory.GetFiles("SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" + Path.DirectorySeparatorChar + Program.session, "sl*.mapi", SearchOption.TopDirectoryOnly);
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

                    if (str1.Contains("Waiting for players"))
                    {
                        if (restart)
                        {
                            RestartServer();
                        }

                        if (shutdown)
                        {
                            ShutdownServer();
                        }

                        round_count++;
                        round_start_wait_time = GetUnixTime();
                        waiting_for_round = true;
                        var restart_after = int.Parse(config_file.GetValue("RESTART_EVERY_NUM_ROUNDS", "-1"));
                        if (restart_after != -1 && round_count > restart_after)
                        {
                            RestartServer();
                        }
                    }

                    if (str1.Contains("New round has been started"))
                    {
                        waiting_for_round = false;
                    }

                    if (str1.Contains("Server starting at port"))
                    {
                        // start other servers
                        if (config_chain.Trim().Length > 0 && config_chain != "\"\"")
                        {
                            try
                            {
                                Process.Start(Directory.GetFiles(Directory.GetCurrentDirectory(), "MultiAdmin.*")[0], config_chain);
                            }
                            catch
                            {
                                SendMessage("Failed to start chained server with config chain:" + config_chain);
                            }
                        }
                    }


                    if (str1.Contains("Server full"))
                    {
                        var config_to_start = config_file.GetValue("START_CONFIG_ON_FULL", "disabled");
                        Program.Write(config_to_start);
                        if (!config_to_start.Equals("disabled") && !IsConfigRunning(config_to_start))
                        {
                            Process.Start(Directory.GetFiles(Directory.GetCurrentDirectory(), "MultiAdmin.*")[0], config_to_start);
                        }
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
			StreamWriter streamWriter = new StreamWriter("SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" + Path.DirectorySeparatorChar + Program.session + Path.DirectorySeparatorChar + "cs" + (object)Program.logID + ".mapi");
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
                            Program.Write("Failed to start new server");
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
                    else if (str == "SHUTDOWNNEXTROUND")
                    {
                        shutdown = true;
                        Program.Write("Server will shutdown at end of next round");
                    }
                    else if (str == "SHUTDOWN")
                    {
                        Program.SendMessage(message);
                        ShutdownServer();
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
