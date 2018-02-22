using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace GameConsole
{
	// Token: 0x0200001D RID: 29
	public partial class Console : MonoBehaviour
	{
		// Token: 0x06000099 RID: 153 RVA: 0x0000B5B0 File Offset: 0x000097B0
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
				string a4 = "offline";
				foreach (GameObject gameObject4 in GameObject.FindGameObjectsWithTag("Player"))
				{
					int num4 = 1;
					if (array.Length >= 2 && !int.TryParse(array[1], out num4))
					{
						this.AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
						return this.response;
					}
					if (gameObject4.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						a4 = "online";
						Inventory component9 = gameObject4.GetComponent<Inventory>();
						if (component9 != null)
						{
							a4 = "none";
							if (num4 < 1)
							{
								this.AddLog("Page '" + num4 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
								this.RefreshConsoleScreen();
								return this.response;
							}
							Item[] availableItems = component9.availableItems;
							for (int j = 10 * (num4 - 1); j < 10 * num4; j++)
							{
								if (10 * (num4 - 1) > availableItems.Length)
								{
									this.AddLog("Page '" + num4 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
									break;
								}
								if (j >= availableItems.Length)
								{
									break;
								}
								this.AddLog("ITEM#" + j.ToString("000") + " : " + availableItems[j].label, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
							}
						}
					}
				}
				if (a4 != "none")
				{
					this.AddLog((!(a4 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
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
							this.AddLog(commandHint.name + " - " + commandHint.fullDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
							this.RefreshConsoleScreen();
							return this.response;
						}
					}
					this.AddLog("Help for command '" + array[1] + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
					this.RefreshConsoleScreen();
					return this.response;
				}
				this.AddLog("List of available commands:\n", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
				foreach (Console.CommandHint commandHint2 in this.hints)
				{
					this.AddLog(commandHint2.name + " - " + commandHint2.shortDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), true);
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
					string text2 = array[1];
					using (List<Console.Value>.Enumerator enumerator = this.values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.key == text2)
							{
								flag = true;
								this.AddLog(string.Concat(new string[]
								{
									"The value of ",
									text2,
									" is: @#{[",
									text2,
									"}]#@"
								}), new Color32(50, 70, 100, byte.MaxValue), false);
							}
						}
					}
					if (!flag)
					{
						this.AddLog("Key " + text2 + " not found!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
					}
				}
			}
			else if (cmd == "SEED")
			{
				GameObject gameObject5 = GameObject.Find("Host");
				int num5 = -1;
				if (gameObject5 != null)
				{
					num5 = gameObject5.GetComponent<RandomSeedSync>().seed;
				}
				this.AddLog("Map seed is: <b>" + ((num5 != -1) ? num5.ToString() : "NONE") + "</b>", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
			}
			else if (cmd == "SHOWRIDS")
			{
				GameObject[] array4 = GameObject.FindGameObjectsWithTag("RoomID");
				foreach (GameObject gameObject6 in array4)
				{
					gameObject6.GetComponentsInChildren<MeshRenderer>()[0].enabled = !gameObject6.GetComponentsInChildren<MeshRenderer>()[0].enabled;
					gameObject6.GetComponentsInChildren<MeshRenderer>()[1].enabled = !gameObject6.GetComponentsInChildren<MeshRenderer>()[1].enabled;
				}
				if (array4.Length != 0)
				{
					this.AddLog("Show RIDS: " + array4[0].GetComponentInChildren<MeshRenderer>().enabled.ToString(), new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
				}
				else
				{
					this.AddLog("There are no RIDS!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
			}
			else if (cmd == "CLASSLIST")
			{
				string a5 = "offline";
				foreach (GameObject gameObject7 in GameObject.FindGameObjectsWithTag("Player"))
				{
					int num6 = 1;
					if (array.Length >= 2 && !int.TryParse(array[1], out num6))
					{
						this.AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
						return this.response;
					}
					if (gameObject7.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						a5 = "online";
						CharacterClassManager component10 = gameObject7.GetComponent<CharacterClassManager>();
						if (component10 != null)
						{
							a5 = "none";
							if (num6 < 1)
							{
								this.AddLog("Page '" + num6 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
								this.RefreshConsoleScreen();
								return this.response;
							}
							Class[] klasy = component10.klasy;
							for (int k = 10 * (num6 - 1); k < 10 * num6; k++)
							{
								if (10 * (num6 - 1) > klasy.Length)
								{
									this.AddLog("Page '" + num6 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
									break;
								}
								if (k >= klasy.Length)
								{
									break;
								}
								this.AddLog("CLASS#" + k.ToString("000") + " : " + klasy[k].fullName, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
							}
						}
					}
				}
				if (a5 != "none")
				{
					this.AddLog((!(a5 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
			}
			else if (cmd == "GOTO")
			{
				if (array.Length >= 2)
				{
					GameObject gameObject8 = null;
					foreach (GameObject gameObject9 in GameObject.FindGameObjectsWithTag("RoomID"))
					{
						if (gameObject9.GetComponent<Rid>().id.ToUpper() == array[1].ToUpper())
						{
							gameObject8 = gameObject9;
						}
					}
					string a6 = "offline";
					if (gameObject8 != null)
					{
						foreach (GameObject gameObject10 in GameObject.FindGameObjectsWithTag("Player"))
						{
							if (gameObject10.GetComponent<NetworkIdentity>().isLocalPlayer)
							{
								if (array[1].ToUpper() == "RANGE" && !gameObject10.GetComponent<ShootingRange>().isOnRange)
								{
									a6 = "range";
								}
								else
								{
									a6 = "none";
									gameObject10.transform.position = gameObject8.transform.position;
								}
							}
						}
						if (a6 == "range")
						{
							this.AddLog("<b>Shooting range is disabled!</b>", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
						}
						else if (a6 == "offline")
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
				string a7 = "offline";
				foreach (GameObject gameObject11 in GameObject.FindGameObjectsWithTag("Player"))
				{
					if (gameObject11.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						a7 = "online";
						ShootingRange component11 = gameObject11.GetComponent<ShootingRange>();
						if (component11 != null)
						{
							a7 = "none";
							component11.isOnRange = true;
						}
					}
				}
				if (a7 == "offline" || a7 == "online")
				{
					this.AddLog((!(a7 == "offline")) ? "Player range script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
				}
				else
				{
					this.AddLog("<b>Shooting range</b> is now available!", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
				}
			}
			else if (cmd == "ROUNDRESTART")
			{
				bool flag2 = false;
				GameObject[] array5 = GameObject.FindGameObjectsWithTag("Player");
				for (int l = 0; l < array5.Length; l++)
				{
					PlayerStats component12 = array5[l].GetComponent<PlayerStats>();
					if (component12.isLocalPlayer && component12.isServer)
					{
						flag2 = true;
						this.AddLog("The round is about to restart! Please wait..", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
						component12.Roundrestart();
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
					string text3 = array[1];
					if (text3 != null)
					{
						if (!(text3 == "RELOAD") && !(text3 == "R") && !(text3 == "RLD"))
						{
							if (!(text3 == "PATH"))
							{
								if (text3 == "VALUE")
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
								string text4 = string.Empty;
								GameObject gameObject12 = Console.FindConnectedRoot(networkConnection);
								if (gameObject12 != null)
								{
									text4 = gameObject12.GetComponent<NicknameSync>().myNick;
								}
								if (text4 == string.Empty)
								{
									this.AddLog("Player :: " + networkConnection.address, new Color32(160, 128, 128, byte.MaxValue), true);
								}
								else
								{
									this.AddLog("Player :: " + text4 + " :: " + networkConnection.address, new Color32(128, 160, 128, byte.MaxValue), true);
								}
							}
							goto IL_1AE6;
						}
					}
					int duration = 0;
					if (int.TryParse(array[2], out duration))
					{
						bool flag3 = false;
						foreach (NetworkConnection networkConnection2 in NetworkServer.connections)
						{
							GameObject gameObject13 = Console.FindConnectedRoot(networkConnection2);
							if (networkConnection2.address.ToUpper().Contains(array[1]) || (gameObject13 != null && gameObject13.GetComponent<NicknameSync>().myNick.ToUpper().Contains(array[1])))
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
			IL_1AE6:
			return this.response;
		}
	}
}
