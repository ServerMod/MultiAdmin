using System;
using System.IO;
using System.Linq;
using System.Text;
using MultiAdmin.ConsoleTools;
using MultiAdmin.ServerIO;
using MultiAdmin.Utility;

namespace MultiAdmin.Config
{
	public class Config
	{
		public string[] rawData = Array.Empty<string>();

		public Config(string path)
		{
			internalConfigPath = path;
			ReadConfigFile(path);
		}

		private string internalConfigPath;

		public string ConfigPath
		{
			get => internalConfigPath;
			private set
			{
				try
				{
					internalConfigPath = Utils.GetFullPathSafe(value) ?? value;
				}
				catch (Exception e)
				{
					internalConfigPath = value;
					Program.LogDebugException(nameof(ConfigPath), e);
				}
			}
		}

		public void ReadConfigFile(string configPath)
		{
			ConfigPath = configPath;

			try
			{
				rawData = File.Exists(ConfigPath) ? File.ReadAllLines(ConfigPath, Encoding.UTF8) : Array.Empty<string>();
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(ReadConfigFile), e);

				new ColoredMessage[]
				{
					new ColoredMessage($"Error while reading config (Path = {ConfigPath}):",
						ConsoleColor.Red),
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
			return rawData != null &&
				   rawData.Any(entry => entry.StartsWith($"{key}:", StringComparison.CurrentCultureIgnoreCase));
		}

		private static string CleanValue(string value, bool removeQuotes = true)
		{
			if (string.IsNullOrEmpty(value)) return value;

			string newValue = value.Trim();

			try
			{
				if (removeQuotes && newValue.StartsWith("\"") && newValue.EndsWith("\""))
					return newValue[1..^1];
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(CleanValue), e);
			}

			return newValue;
		}

		public string? GetString(string key, string? def = null, bool removeQuotes = true)
		{
			try
			{
				foreach (string line in rawData)
				{
					if (!line.ToLower().StartsWith(key.ToLower() + ":")) continue;

					try
					{
						return CleanValue(line[(key.Length + 1)..], removeQuotes);
					}
					catch (Exception e)
					{
						Program.LogDebugException(nameof(GetString), e);
					}
				}
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(GetString), e);
			}

			return def;
		}

		public string[]? GetStringArray(string key, string[]? def = null)
		{
			try
			{
				string? value = GetString(key, removeQuotes: false);

				if (!string.IsNullOrEmpty(value))
				{
					try
					{
						return value.Split(',').Select(entry => CleanValue(entry)).ToArray();
					}
					catch (Exception e)
					{
						Program.LogDebugException(nameof(GetStringArray), e);
					}
				}
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(GetStringArray), e);
			}

			return def;
		}

		public int GetInt(string key, int def = 0)
		{
			try
			{
				string? value = GetString(key);

				if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int parseValue))
					return parseValue;
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(GetInt), e);
			}

			return def;
		}

		public uint GetUInt(string key, uint def = 0)
		{
			try
			{
				string? value = GetString(key);

				if (!string.IsNullOrEmpty(value) && uint.TryParse(value, out uint parseValue))
					return parseValue;
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(GetUInt), e);
			}

			return def;
		}

		public float GetFloat(string key, float def = 0)
		{
			try
			{
				string? value = GetString(key);

				if (!string.IsNullOrEmpty(value) && float.TryParse(value, out float parsedValue))
					return parsedValue;
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(GetFloat), e);
			}

			return def;
		}

		public double GetDouble(string key, double def = 0)
		{
			try
			{
				string? value = GetString(key);

				if (!string.IsNullOrEmpty(value) && double.TryParse(value, out double parsedValue))
					return parsedValue;
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(GetDouble), e);
			}

			return def;
		}

		public decimal GetDecimal(string key, decimal def = 0)
		{
			try
			{
				string? value = GetString(key);

				if (!string.IsNullOrEmpty(value) && decimal.TryParse(value, out decimal parsedValue))
					return parsedValue;
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(GetDecimal), e);
			}

			return def;
		}

		public bool GetBool(string key, bool def = false)
		{
			try
			{
				string? value = GetString(key);

				if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out bool parsedValue))
					return parsedValue;
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(GetBool), e);
			}

			return def;
		}

		public InputHandler.ConsoleInputSystem GetConsoleInputSystem(string key, InputHandler.ConsoleInputSystem def = InputHandler.ConsoleInputSystem.New)
		{
			try
			{
				string? value = GetString(key);

				if (!string.IsNullOrEmpty(value) && Enum.TryParse<InputHandler.ConsoleInputSystem>(value, out var consoleInputSystem))
					return consoleInputSystem;
			}
			catch (Exception e)
			{
				Program.LogDebugException(nameof(GetConsoleInputSystem), e);
			}

			return def;
		}
	}
}
