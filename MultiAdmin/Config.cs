using System.IO;
using System.Linq;
using System.Text;

namespace MultiAdmin
{
	public class Config
	{
		public string[] rawData;

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
			catch
			{
				// ignored
			}

			rawData = File.Exists(ConfigPath) ? File.ReadAllLines(ConfigPath, Encoding.UTF8) : new string[] { };
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
			catch
			{
				// ignored
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
					catch
					{
						// ignored
					}
				}
			}
			catch
			{
				// ignored
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
			catch
			{
				// ignored
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
			catch
			{
				// ignored
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
			catch
			{
				// ignored
			}

			return def;
		}
	}
}