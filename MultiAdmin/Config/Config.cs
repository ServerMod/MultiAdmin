using System;
using System.IO;
using System.Linq;
using System.Text;
using MultiAdmin.ConsoleTools;

namespace MultiAdmin.Config
{
	public class Config
	{
		public string[] rawData = { };

		public Config(string path)
		{
			ReadConfigFile(path);
		}

		public string ConfigPath { get; private set; }

		public void ReadConfigFile(string configPath)
		{
			if (string.IsNullOrEmpty(configPath)) return;

			ConfigPath = configPath;
			try
			{
				ConfigPath = Utils.GetFullPathSafe(ConfigPath);
			}
			catch (Exception e)
			{
				Program.LogDebugException("ReadConfigFile", e);
			}

			try
			{
				rawData = File.Exists(ConfigPath) ? File.ReadAllLines(ConfigPath, Encoding.UTF8) : new string[] { };
			}
			catch (Exception e)
			{
				new ColoredMessage[]
				{
					new ColoredMessage($"Error while reading config (Path = {ConfigPath ?? "Null"}):", ConsoleColor.Red),
					new ColoredMessage(e.ToString(), ConsoleColor.Red)
				}.WriteLines();
			}
		}

		public void ReadConfigFile()
		{
			ReadConfigFile(ConfigPath);
		}

		public bool Contains(string key)
		{
			return rawData != null && rawData.Any(entry => entry.ToLower().StartsWith(key.ToLower() + ":"));
		}

		private static string CleanValue(string value)
		{
			if (string.IsNullOrEmpty(value)) return value;

			string newValue = value.Trim();

			try
			{
				if (newValue.StartsWith("\"") && newValue.EndsWith("\""))
					return newValue.Substring(1, newValue.Length - 2);
			}
			catch (Exception e)
			{
				Program.LogDebugException("CleanValue", e);
			}

			return newValue;
		}

		public string GetString(string key, string def = null)
		{
			try
			{
				foreach (string line in rawData)
				{
					if (!line.ToLower().StartsWith(key.ToLower() + ":")) continue;

					try
					{
						return CleanValue(line.Substring(key.Length + 1));
					}
					catch (Exception e)
					{
						Program.LogDebugException("GetString", e);
					}
				}
			}
			catch (Exception e)
			{
				Program.LogDebugException("GetString", e);
			}

			return def;
		}

		public string[] GetStringList(string key, string[] def = null)
		{
			try
			{
				foreach (string line in rawData)
				{
					if (!line.ToLower().StartsWith(key.ToLower() + ":")) continue;

					try
					{
						return line.Substring(key.Length + 1).Split(',').Select(CleanValue).ToArray();
					}
					catch (Exception e)
					{
						Program.LogDebugException("GetStringList", e);
					}
				}
			}
			catch (Exception e)
			{
				Program.LogDebugException("GetStringList", e);
			}

			return def;
		}

		public int GetInt(string key, int def = 0)
		{
			try
			{
				string value = GetString(key);

				if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int parseValue))
					return parseValue;
			}
			catch (Exception e)
			{
				Program.LogDebugException("GetInt", e);
			}

			return def;
		}

		public uint GetUInt(string key, uint def = 0)
		{
			try
			{
				string value = GetString(key);

				if (!string.IsNullOrEmpty(value) && uint.TryParse(value, out uint parseValue))
					return parseValue;
			}
			catch (Exception e)
			{
				Program.LogDebugException("GetUInt", e);
			}

			return def;
		}

		public float GetFloat(string key, float def = 0)
		{
			try
			{
				string value = GetString(key);

				if (!string.IsNullOrEmpty(value) && float.TryParse(value, out float parsedValue))
					return parsedValue;
			}
			catch (Exception e)
			{
				Program.LogDebugException("GetFloat", e);
			}

			return def;
		}

		public bool GetBool(string key, bool def = false)
		{
			try
			{
				string value = GetString(key);

				if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out bool parsedValue))
					return parsedValue;
			}
			catch (Exception e)
			{
				Program.LogDebugException("GetBool", e);
			}

			return def;
		}
	}
}
