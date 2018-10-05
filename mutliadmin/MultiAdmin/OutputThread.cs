using System;//why is a bullet here?
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MultiAdmin.MultiAdmin;

namespace MultiAdmin
{
	class OutputThread
	{
		public static readonly Regex SMOD_REGEX = new Regex(@"\[(DEBUG|INFO|WARN|ERROR)\] (\[.*?\]) (.*)", RegexOptions.Compiled);
		public static readonly ConsoleColor DEFAULT_FOREGROUND = ConsoleColor.Cyan;
		public static readonly ConsoleColor DEFAULT_BACKGROUND = ConsoleColor.Black;


		public static ConsoleColor MapConsoleColor(string color, ConsoleColor def = ConsoleColor.Cyan)
		{
			try
			{
				return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color);
			}
			catch
			{
				return def;
			}
		}

		public static void Read(Server server)
		{
			while (!server.IsStopping())
			{
				string dedicatedDir = "SCPSL_Data" + Path.DirectorySeparatorChar + "Dedicated" + Path.DirectorySeparatorChar + server.GetSessionId();
				string[] activeDir = new string[] {};

				try
				{
					if (Directory.Exists(dedicatedDir))
					{
						activeDir = Directory.GetFiles(dedicatedDir, "sl*.mapi", SearchOption.TopDirectoryOnly).OrderBy(f => f).ToArray<string>();
					}
				}
				catch
				{
					if (!server.IsStopping())
					{
						server.Write("Message printer warning: 'SCPSL_Data/Dedicated' directory not found.", ConsoleColor.Yellow);
					}
				}

				if (activeDir.Length == 0)
				{
					continue;
				}
				
				foreach (string file in activeDir)
				{
					string stream = string.Empty;
					string command = "open";
					int attempts = 0;
					bool read = false;

					while (attempts < (server.runOptimized ? 10 : 100) && !read && !server.IsStopping())
					{
						try
						{
							if (!File.Exists(file))
							{
								server.Write("Message printer warning: Could not " + command + " " + file + ". File does not exist!", ConsoleColor.Yellow);
								server.Write("skipping");
								break;
							}

							StreamReader sr = new StreamReader(file);
							stream = sr.ReadToEnd();
							command = "close";
							sr.Close();
							command = "delete";
							File.Delete(file);
							read = true;
						}
						catch
						{
							attempts++;
							if (attempts >= (server.runOptimized ? 10 : 100))
							{
								server.Write("Message printer warning: Could not " + command + " " + file + ". Make sure that MultiAdmin.exe has all necessary read-write permissions.", ConsoleColor.Yellow);
								server.Write("skipping");
							}
						}
						Thread.Sleep(server.printSpeed);
					}

					if (server.IsStopping()) break;

					bool display = true;
					ConsoleColor color = ConsoleColor.Cyan;

					if (!string.IsNullOrEmpty(stream.Trim()))
					{
						if (stream.Contains("LOGTYPE"))
						{
							string type = stream.Substring(stream.IndexOf("LOGTYPE")).Trim();
							stream = stream.Substring(0, stream.IndexOf("LOGTYPE")).Trim();

							switch (type)
							{
								case "LOGTYPE02":
									color = ConsoleColor.Green;
									break;
								case "LOGTYPE-8":
									color = ConsoleColor.DarkRed;
									break;
								case "LOGTYPE14":
									color = ConsoleColor.Magenta;
									break;
								default:
									color = ConsoleColor.Cyan;
									break;
							}
						}

					}

					// Smod3 Color tags

					string[] streamSplit = stream.Split("@#".ToCharArray());

					if (streamSplit.Length > 1)
					{
						ConsoleColor fg = DEFAULT_FOREGROUND;
						ConsoleColor bg = DEFAULT_BACKGROUND;
						// date
						server.WritePart(string.Empty, DEFAULT_BACKGROUND, ConsoleColor.Cyan, true, false);

						foreach (string line in streamSplit)
						{
							string part = line;
							if (part.Length >= 3 && part.Contains(";"))
							{
								string colorTag = part.Substring(3, part.IndexOf(";") - 3);

								if (part.Substring(0, 3).Equals("fg="))
								{
									fg = MapConsoleColor(colorTag, DEFAULT_FOREGROUND);
								}

								if (line.Substring(0, 3).Equals("bg="))
								{
									bg = MapConsoleColor(colorTag, DEFAULT_BACKGROUND);
								}

								if (part.Length == line.IndexOf(";"))
								{
									part = string.Empty;
								}
								else
								{
									part = part.Substring(line.IndexOf(";") + 1);
								}

							}

							server.WritePart(part, bg, fg, false, false);
						}
						// end
						server.WritePart(string.Empty, DEFAULT_BACKGROUND, ConsoleColor.Cyan, false, true);
						display = false;
					}

					// Smod2 loggers pretty printing

					var match = SMOD_REGEX.Match(stream);
					if (match.Success)
					{
						if (match.Groups.Count >= 2)
						{
							ConsoleColor levelColor = ConsoleColor.Cyan;
							ConsoleColor tagColor = ConsoleColor.Yellow;
							ConsoleColor msgColor = ConsoleColor.White;
							switch (match.Groups[1].Value.Trim())
							{
								case "[DEBUG]":
									levelColor = ConsoleColor.Gray;
									break;
								case "[INFO]":
									levelColor = ConsoleColor.Green;
									break;
								case "[WARN]":
									levelColor = ConsoleColor.DarkYellow;
									break;
								case "[ERROR]":
									levelColor = ConsoleColor.Red;
									msgColor = ConsoleColor.Red;
									break;
								default:
									color = ConsoleColor.Cyan;
									break;
							}
							server.WritePart(string.Empty, DEFAULT_BACKGROUND, ConsoleColor.Cyan, true, false);
							server.WritePart("[" + match.Groups[1].Value + "] ", DEFAULT_BACKGROUND, levelColor, false, false);
							server.WritePart(match.Groups[2].Value + " ", DEFAULT_BACKGROUND, tagColor, false, false);
							// OLD: server.WritePart(match.Groups[3].Value, msgColor, 0, false, true);
							// The regex.Match was trimming out the new lines and that is why no new lines were created.
							// To be sure this will not happen again:
							streamSplit = stream.Split(new char[] { ']' }, 3);
							server.WritePart(streamSplit[2], DEFAULT_BACKGROUND, msgColor, false, true);
							// This way, it outputs the whole message.
							// P.S. the format is [Info] [courtney.exampleplugin] Something intresting happened
							// That was just an example
							display = false;
						}

					}


					if (stream.Contains("Mod Log:"))
					{
						foreach (Feature f in server.Features)
						{
							if (f is IEventAdminAction)
							{
								((IEventAdminAction)f).OnAdminAction(stream.Replace("Mod log:", string.Empty));
							}
						}
					}

					if (stream.Contains("ServerMod - Version"))
					{
						server.HasServerMod = true;
						// This should work fine with older ServerMod versions too
						streamSplit = stream.Replace("ServerMod - Version", string.Empty).Split('-');
						server.ServerModVersion = streamSplit[0].Trim();
						server.ServerModBuild = (streamSplit.Length > 1 ? streamSplit[1] : "A").Trim();
					}

					if (server.ServerModCheck(1, 7, 2))
					{
						if (stream.Contains("Round restarting"))
						{
							foreach (Feature f in server.Features)
							{
								if (f is IEventRoundEnd)
								{
									((IEventRoundEnd)f).OnRoundEnd();
								}
							}
						}

						if (stream.Contains("Waiting for players"))
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
						if (stream.Contains("Waiting for players"))
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



					if (stream.Contains("New round has been started"))
					{

						foreach (Feature f in server.Features)
						{
							if (f is IEventRoundStart)
							{
								((IEventRoundStart)f).OnRoundStart();
							}
						}
					}

					if (stream.Contains("Level loaded. Creating match..."))
					{
						foreach (Feature f in server.Features)
						{
							if (f is IEventServerStart)
							{
								((IEventServerStart)f).OnServerStart();
							}
						}
					}


					if (stream.Contains("Server full"))
					{
						foreach (Feature f in server.Features)
						{
							if (f is IEventServerFull)
							{
								((IEventServerFull)f).OnServerFull();
							}
						}
					}


					if (stream.Contains("Player connect"))
					{
						display = false;
						server.Log("Player connect event");
						foreach (Feature f in server.Features)
						{
							if (f is IEventPlayerConnect)
							{
								string name = stream.Substring(stream.IndexOf(":"));
								((IEventPlayerConnect)f).OnPlayerConnect(name);
							}
						}
					}

					if (stream.Contains("Player disconnect"))
					{
						display = false;
						server.Log("Player disconnect event");
						foreach (Feature f in server.Features)
						{
							if (f is IEventPlayerDisconnect)
							{
								string name = stream.Substring(stream.IndexOf(":"));
								((IEventPlayerDisconnect)f).OnPlayerDisconnect(name);
							}
						}
					}

					if (stream.Contains("Player has connected before load is complete"))
					{
						if (server.ServerModCheck(1, 5, 0))
						{
							server.fixBuggedPlayers = true;
						}
					}

					if (display) server.Write(stream.Trim(), color);
				}

				Thread.Sleep(server.runOptimized ? 5 : 10);
			}

		}
	}
}
