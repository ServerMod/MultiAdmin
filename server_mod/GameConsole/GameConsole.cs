using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameConsole
{
	// Token: 0x02000016 RID: 22
	public class Console : MonoBehaviour
	{
		// Token: 0x06000063 RID: 99
		public Console()
		{
		}

		// Token: 0x06000064 RID: 100
		public List<Console.Log> GetAllLogs()
		{
			return this.logs;
		}

		// Token: 0x06000065 RID: 101
		public void UpdateValue(string key, string value)
		{
			bool flag = false;
			key = key.ToUpper();
			foreach (Console.Value value2 in this.values)
			{
				if (value2.key == key)
				{
					value2.value = value;
					flag = true;
				}
			}
			if (!flag)
			{
				this.values.Add(new Console.Value(key, value));
			}
		}

		// Token: 0x06000066 RID: 102
		public void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			if (Console.singleton == null)
			{
				Console.singleton = this;
				return;
			}
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}

		// Token: 0x06000067 RID: 103
		public void Start()
		{
			this.AddLog("Hi there! Initializing console...", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
			this.AddLog("Done! Type 'help' to print the list of available commands.", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
			Debug.logger.logEnabled = true;
			this.RefreshConsoleScreen();
		}

		// Token: 0x06000068 RID: 104
		public void RefreshConsoleScreen()
		{
			bool flag = false;
			if (this.txt.text.Length > 15000)
			{
				this.logs.RemoveAt(0);
				flag = true;
			}
			if (this.txt == null)
			{
				return;
			}
			this.txt.text = string.Empty;
			if (this.logs.Count > 0)
			{
				for (int i = 0; i < this.logs.Count - this.scrollup; i++)
				{
					string text = string.Concat(new string[]
					{
						(!this.logs[i].nospace) ? "\n\n" : "\n",
						"<color=",
						this.ColorToHex(this.logs[i].color),
						">",
						this.logs[i].text,
						"</color>"
					});
					if (text.Contains("@#{["))
					{
						string str = text.Remove(text.IndexOf("@#{["));
						string text2 = text.Remove(0, text.IndexOf("@#{[") + 4);
						text2 = text2.Remove(text2.Length - 12);
						foreach (Console.Value value in this.values)
						{
							if (value.key == text2)
							{
								text = str + value.value + "</color>";
							}
						}
					}
					Text text3 = this.txt;
					text3.text += text;
				}
			}
			if (flag)
			{
				this.RefreshConsoleScreen();
			}
		}

		// Token: 0x06000069 RID: 105
		public void AddLog(string text, Color32 c, bool nospace = false)
		{
			this.response = this.response + text + Environment.NewLine;
			if (!nospace)
			{
				this.response += Environment.NewLine;
			}
			this.scrollup = 0;
			this.logs.Add(new Console.Log(text, c, nospace));
			this.RefreshConsoleScreen();
		}

		// Token: 0x0600006A RID: 106
		public string ColorToHex(Color32 color)
		{
			string str = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
			return "#" + str;
		}

		// Token: 0x0600006B RID: 107
		public static GameObject FindConnectedRoot(NetworkConnection conn)
		{
			try
			{
				foreach (PlayerController playerController in conn.playerControllers)
				{
					if (playerController.gameObject.tag == "Player")
					{
						return playerController.gameObject;
					}
				}
			}
			catch
			{
			}
			return null;
		}

		// Token: 0x0600006C RID: 108
		public string TypeCommand(string cmd)
		{
			try
			{
				if (!GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					this.AddLog("Console commands are disabled for the clients.", new Color32(byte.MaxValue, 0, 0, byte.MaxValue), false);
					return "not owner";
				}
			}
			catch
			{
				return "not owner";
			}
			this.response = string.Empty;
			string[] array = cmd.ToUpper().Split(new char[]
			{
				' '
			});
			cmd = array[0];
			if (cmd == "HELLO")
			{
				this.AddLog("Hello World!", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
			}
			else if (cmd == "LENNY")
			{
				this.AddLog("<size=450>( ͡° ͜ʖ ͡°)</size>\n\n", new Color32(byte.MaxValue, 180, 180, byte.MaxValue), false);
			}
			else if (cmd == "GIVE")
			{
				int num = 0;
				if (array.Length >= 2 && int.TryParse(array[1], out num))
				{
					string a = "offline";
					foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Player"))
					{
						if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
						{
							a = "online";
							Inventory component = gameObject.GetComponent<Inventory>();
							if (component != null)
							{
								if (component.availableItems.Length > num)
								{
									component.AddItem(num, -4.65664672E+11f);
									a = "none";
								}
								else
								{
									this.AddLog("Failed to add ITEM#" + num.ToString("000") + " - item does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
								}
							}
						}
					}
					if (a == "offline" || a == "online")
					{
						this.AddLog((!(a == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
					}
					else
					{
						this.AddLog("ITEM#" + num.ToString("000") + " has been added!", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
					}
				}
				else
				{
					this.AddLog("Second argument has to be a number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
			}
			else if (cmd == "ITEMLIST")
			{
				string a2 = "offline";
				foreach (GameObject gameObject2 in GameObject.FindGameObjectsWithTag("Player"))
				{
					int num2 = 1;
					if (array.Length >= 2 && !int.TryParse(array[1], out num2))
					{
						this.AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
						return this.response;
					}
					if (gameObject2.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						a2 = "online";
						Inventory component2 = gameObject2.GetComponent<Inventory>();
						if (component2 != null)
						{
							a2 = "none";
							if (num2 < 1)
							{
								this.AddLog("Page '" + num2 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
								this.RefreshConsoleScreen();
								return this.response;
							}
							Item[] availableItems = component2.availableItems;
							for (int i = 10 * (num2 - 1); i < 10 * num2; i++)
							{
								if (10 * (num2 - 1) > availableItems.Length)
								{
									this.AddLog("Page '" + num2 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
									break;
								}
								if (i >= availableItems.Length)
								{
									break;
								}
								this.AddLog("ITEM#" + i.ToString("000") + " : " + availableItems[i].label, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
							}
						}
					}
				}
				if (a2 != "none")
				{
					this.AddLog((!(a2 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
			}
			else if (cmd == "CLS" || cmd == "CLEAR")
			{
				this.logs.Clear();
				this.RefreshConsoleScreen();
			}
			else if (cmd == "QUIT" || cmd == "EXIT")
			{
				this.logs.Clear();
				this.RefreshConsoleScreen();
				this.AddLog("<size=50>GOODBYE!</size>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
				this.RefreshConsoleScreen();
				base.Invoke("QuitGame", 1f);
			}
			else if (cmd == "HELP")
			{
				if (array.Length > 1)
				{
					string b = array[1];
					foreach (Console.CommandHint commandHint in this.hints)
					{
						if (commandHint.name == b)
						{
							this.AddLog(commandHint.name + " - " + commandHint.fullDesc + "LOGTYPE02", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
							this.RefreshConsoleScreen();
							return this.response;
						}
					}
					this.AddLog("Help for command '" + array[1] + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
					this.RefreshConsoleScreen();
					return this.response;
				}
				this.AddLog("List of available commands:LOGTYPE02", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
				foreach (Console.CommandHint commandHint2 in this.hints)
				{
					this.AddLog(commandHint2.name + " - " + commandHint2.shortDesc + "LOGTYPE02", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), true);
				}
				this.AddLog("Type 'HELP [COMMAND]' to print a full description of the chosen command.", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
				this.RefreshConsoleScreen();
			}
			else if (cmd == "REFRESHFIX")
			{
				this.allwaysRefreshing = !this.allwaysRefreshing;
				this.AddLog("Console log refresh mode: " + ((!this.allwaysRefreshing) ? "OPTIMIZED" : "FIXED"), new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
			}
			else if (cmd == "VALUE")
			{
				if (array.Length < 2)
				{
					this.AddLog("The second argument cannot be <i>null</i>!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
				else
				{
					bool flag = false;
					string text = array[1];
					using (List<Console.Value>.Enumerator enumerator = this.values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.key == text)
							{
								flag = true;
								this.AddLog(string.Concat(new string[]
								{
									"The value of ",
									text,
									" is: @#{[",
									text,
									"}]#@"
								}), new Color32(50, 70, 100, byte.MaxValue), false);
							}
						}
					}
					if (!flag)
					{
						this.AddLog("Key " + text + " not found!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
					}
				}
			}
			else if (cmd == "SEED")
			{
				GameObject gameObject3 = GameObject.Find("Host");
				int num3 = -1;
				if (gameObject3 != null)
				{
					num3 = gameObject3.GetComponent<RandomSeedSync>().seed;
				}
				this.AddLog("Map seed is: <b>" + ((num3 != -1) ? num3.ToString() : "NONE") + "</b>", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
			}
			else if (cmd == "SHOWRIDS")
			{
				GameObject[] array2 = GameObject.FindGameObjectsWithTag("RoomID");
				foreach (GameObject gameObject4 in array2)
				{
					gameObject4.GetComponentsInChildren<MeshRenderer>()[0].enabled = !gameObject4.GetComponentsInChildren<MeshRenderer>()[0].enabled;
					gameObject4.GetComponentsInChildren<MeshRenderer>()[1].enabled = !gameObject4.GetComponentsInChildren<MeshRenderer>()[1].enabled;
				}
				if (array2.Length != 0)
				{
					this.AddLog("Show RIDS: " + array2[0].GetComponentInChildren<MeshRenderer>().enabled.ToString(), new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
				}
				else
				{
					this.AddLog("There are no RIDS!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
			}
			else if (cmd == "CLASSLIST")
			{
				string a3 = "offline";
				foreach (GameObject gameObject5 in GameObject.FindGameObjectsWithTag("Player"))
				{
					int num4 = 1;
					if (array.Length >= 2 && !int.TryParse(array[1], out num4))
					{
						this.AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
						return this.response;
					}
					if (gameObject5.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						a3 = "online";
						CharacterClassManager component3 = gameObject5.GetComponent<CharacterClassManager>();
						if (component3 != null)
						{
							a3 = "none";
							if (num4 < 1)
							{
								this.AddLog("Page '" + num4 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
								this.RefreshConsoleScreen();
								return this.response;
							}
							Class[] klasy = component3.klasy;
							for (int j = 10 * (num4 - 1); j <= 10 * num4; j++)
							{
								if (10 * (num4 - 1) > klasy.Length)
								{
									this.AddLog("Page '" + num4 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
									break;
								}
								if (j >= klasy.Length)
								{
									break;
								}
								this.AddLog(string.Concat(new object[]
								{
									"CLASS#",
									j.ToString("000"),
									" : ",
									klasy[j].fullName,
									" (Max HP: ",
									klasy[j].maxHP,
									")"
								}), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
							}
						}
					}
				}
				if (a3 != "none")
				{
					this.AddLog((!(a3 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
			}
			else if (cmd == "GOTO")
			{
				if (array.Length >= 2)
				{
					GameObject gameObject6 = null;
					foreach (GameObject gameObject7 in GameObject.FindGameObjectsWithTag("RoomID"))
					{
						if (gameObject7.GetComponent<Rid>().id.ToUpper() == array[1].ToUpper())
						{
							gameObject6 = gameObject7;
						}
					}
					string a4 = "offline";
					if (gameObject6 != null)
					{
						foreach (GameObject gameObject8 in GameObject.FindGameObjectsWithTag("Player"))
						{
							if (gameObject8.GetComponent<NetworkIdentity>().isLocalPlayer)
							{
								if (array[1].ToUpper() == "RANGE" && !gameObject8.GetComponent<ShootingRange>().isOnRange)
								{
									a4 = "range";
								}
								else
								{
									a4 = "none";
									gameObject8.transform.position = gameObject6.transform.position;
								}
							}
						}
						if (a4 == "range")
						{
							this.AddLog("<b>Shooting range is disabled!</b>", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
						}
						else if (a4 == "offline")
						{
							this.AddLog("You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
						}
						else
						{
							this.AddLog("Teleported!", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
						}
					}
					else
					{
						this.AddLog("Room: <i>" + array[1].ToUpper() + "</i> not found!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
					}
				}
				else
				{
					this.AddLog("Second argument is missing!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
				this.RefreshConsoleScreen();
			}
			else if (cmd == "RANGE")
			{
				string a5 = "offline";
				foreach (GameObject gameObject9 in GameObject.FindGameObjectsWithTag("Player"))
				{
					if (gameObject9.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						a5 = "online";
						ShootingRange component4 = gameObject9.GetComponent<ShootingRange>();
						if (component4 != null)
						{
							a5 = "none";
							component4.isOnRange = true;
						}
					}
				}
				if (a5 == "offline" || a5 == "online")
				{
					this.AddLog((!(a5 == "offline")) ? "Player range script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
				else
				{
					this.AddLog("<b>Shooting range</b> is now available!", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
				}
			}
			else if (cmd == "ROUNDRESTART")
			{
				bool flag2 = false;
				GameObject[] array3 = GameObject.FindGameObjectsWithTag("Player");
				for (int k = 0; k < array3.Length; k++)
				{
					PlayerStats component5 = array3[k].GetComponent<PlayerStats>();
					if (component5.isLocalPlayer && component5.isServer)
					{
						flag2 = true;
						this.AddLog("The round is about to restart! Please wait..", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
						component5.Roundrestart();
					}
				}
				if (!flag2)
				{
					this.AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
			}
			else if (cmd == "CONFIG")
			{
				if (array.Length < 2)
				{
					this.TypeCommand("HELP CONFIG");
				}
				else
				{
					string text2 = array[1];
					if (text2 != null)
					{
						if (!(text2 == "RELOAD") && !(text2 == "R") && !(text2 == "RLD"))
						{
							if (!(text2 == "PATH"))
							{
								if (text2 == "VALUE")
								{
									if (array.Length < 3)
									{
										this.AddLog("Please enter key name in the third argument. (CONFIG VALUE <i>KEYNAME</i>)", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
									}
									else
									{
										this.AddLog("The value of <i>'" + array[2] + "'</i> is: " + ConfigFile.GetString(array[2], "<color=ff0>DENIED: Entered key does not exists</color>"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
									}
								}
							}
							else
							{
								this.AddLog("Configuration file path: <i>" + ConfigFile.path + "</i>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
								this.AddLog("<i>No visible drive letter means the root game directory.</i>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
							}
						}
						else if (ConfigFile.singleton.ReloadConfig())
						{
							((CustomNetworkManager)NetworkManager.singleton).UpdateMotd(0);
							this.AddLog("Configuration file <b>successfully reloaded</b>. New settings will be applied on <b>your</b> server in <b>next</b> round.", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
						}
						else
						{
							this.AddLog("Configuration file reload <b>failed</b> - no such file - '<i>" + ConfigFile.path + "</i>'. Loading defult settings..", new Color32(byte.MaxValue, 0, 0, byte.MaxValue), false);
							this.AddLog("Default settings have been loaded.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
						}
					}
				}
			}
			else if (cmd == "BAN")
			{
				if (GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					if (array.Length < 3)
					{
						this.AddLog("Syntax: BAN [player kick / ip] [minutes]", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
						using (IEnumerator<NetworkConnection> enumerator2 = NetworkServer.connections.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								NetworkConnection networkConnection = enumerator2.Current;
								string text3 = string.Empty;
								GameObject gameObject10 = Console.FindConnectedRoot(networkConnection);
								if (gameObject10 != null)
								{
									text3 = gameObject10.GetComponent<NicknameSync>().myNick;
								}
								if (text3 == string.Empty)
								{
									this.AddLog("Player :: " + networkConnection.address, new Color32(160, 128, 128, byte.MaxValue), true);
								}
								else
								{
									this.AddLog("Player :: " + text3 + " :: " + networkConnection.address, new Color32(128, 160, 128, byte.MaxValue), true);
								}
							}
							goto IL_1386;
						}
					}
					int duration = 0;
					if (int.TryParse(array[2], out duration))
					{
						bool flag3 = false;
						foreach (NetworkConnection networkConnection2 in NetworkServer.connections)
						{
							GameObject gameObject11 = Console.FindConnectedRoot(networkConnection2);
							if (networkConnection2.address.ToUpper().Contains(array[1]) || (gameObject11 != null && gameObject11.GetComponent<NicknameSync>().myNick.ToUpper().Contains(array[1])))
							{
								flag3 = true;
								BanPlayer.BanConnection(networkConnection2, duration);
								this.AddLog("Player banned.", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
							}
						}
						if (!flag3)
						{
							this.AddLog("Player not found.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
						}
					}
					else
					{
						this.AddLog("Parse error: [minutes] - has to be an integer.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
					}
				}
				else
				{
					this.AddLog("You are not the owner!.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
				}
			}
			else if (cmd == "BANREFRESH")
			{
				if (GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					BanPlayer.ReloadBans();
				}
				else
				{
					this.AddLog("You are not the owner!.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
				}
			}
			else
			{
				this.AddLog("Command " + cmd + " does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
			}
			IL_1386:
			return this.response;
		}

		// Token: 0x0600006D RID: 109
		public void ProceedButton()
		{
			if (this.cmdField.text != string.Empty)
			{
				this.TypeCommand(this.cmdField.text);
			}
			this.cmdField.text = string.Empty;
			EventSystem.current.SetSelectedGameObject(this.cmdField.gameObject);
		}

		// Token: 0x0600006E RID: 110
		public void LateUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				this.ProceedButton();
			}
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				this.ToggleConsole();
			}
			this.scrollup += Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel") * 10f);
			if (this.logs.Count > 0)
			{
				this.scrollup = Mathf.Clamp(this.scrollup, 0, this.logs.Count - 1);
			}
			else
			{
				this.scrollup = 0;
			}
			if (this.previous_scrlup != this.scrollup)
			{
				this.previous_scrlup = this.scrollup;
				this.RefreshConsoleScreen();
			}
			Scene activeScene = SceneManager.GetActiveScene();
			if (activeScene.name != this.loadedLevel)
			{
				this.loadedLevel = activeScene.name;
				this.AddLog(string.Concat(new string[]
				{
					"Scene Manager: Loaded scene '",
					activeScene.name,
					"' [",
					activeScene.path,
					"]"
				}), new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
				this.RefreshConsoleScreen();
			}
			if (this.allwaysRefreshing)
			{
				this.RefreshConsoleScreen();
			}
		}

		// Token: 0x0600006F RID: 111
		public void ToggleConsole()
		{
			CursorManager.consoleOpen = !this.console.activeSelf;
			this.cmdField.text = string.Empty;
			this.console.SetActive(!this.console.activeSelf);
			if (PlayerManager.singleton != null)
			{
				foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Player"))
				{
					if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						FirstPersonController component = gameObject.GetComponent<FirstPersonController>();
						if (component != null)
						{
							component.usingConsole = this.console.activeSelf;
						}
					}
				}
			}
			if (this.console.activeSelf)
			{
				EventSystem.current.SetSelectedGameObject(this.cmdField.gameObject);
			}
		}

		// Token: 0x06000070 RID: 112
		public void QuitGame()
		{
			Application.Quit();
		}

		// Token: 0x06000071 RID: 113
		public void DumpGameObjStats()
		{
			GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (GameObject gameObject in array)
			{
				int num;
				dictionary.TryGetValue(gameObject.name.Trim(), out num);
				num++;
				dictionary.Add(gameObject.name.Trim(), num);
				ServerConsole.AddLog(gameObject.name.Trim() + " " + num);
			}
			string text = "C:\\dev\\unity-scene_stats.txt";
			ServerConsole.AddLog("Dumping scene stats to " + text + " ...");
			using (StreamWriter streamWriter = new StreamWriter(text, false))
			{
				foreach (KeyValuePair<string, int> keyValuePair in dictionary)
				{
					streamWriter.WriteLine("{0}={1}", keyValuePair.Key, keyValuePair.Value);
				}
			}
			ServerConsole.AddLog("Scene dumped to " + text);
		}

		// Token: 0x06000072 RID: 114
		public void GetStats(string parent, Component obj, Dictionary<string, int> dictionary)
		{
			string key = parent + "_" + obj.GetType().Name;
			int num = 0;
			dictionary.TryGetValue(key, out num);
			dictionary.Add(parent + obj.tag, num++);
		}

		// Token: 0x06000073 RID: 115
		public void GetStats(string parent, GameObject obj, Dictionary<string, int> dictionary, HashSet<int> done)
		{
			if (done.Contains(obj.GetInstanceID()))
			{
				return;
			}
			string text = parent + "_" + obj.name;
			int num = 0;
			dictionary.TryGetValue(text, out num);
			dictionary.Add(parent + obj.tag, num++);
			done.Add(obj.GetInstanceID());
			foreach (Component component in base.gameObject.GetComponents<Component>())
			{
				if (!(component == obj))
				{
					this.GetStats(text, component, dictionary);
				}
			}
		}

		// Token: 0x04000069 RID: 105
		public bool allwaysRefreshing;

		// Token: 0x0400006A RID: 106
		public List<Console.Log> logs = new List<Console.Log>();

		// Token: 0x0400006B RID: 107
		public List<Console.Value> values = new List<Console.Value>();

		// Token: 0x0400006C RID: 108
		public Console.CommandHint[] hints;

		// Token: 0x0400006D RID: 109
		public Text txt;

		// Token: 0x0400006E RID: 110
		public InputField cmdField;

		// Token: 0x0400006F RID: 111
		public GameObject console;

		// Token: 0x04000070 RID: 112
		public static Console singleton;

		// Token: 0x04000071 RID: 113
		public int scrollup;

		// Token: 0x04000072 RID: 114
		public int previous_scrlup;

		// Token: 0x04000073 RID: 115
		public string loadedLevel;

		// Token: 0x04000074 RID: 116
		public string response = string.Empty;

		// Token: 0x02000017 RID: 23
		[Serializable]
		public class CommandHint
		{
			// Token: 0x06000074 RID: 116
			public CommandHint()
			{
			}

			// Token: 0x04000075 RID: 117
			public string name;

			// Token: 0x04000076 RID: 118
			public string shortDesc;

			// Token: 0x04000077 RID: 119
			[Multiline]
			public string fullDesc;
		}

		// Token: 0x02000018 RID: 24
		[Serializable]
		public class Value
		{
			// Token: 0x06000075 RID: 117
			public Value(string k, string v)
			{
				this.key = k;
				this.value = v;
			}

			// Token: 0x04000078 RID: 120
			public string key;

			// Token: 0x04000079 RID: 121
			public string value;
		}

		// Token: 0x02000019 RID: 25
		[Serializable]
		public class Log
		{
			// Token: 0x06000076 RID: 118
			public Log(string t, Color32 c, bool b)
			{
				this.text = t;
				this.color = c;
				this.nospace = b;
			}

			// Token: 0x0400007A RID: 122
			public string text;

			// Token: 0x0400007B RID: 123
			public Color32 color;

			// Token: 0x0400007C RID: 124
			public bool nospace;
		}
	}
}
