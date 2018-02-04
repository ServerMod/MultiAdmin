using System;
using System.IO;
using System.Threading;
using MultiAdmin.MultiAdmin;

namespace MultiAdmin
{
    class OutputThread
    {
        public static void Read(Server server)
        {
            while (!server.IsStopping())
            {
                string[] strArray = null;
                try
                {
                    strArray = Directory.GetFiles("SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" + Path.DirectorySeparatorChar + server.GetSessionId(), "sl*.mapi", SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    server.Write("Message printer warning: 'SCPSL_Data/Dedicated' directory not found.", ConsoleColor.Yellow);
                    server.Write("Press any key to ignore...", ConsoleColor.DarkGray);
                    Console.ReadKey();
                }

                foreach (string path in strArray)
                {

                    string gameMessage = "";
                    string fileCommand = "open";
                    try
                    {
                        StreamReader streamReader = new StreamReader(path);
                        gameMessage = streamReader.ReadToEnd();
                        fileCommand = "close";
                        streamReader.Close();
                        fileCommand = "delete";
                        File.Delete(path);
                    }
                    catch
                    {
                        server.Write("Message printer warning: Could not " + fileCommand + " file " + path + ". Make sure that MultiAdmin.exe has all necessary read-write permissions.", ConsoleColor.Yellow);
                        server.Write("Press any key to ignore...", ConsoleColor.DarkGray);
                        Console.ReadKey();
                    }

                    if (!string.IsNullOrEmpty(gameMessage.Trim()))
                    {
                        ConsoleColor colour = ConsoleColor.Cyan;
                        if (gameMessage.Contains("LOGTYPE"))
                        {
                            String type = gameMessage.Substring(gameMessage.IndexOf("LOGTYPE")).Trim();
                            gameMessage = gameMessage.Substring(0, gameMessage.IndexOf("LOGTYPE")).Trim();

                            switch(type)
                            {
                                case "LOGTYPE02":
                                    colour = ConsoleColor.Green;
                                    break;
                                case "LOGTYPE-8":
                                    colour = ConsoleColor.DarkRed;
                                    break;
                                case "LOGTYPE14":
                                    colour = ConsoleColor.White;
                                    break;
                                default:
                                    colour = ConsoleColor.Cyan;
                                    break;
                            }
                        }
                        server.Write(gameMessage.Trim(), colour);
                    }

                    if (gameMessage.Contains("ServerMod"))
                    {
                        server.HasServerMod = true;
                    }

                    if (gameMessage.Contains("Waiting for players"))
                    {
                        if (!server.InitialRoundStarted)
                        {
                            server.InitialRoundStarted = true;
                        }
                        else
                        {
                            foreach (Feature f in server.Features)
                            {
                                if (f is IEventRoundEnd)
                                {
                                    ((IEventRoundEnd)f).OnRoundEnd();
                                }
                            }
                        }

                    }

                    if (gameMessage.Contains("New round has been started"))
                    {

                        foreach (Feature f in server.Features)
                        {
                            if (f is IEventRoundStart)
                            {
                                ((IEventRoundStart)f).OnRoundStart();
                            }
                        }
                    }

                    if (gameMessage.Contains("Server starting at port"))
                    {
                        foreach (Feature f in server.Features)
                        {
                            if (f is IEventServerStart)
                            {
                                ((IEventServerStart)f).OnServerStart();
                            }
                        }
                    }


                    if (gameMessage.Contains("Server full"))
                    {
                        foreach (Feature f in server.Features)
                        {
                            if (f is IEventServerFull)
                            {
                                ((IEventServerFull)f).OnServerFull();
                            }
                        }
                    }


                    if (gameMessage.Contains("Player connect"))
                    {
                        foreach (Feature f in server.Features)
                        {
                            if (f is IEventPlayerConnect)
                            {
                                String name = gameMessage.Substring(gameMessage.IndexOf(":"));
                                ((IEventPlayerConnect)f).OnPlayerConnect(name);
                            }
                        }
                    }

                    if (gameMessage.Contains("Player disconnect"))
                    {
                        foreach (Feature f in server.Features)
                        {
                            if (f is IEventPlayerDisconnect)
                            {
                                String name = gameMessage.Substring(gameMessage.IndexOf(":"));
                                ((IEventPlayerDisconnect)f).OnPlayerDisconnect(name);
                            }
                        }
                    }

                }
                Thread.Sleep(300);
            }
            Console.WriteLine("READ THREAD DEAD");
        }
    }
}
