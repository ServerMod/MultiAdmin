using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using MultiAdmin.MultiAdmin;

namespace MutliAdmin
{
    public static class Program
    {
        private static string configKey;
        private static string configLocation;
        private static string configChain;
        private static MultiAdmin.Config multiadminConfig;
        private static Server server;


        public static void Write(String message, ConsoleColor color = ConsoleColor.DarkYellow)
        {
			if (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero)
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
			var defaultLoc = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/config.txt";
			var path = Program.multiadminConfig.GetValue("cfg_loc", defaultLoc);
            var backup = path.Replace(".txt", "_backup.txt");

			if (!File.Exists(path))
			{
				Write("Default config file not in expected location (" + path + "), copying config_template.txt");
				File.Copy("config_template.txt", path);
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
				// should never happen
				throw new FileNotFoundException("Config.txt file not found! something has gone wrong with initial setup, try running LocalAdmin.exe first");
            }

			return true;
        }



        public static Boolean StartHandleConfigs(string[] args)
        {
            Boolean hasServerToStart = false;
            if (args.Length > 0)
            {
                configKey = args[0];
                hasServerToStart = true;
                multiadminConfig = new MultiAdmin.Config(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + configKey + Path.DirectorySeparatorChar + "config.txt");
                Write("Starting this instance with config directory:" + configKey, ConsoleColor.DarkYellow);
                // chain the rest
                string[] newArgs = args.Skip(1).Take(args.Length - 1).ToArray();
                configChain = "\"" + string.Join("\" \"", newArgs).Trim() + "\"";
            }
            else
            {
                // start all servers, the first server will be this one
                bool first = true;
                if (!Directory.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers"))
                {
                    Write("Server directory not found, please make a new directory in the following format:", ConsoleColor.DarkYellow);
                    Write(Directory.GetCurrentDirectory() + "servers\\<Server id>\\config.txt", ConsoleColor.Cyan);
                    Write("Once corrected please restart this exe.", ConsoleColor.DarkYellow);
                    return false;
                }

                String[] dirs = Directory.GetDirectories(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar);
                foreach (string file in dirs)
                {
                    String name = new DirectoryInfo(file).Name;
                    if (first)
                    {
                        multiadminConfig = new MultiAdmin.Config(file + Path.DirectorySeparatorChar + "config.txt");
                        Program.Write(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "config.txt");
                        if (multiadminConfig.GetValue("MANUAL_START", "false").Equals("true"))
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
                        var other_config = new MultiAdmin.Config(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "config.txt");
                        Write(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "config.txt");
                        if (other_config.GetValue("MANUAL_START", "false").Equals("true"))
                        {
                            Write("Skipping auto start for: " + name, ConsoleColor.DarkYellow);
                        }
                        else
                        {
                            configChain += "\"" + name + "\" ";
                        }

                    }

                    // make log folder

                    if (!Directory.Exists(file + Path.DirectorySeparatorChar + "logs"))
                    {
                        Directory.CreateDirectory(file + Path.DirectorySeparatorChar + "logs");
                    }
                }

            }

            if (!hasServerToStart)
            {
                Write("All servers are set to manual start! you should have at least one config that auto starts", ConsoleColor.Red);
            }

            return hasServerToStart;
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

		private static void FixTypo()
		{
			// some idiot (courtney) accidently made the config file spc_multiadmin.cfg instead of scp_multiadmin.cfg
			// this method fixes it
			if (File.Exists("spc_multiadmin.cfg"))
			{
				Write("Renaming spc_multiadmin.cfg to scp_multiadmin.cfg");
				File.Move("spc_multiadmin.cfg", "scp_multiadmin.cfg");
			}
		}

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);
			FixTypo();
            multiadminConfig = new MultiAdmin.Config("scp_multiadmin.cfg");
            if (!FindConfig())
			{
				Console.ReadKey();
				return;
			}

            configChain = "";
            if (StartHandleConfigs(args))
            {
                server = new Server(GetServerDirectory(), configKey, multiadminConfig, configLocation, configChain);
            }
            else
            {
                Console.ReadKey();
            }
        }
    }
}