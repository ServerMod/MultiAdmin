using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MultiAdmin.MultiAdmin;

namespace MultiAdmin
{
	class OutputThread
	{
		public static readonly Regex smodRegex = new Regex(@"\[(DEBUG|INFO|WARN|ERROR)\] (\[.*?\]) (.*)", RegexOptions.Compiled);

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
						strArray = Directory.GetFiles(dir, "sl*.mapi", SearchOption.TopDirectoryOnly).OrderBy(f => f).ToArray<String>();
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

					while (attempts < (server.runOptimized ? 10 : 100) && !read && !server.IsStopping())
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
							if (attempts >= (server.runOptimized ? 10 : 100))
							{
								server.Write("Message printer warning: Could not " + fileCommand + " file " + path + ". Make sure that MultiAdmin.exe has all necessary read-write permissions.", ConsoleColor.Yellow);
								server.Write("skipping");
							}
						}
						Thread.Sleep(server.printSpeed);
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

							switch (type)
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

					// Smod2 loggers pretty printing


					var match = smodRegex.Match(gameMessage);
					if (match.Success)
					{
						if (match.Groups.Count >= 2)
						{
							ConsoleColor levelColour = ConsoleColor.Cyan;
							ConsoleColor tagColour = ConsoleColor.Yellow;
							ConsoleColor msgColour = ConsoleColor.White;
							switch (match.Groups[1].Value.Trim())
							{
								case "[DEBUG]":
									levelColour = ConsoleColor.Gray;
									break;
								case "[INFO]":
									levelColour = ConsoleColor.Green;
									break;
								case "[WARN]":
									levelColour = ConsoleColor.DarkYellow;
									break;
								case "[ERROR]":
									levelColour = ConsoleColor.Red;
									msgColour = ConsoleColor.Red;
									break;
								default:
									colour = ConsoleColor.Cyan;
									break;
							}
							server.WritePart("", ConsoleColor.Cyan, 0, true, false);
							server.WritePart("[" + match.Groups[1].Value + "] ", levelColour, 0, false, false);
							server.WritePart(match.Groups[2].Value + " ", tagColour, 0, false, false);
							// OLD: server.WritePart(match.Groups[3].Value, msgColour, 0, false, true);
							// The regex.Match was trimming out the new lines and that is why no new lines were created.
							// To be sure this will not happen again:
							server.WritePart(gameMessage.Split(new char[] { ']' }, 3)[2], msgColour, 0, false, true);
							// This way, it outputs the whole message.
							// P.S. the format is [Info] [courtney.exampleplugin] Something intresting happened
							// That was just an example
							display = false;
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

				Thread.Sleep(server.runOptimized ? 5 : 10);
			}

		}
	}
}
