﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MultiAdmin.MultiAdmin.Commands;
using MultiAdmin.MultiAdmin.Features;

namespace MultiAdmin.MultiAdmin
{
    public class Server
    {
        public static readonly string MA_VERSION = "1.2.5";

        public Boolean HasServerMod { get; set; }
        public String ServerModVersion { get; set; }
        public Config MultiAdminCfg { get; }
        public Config ServerConfig
        {
            get
            {
                return serverConfig;
            }
        }
        public String ConfigKey { get; }
        public String MainConfigLocation { get; }
        public String ConfigChain { get;  }
        public String ServerDir { get;  }

        private Config serverConfig;
        public Boolean InitialRoundStarted { get; set; }

        public List<Feature> Features { get; }
        public Dictionary<String, ICommand> Commands { get; }
        private List<IEventTick> tick; // we want a tick only list since its the only event that happens constantly, all the rest can be in a single list

        private Thread readerThread;
        private Thread printerThread;

        private int logID;
        private Process gameProcess;
        private Boolean stopping;
        private String session_id;
        private String maLogLocation;
        public String logFolder { get; }

        public Server(String serverDir, String configKey, Config multiAdminCfg, String mainConfigLocation, String configChain)
        {
            MainConfigLocation = mainConfigLocation;
            ConfigKey = configKey;
            ConfigChain = configChain;
            ServerDir = serverDir;
            session_id = Utils.GetUnixTime().ToString();
            Commands = new Dictionary<string, ICommand>();
            Features = new List<Feature>();
            tick = new List<IEventTick>();
            MultiAdminCfg = multiAdminCfg;
            logFolder = "servers" + Path.DirectorySeparatorChar + ConfigKey + Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar;
            maLogLocation = logFolder + Utils.GetDate() + "_MA_output_log.txt";
            stopping = false;
            InitialRoundStarted = false;
            readerThread = new Thread(new ThreadStart(() => InputThread.Write(this)));
            printerThread = new Thread(new ThreadStart(() => OutputThread.Read(this)));

            // Register all features 
            RegisterFeatures();
            // Load config 
            serverConfig = new Config(ServerDir + Path.DirectorySeparatorChar + ConfigKey + Path.DirectorySeparatorChar + "config.txt");
            // Init features
            InitFeatures();
            // Start the server and threads
            if (StartServer())
            {
                readerThread.Start();
                printerThread.Start();
                MainLoop();
            }
        }


        private void RegisterFeatures()
        {
            RegisterFeature(new Autoscale(this));
            RegisterFeature(new ChainStart(this));
            RegisterFeature(new ConfigReload(this));
            RegisterFeature(new ExitCommand(this));
            //RegisterFeature(new EventTest(this));
            RegisterFeature(new GithubGenerator(this));
            RegisterFeature(new GithubLogSubmitter(this));
            RegisterFeature(new HelpCommand(this));
            RegisterFeature(new InactivityShutdown(this));
            RegisterFeature(new MemoryChecker(this));
            RegisterFeature(new MemoryCheckerSoft(this));
            RegisterFeature(new MultiAdminInfo(this));
            RegisterFeature(new NewCommand(this));
            RegisterFeature(new RestartNextRound(this));
            RegisterFeature(new RestartRoundCounter(this));
            RegisterFeature(new StopNextRound(this));
            RegisterFeature(new Titlebar(this));
        }

        private void InitFeatures()
        {
            foreach (Feature feature in Features)
            {
                feature.Init();
                feature.OnConfigReload();
            }
        }

        public void MainLoop()
        {
            while (!stopping)
            {
                if (gameProcess != null && !gameProcess.HasExited)
                {
                    foreach (IEventTick tickEvent in tick)
                    {
                        tickEvent.OnTick();
                    }
                }
                else if (!stopping)
                {
                    foreach (Feature f in Features)
                    {
                        if (f is IEventCrash)
                        {
                            ((IEventCrash)f).OnCrash();
                        }
                    }
                    Write("Game engine exited/crashed", ConsoleColor.Red);
                    Write("Removing Session", ConsoleColor.Red);
                    CleanSession();
                    Write("Restarting game with same session id");
                    StartServer();
                    InitFeatures();

                }

                Thread.Sleep(1000);
            }
            Thread.Sleep(100);
            CleanUp();
        }

        public Boolean IsStopping()
        {
            return stopping;
        }

        public void RegisterFeature(Feature feature)
        {
            if (feature is IEventTick) tick.Add((IEventTick) feature);
            if (feature is ICommand)
            {
                ICommand command = (ICommand)feature;
                Commands.Add(command.GetCommand().ToLower().Trim(), command);
            }
            Features.Add(feature);
        }



        public void PrepareFiles()
        {
            try
            {
                Directory.CreateDirectory("SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" + Path.DirectorySeparatorChar + session_id);
                Write("Started new session.", ConsoleColor.DarkGreen);
            }
            catch
            {
                Write("Failed - Please close all open files in SCPSL_Data/Dedicated and restart the server!", ConsoleColor.Red);
                Write("Press any key to close...", ConsoleColor.DarkGray);
                Console.ReadKey(true);
                Process.GetCurrentProcess().Kill();
            }

        }

        public void Write(String message, ConsoleColor color = ConsoleColor.Yellow, int height = 0)
        {
            Log(message);
            Thread.Sleep(50);
            Console.CursorTop += height;
            Console.ForegroundColor = color;
            DateTime now = DateTime.Now;
            string str = "[" + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + "] ";
            Console.WriteLine(message == "" ? "" : str + message);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public void Log(String message)
        {
            using (StreamWriter sw = File.AppendText(this.maLogLocation))
            {
                DateTime now = DateTime.Now;
                string date = "[" + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + "] ";
                sw.WriteLine(date + message);
            }
        }

        public void SoftRestartServer()
        {
            gameProcess.Kill();
        }

        public void RestartServer()
        {
            gameProcess.Kill();
            Process.Start(Directory.GetFiles(Directory.GetCurrentDirectory(), "MultiAdmin.*")[0], ConfigKey);
            stopping = true;
        }

        public void StopServer(bool killGame = true)
        {
            foreach (Feature f in Features)
            {
                if (f is IEventServerStop)
                {
                    ((IEventServerStop)f).OnServerStop();
                }
            }

            if (killGame) gameProcess.Kill();
            stopping = true;
        }

        public void CleanUp()
        {
            RemoveRunFile();
            DeleteSession();
        }


        public Boolean StartServer()
        {
            Boolean started = false;
            InitialRoundStarted = false;
            try
            {
                PrepareFiles();
                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "SCPSL.*", SearchOption.TopDirectoryOnly);
                Write("Executing: " + files[0], ConsoleColor.DarkGreen);
                SwapConfigs();
                string args = "-batchmode -nographics -key" + session_id  + " -silent-crashes -id" + (object)Process.GetCurrentProcess().Id + " -logFile \"" + logFolder + Utils.GetDate() + "_SCP_output_log.txt" + "\"";
                Write("Starting server with the following parameters");
                Write(files[0] + " " + args);
                gameProcess = Process.Start(files[0], args);
                CreateRunFile();
                started = true;
                foreach (Feature f in Features)
                {
                    if (f is IEventServerPreStart)
                    {
                        ((IEventServerPreStart)f).OnServerPreStart();
                    }
                }
            }
            catch (Exception e)
            {
                Write("Failed - Executable file not found or config issue!", ConsoleColor.Red);
                Write(e.Message, ConsoleColor.Red);
                Write("Press any key to close...", ConsoleColor.DarkGray);
                RemoveRunFile();
                Console.ReadKey(true);
                Process.GetCurrentProcess().Kill();
            }

            return started;
        }

        public Process GetGameProccess()
        {
            return gameProcess;
        }

        public void SwapConfigs()
        {
            if (File.Exists("servers" + Path.DirectorySeparatorChar + ConfigKey + Path.DirectorySeparatorChar + "config.txt"))
            {
                var contents = File.ReadAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "servers" + Path.DirectorySeparatorChar + ConfigKey + Path.DirectorySeparatorChar + "config.txt");
                File.WriteAllText(MainConfigLocation, contents);
                Write("Config file swapped", ConsoleColor.DarkYellow);
            }
            else
            {
                Write("Config file for server " + ConfigKey + " does not exist! expected location:" + "servers\\" + ConfigKey + "\\config.txt", ConsoleColor.DarkYellow);
                throw new FileNotFoundException("config file not found");
            }

        }

        private void RemoveRunFile()
        {
            File.Delete(ServerDir + Path.DirectorySeparatorChar + ConfigKey + Path.DirectorySeparatorChar + "running");
        }

        private void CreateRunFile()
        {
            File.Create(ServerDir + Path.DirectorySeparatorChar + ConfigKey + Path.DirectorySeparatorChar + "running").Close();
        }

        private void CleanSession()
        {
            foreach (String file in Directory.GetFiles("SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" + Path.DirectorySeparatorChar + session_id))
            {
                File.Delete(file);
            }
        }

        private void DeleteSession()
        {
            CleanSession();
            Directory.Delete("SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" + Path.DirectorySeparatorChar + session_id);
        }


        public String GetSessionId()
        {
            return session_id;
        }

        public Boolean IsConfigRunning(String config)
        {
            return File.Exists(ServerDir + Path.DirectorySeparatorChar + config + Path.DirectorySeparatorChar + "running");
        }

        public void NewInstance(String configChain)
        {
            String file = Directory.GetFiles(Directory.GetCurrentDirectory(), "MultiAdmin.*")[0];
            ProcessStartInfo psi = new ProcessStartInfo(file, configChain);
            Process.Start(psi);
        }

        public void SendMessage(string message)
        {
            StreamWriter streamWriter = new StreamWriter("SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" + Path.DirectorySeparatorChar + session_id + Path.DirectorySeparatorChar + "cs" + logID + ".mapi");
            logID++;
            streamWriter.WriteLine(message + "terminator");
            streamWriter.Close();
            Write("Sending request to SCP: Secret Laboratory...", ConsoleColor.White);
        }

    }
}
