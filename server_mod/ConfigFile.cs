using System;
using System.IO;
using GameConsole;
using UnityEngine;

// Token: 0x0200001C RID: 28
public class ConfigFile : MonoBehaviour
{
	// Token: 0x0600008B RID: 139 RVA: 0x0000AF80 File Offset: 0x00009180
	private void Awake()
	{
		ConfigFile.singleton = this;
		ConfigFile.path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory";
		try
		{
			if (!Directory.Exists(ConfigFile.path))
			{
				Directory.CreateDirectory(ConfigFile.path);
			}
		}
		catch
		{
			GameConsole.Console.singleton.AddLog("Configuration file directory creation failed.", new Color32(byte.MaxValue, 0, 0, byte.MaxValue), false);
		}
		ConfigFile.path += "/config.txt";
	}

	// Token: 0x0600008C RID: 140 RVA: 0x0000B00C File Offset: 0x0000920C
	private void Start()
	{
		if (!this.ReloadConfig())
		{
			GameConsole.Console.singleton.AddLog("Configuration file could not be loaded - template not found! Loading default settings..", new Color32(byte.MaxValue, 0, 0, byte.MaxValue), false);
			GameConsole.Console.singleton.AddLog("Default settings have been loaded.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
		}
	}

	// Token: 0x0600008D RID: 141 RVA: 0x0000B068 File Offset: 0x00009268
	public bool ReloadConfig()
	{
		if (!File.Exists(ConfigFile.path))
		{
			try
			{
				File.Copy("config_template.txt", ConfigFile.path);
			}
			catch
			{
				return false;
			}
		}
		StreamReader streamReader = new StreamReader(ConfigFile.path);
		this.cfg = streamReader.ReadToEnd();
		streamReader.Close();
		return true;
	}

	// Token: 0x0600008E RID: 142 RVA: 0x0000B0C8 File Offset: 0x000092C8
	public static string GetString(string key, string defaultValue = "")
	{
		string text = ConfigFile.singleton.cfg;
		if (text.Contains(key))
		{
			try
			{
				while (!text.ToLower().Replace(" ", "").StartsWith(key.ToLower() + "="))
				{
					if (!text.Contains(Environment.NewLine))
					{
						return defaultValue;
					}
					text = text.Remove(0, text.IndexOf(Environment.NewLine) + Environment.NewLine.Length).TrimStart(new char[]
					{
						' '
					});
				}
				text = text.Remove(0, text.IndexOf("=") + 1);
				text = text.TrimStart(new char[]
				{
					' '
				});
				return text.Remove(text.IndexOf(";"));
			}
			catch
			{
				return defaultValue;
			}
			return defaultValue;
		}
		return defaultValue;
	}

	// Token: 0x0600008F RID: 143 RVA: 0x0000B1B0 File Offset: 0x000093B0
	public static int GetInt(string key, int defaultValue = 0)
	{
		int result = 0;
		if (int.TryParse(ConfigFile.GetString(key, "errorInConverting"), out result))
		{
			return result;
		}
		return defaultValue;
	}

	// Token: 0x0400009C RID: 156
	public static ConfigFile singleton;

	// Token: 0x0400009D RID: 157
	public static string path;

	// Token: 0x0400009E RID: 158
	public string cfg;
}
