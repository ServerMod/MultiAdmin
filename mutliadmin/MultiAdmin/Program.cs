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
            Console.ForegroundColor = color;
            DateTime now = DateTime.Now;
            string str = "[" + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + "] ";
            Console.WriteLine(message == "" ? "" : str + message);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }




        public static void FindConfig()
        {
            var defaultLoc = Environment.ExpandEnvironmentVariables(String.Format("%appdata%{0}SCP Secret Laboratory{0}config.txt", Path.DirectorySeparatorChar));
            var path = Program.multiadminConfig.GetValue("cfg_loc", defaultLoc);
            var backup = path.Replace(".txt", "_backup.txt");
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
                throw new FileNotFoundException("Default config file not in expected location (" + path + "), try runing LocalAdmin first");
            }
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

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);
            multiadminConfig = new MultiAdmin.Config("spc_multiadmin.cfg");
            FindConfig();
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
