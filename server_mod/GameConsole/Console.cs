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
	public class Console : MonoBehaviour
	{
		public Console()
		{
		}

		public List<Console.Log> GetAllLogs()
		{
			return this.logs;
		}

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

		public void Start()
		{
			this.AddLog("Hi there! Initializing console...", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
			this.AddLog("Done! Type 'help' to print the list of available commands.", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
			Debug.logger.logEnabled = true;
			this.RefreshConsoleScreen();
		}

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

		public string ColorToHex(Color32 color)
		{
			string str = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
			return "#" + str;
		}

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
            else if (cmd == "GIVEALL")
            {
                int num2 = 0;
                if (array.Length >= 2 && int.TryParse(array[1], out num2))
                {
                    string a2 = "offline";
                    foreach (GameObject gameObject2 in PlayerManager.singleton.players)
                    {
                        a2 = "online";
                        CharacterClassManager component2 = gameObject2.GetComponent<CharacterClassManager>();
                        if (component2 != null && component2.klasy[component2.NetworkcurClass].team != Team.SCP)
                        {
                            Inventory component3 = gameObject2.GetComponent<Inventory>();
                            if (component3 != null)
                            {
                                if (component3.availableItems.Length > num2)
                                {
                                    Searching component4 = gameObject2.GetComponent<Searching>();
                                    if (component4 != null)
                                    {
                                        Locker locker = UnityEngine.Object.FindObjectOfType<Locker>();
                                        if (locker != null)
                                        {
                                            if (component3.netFreeSlots > 0)
                                            {
                                                int[] ids = locker.ids;
                                                locker.ids = new int[]
                                                {
                                                    num2
                                                };
                                                component4.CallCmdPickupItem(locker.gameObject, gameObject2);
                                                locker.SetTaken(false);
                                                locker.ids = ids;
                                            }
                                            else
                                            {
                                                component4.CallCmdPickupItem(locker.gameObject, gameObject2);
                                                locker.SetTaken(false);
                                                component3.CallCmdSetPickup(num2, -4.65664672E+11f, component3.transform.position, component3.transform.rotation, component3.transform.localRotation);
                                            }
                                        }
                                        else
                                        {
                                            this.AddLog("Failed to add ITEM#" + num2.ToString("000") + " - There are no lockers, silently dropping instead...", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                                            component3.CallCmdSetPickup(num2, -4.65664672E+11f, component3.transform.position, component3.transform.rotation, component3.transform.localRotation);
                                            component3.netFreeSlots--;
                                        }
                                    }
                                    a2 = "none";
                                }
                                else
                                {
                                    this.AddLog("Failed to add ITEM#" + num2.ToString("000") + " - item does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                                }
                            }
                            else
                            {
                                this.AddLog("Failed to add ITEM#" + num2.ToString("000") + " - Player has no inventory!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            }
                        }
                        else
                        {
                            this.AddLog("Failed to add ITEM#" + num2.ToString("000") + " - Player is an SCP!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            a2 = "none";
                        }
                    }
                    if (a2 == "offline" || a2 == "online")
                    {
                        this.AddLog((!(a2 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                    }
                    else
                    {
                        this.AddLog("ITEM#" + num2.ToString("000") + " has been added!", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
                    }
                }
                else
                {
                    this.AddLog("Second argument has to be a number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                }
            }
            else if (cmd == "GIVEPLAYER")
            {
                int num3 = 0;
                if (array.Length >= 3 && int.TryParse(array[2], out num3))
                {
                    string text = array[1];
                    string a3 = "offline";
                    foreach (GameObject gameObject3 in PlayerManager.singleton.players)
                    {
                        a3 = "online";
                        CharacterClassManager component5 = gameObject3.GetComponent<CharacterClassManager>();
                        NicknameSync component6 = gameObject3.GetComponent<NicknameSync>();
                        if (component5 != null && component5.klasy[component5.NetworkcurClass].team != Team.SCP)
                        {
                            if (component6 != null)
                            {
                                if (component6.NetworkmyNick.ToLower().Contains(text.ToLower()))
                                {
                                    Inventory component7 = gameObject3.GetComponent<Inventory>();
                                    if (component7 != null)
                                    {
                                        if (component7.availableItems.Length > num3)
                                        {
                                            Searching component8 = gameObject3.GetComponent<Searching>();
                                            if (component8 != null)
                                            {
                                                Locker locker2 = UnityEngine.Object.FindObjectOfType<Locker>();
                                                if (locker2 != null)
                                                {
                                                    if (component7.netFreeSlots > 0)
                                                    {
                                                        int[] ids2 = locker2.ids;
                                                        locker2.ids = new int[]
                                                        {
                                                            num3
                                                        };
                                                        component8.CallCmdPickupItem(locker2.gameObject, gameObject3);
                                                        locker2.SetTaken(false);
                                                        locker2.ids = ids2;
                                                    }
                                                    else
                                                    {
                                                        component8.CallCmdPickupItem(locker2.gameObject, gameObject3);
                                                        locker2.SetTaken(false);
                                                        component7.CallCmdSetPickup(num3, -4.65664672E+11f, component7.transform.position, component7.transform.rotation, component7.transform.localRotation);
                                                    }
                                                }
                                                else
                                                {
                                                    this.AddLog("Failed to add ITEM#" + num3.ToString("000") + " - There are no lockers, silently dropping instead...", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                                                    component7.CallCmdSetPickup(num3, -4.65664672E+11f, component7.transform.position, component7.transform.rotation, component7.transform.localRotation);
                                                    component7.netFreeSlots--;
                                                }
                                            }
                                            a3 = "none";
                                        }
                                        else
                                        {
                                            this.AddLog("Failed to add ITEM#" + num3.ToString("000") + " - item does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                                        }
                                    }
                                    else
                                    {
                                        this.AddLog("Failed to add ITEM#" + num3.ToString("000") + " - Player has no inventory!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                                    }
                                }
                                else
                                {
                                    this.AddLog(string.Concat(new string[]
                                    {
                                        "Failed to add ITEM#",
                                        num3.ToString("000"),
                                        " - Username doesn't match! (\"",
                                        component6.NetworkmyNick,
                                        "\" against \"",
                                        text,
                                        "\")"
                                    }), new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                                    a3 = "none";
                                }
                            }
                            else
                            {
                                this.AddLog("Failed to add ITEM#" + num3.ToString("000") + " - Player has no nickname!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                                a3 = "none";
                            }
                        }
                        else
                        {
                            this.AddLog("Failed to add ITEM#" + num3.ToString("000") + " - Player is an SCP!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            a3 = "none";
                        }
                    }
                    if (a3 == "offline" || a3 == "online")
                    {
                        this.AddLog((!(a3 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                    }
                    else
                    {
                        this.AddLog("ITEM#" + num3.ToString("000") + " has been added!", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
                    }
                }
                else
                {
                    this.AddLog("Third argument has to be a number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
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
							((CustomNetworkManager) NetworkManager.singleton).UpdateMotd(0);
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

		public void ProceedButton()
		{
			if (this.cmdField.text != string.Empty)
			{
				this.TypeCommand(this.cmdField.text);
			}
			this.cmdField.text = string.Empty;
			EventSystem.current.SetSelectedGameObject(this.cmdField.gameObject);
		}

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

		public void QuitGame()
		{
			Application.Quit();
		}

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

		public void GetStats(string parent, Component obj, Dictionary<string, int> dictionary)
		{
			string key = parent + "_" + obj.GetType().Name;
			int num = 0;
			dictionary.TryGetValue(key, out num);
			dictionary.Add(parent + obj.tag, num++);
		}

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

		public bool allwaysRefreshing;

		public List<Console.Log> logs = new List<Console.Log>();

		public List<Console.Value> values = new List<Console.Value>();

		public Console.CommandHint[] hints;

		public Text txt;

		public InputField cmdField;

		public GameObject console;

		public static Console singleton;

		public int scrollup;

		public int previous_scrlup;

		public string loadedLevel;

		public string response = string.Empty;

		[Serializable]
		public class CommandHint
		{
			public CommandHint()
			{
			}

			public string name;

			public string shortDesc;

			[Multiline]
			public string fullDesc;
		}

		[Serializable]
		public class Value
		{
			public Value(string k, string v)
			{
				this.key = k;
				this.value = v;
			}

			public string key;

			public string value;
		}

		[Serializable]
		public class Log
		{
			public Log(string t, Color32 c, bool b)
			{
				this.text = t;
				this.color = c;
				this.nospace = b;
			}

			public string text;

			public Color32 color;

			public bool nospace;
		}
	}
}
