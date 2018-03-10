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
				String dir = "SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" + Path.DirectorySeparatorChar + server.GetSessionId();

				try
                {
					if (Directory.Exists(dir))
					{
						strArray = Directory.GetFiles(dir, "sl*.mapi", SearchOption.TopDirectoryOnly);
					}
                }
                catch
                {
					if (!server.IsStopping())
					{
						server.Write("Message printer warning: 'SCPSL_Data/Dedicated' directory not found.", ConsoleColor.Yellow);
					}                    
                }

                if (strArray == null) continue;
                foreach (string path in strArray)
                {

                    string gameMessage = "";
                    string fileCommand = "open";
                    int attempts = 0;
                    Boolean read = false;

                    while (attempts < 100 && !read && !server.IsStopping())
                    {
                        try
                        {
                            StreamReader streamReader = new StreamReader(path);
                            gameMessage = streamReader.ReadToEnd();
                            fileCommand = "close";
                            streamReader.Close();
                            fileCommand = "delete";
                            File.Delete(path);
                            read = true;
                        }
                        catch
                        {
                            attempts++;
							if (attempts >= 100)
							{
								server.Write("Message printer warning: Could not " + fileCommand + " file " + path + ". Make sure that MultiAdmin.exe has all necessary read-write permissions.", ConsoleColor.Yellow);
								server.Write("skipping");
							}
                        }
                        Thread.Sleep(300);
                    }

                    if (server.IsStopping()) break;

                    Boolean display = true;
                    ConsoleColor colour = ConsoleColor.Cyan;

                    if (!string.IsNullOrEmpty(gameMessage.Trim()))
                    {
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
									colour = ConsoleColor.Magenta;
                                    break;
                                default:
                                    colour = ConsoleColor.Cyan;
                                    break;
                            }
                        }
                       
                    }

					if (gameMessage.Contains("Mod Log:"))
					{
						foreach (Feature f in server.Features)
						{
							if (f is IEventAdminAction)
							{
								((IEventAdminAction)f).OnAdminAction(gameMessage.Replace("Mod log:", ""));
							}
						}
					}

                    if (gameMessage.Contains("ServerMod"))
                    {
                        server.HasServerMod = true;
                        server.ServerModVersion = gameMessage.Replace("ServerMod - Version", "").Trim();
                    }

					if (server.ServerModCheck(1, 7, 2))
					{
						if (gameMessage.Contains("Round restarting"))
						{
							foreach (Feature f in server.Features)
							{
								if (f is IEventRoundEnd)
								{
									((IEventRoundEnd)f).OnRoundEnd();
								}
							}
						}

						if (gameMessage.Contains("Waiting for players"))
						{
                            if (!server.InitialRoundStarted)
							{
								server.InitialRoundStarted = true;
								foreach (Feature f in server.Features)
								{
									if (f is IEventRoundStart)
									{
										((IEventRoundStart)f).OnRoundStart();
									}
								}
							}

                            if (server.ServerModCheck(1, 5, 0) && server.fixBuggedPlayers)
                            {
                                server.SendMessage("ROUNDRESTART");
                                server.fixBuggedPlayers = false;
                            }
                        }
					}
					else
					{
						if (gameMessage.Contains("Waiting for players"))
						{
							if (!server.InitialRoundStarted)
							{
								server.InitialRoundStarted = true;
								foreach (Feature f in server.Features)
								{
									if (f is IEventRoundStart)
									{
										((IEventRoundStart)f).OnRoundStart();
									}
								}
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

                            if (server.ServerModCheck(1, 5, 0) && server.fixBuggedPlayers)
                            {
                                server.SendMessage("ROUNDRESTART");
                                server.fixBuggedPlayers = false;
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

                    if (gameMessage.Contains("Level loaded. Creating match..."))
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
                        display = false;
                        server.Log("Player connect event");
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
                        display = false;
                        server.Log("Player disconnect event");
                        foreach (Feature f in server.Features)
                        {
                            if (f is IEventPlayerDisconnect)
                            {
                                String name = gameMessage.Substring(gameMessage.IndexOf(":"));
                                ((IEventPlayerDisconnect)f).OnPlayerDisconnect(name);
                            }
                        }
                    }

                    if (gameMessage.Contains("Player has connected before load is complete"))
                    {
                        if (server.ServerModCheck(1, 5, 0))
                        {
                            server.fixBuggedPlayers = true;
                        }
                    }

                    if (display) server.Write(gameMessage.Trim(), colour);
                }

                Thread.Sleep(10);
            }

        }
    }
}
