using System;
using System.IO;
using System.Linq;

namespace MultiAdmin
{
	public class Config
	{
		public string[] rawData;

		public Config(string path)
		{
			ReadConfigFile(path);
		}

		public string Path { get; private set; }

		public void ReadConfigFile(string path)
		{
			rawData = File.Exists(path) ? FileManager.ReadAllLines(path) : new string[] { };
			Path = path;
		}

		public void ReadConfigFile()
		{
			ReadConfigFile(Path);
		}

		public bool Contains(string key)
		{
			return rawData.Any(entry => entry.ToLower().StartsWith(key.ToLower() + ":"));
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

		private static string CleanValue(string value)
		{
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

		public int GetInt(string key, int def = 0)
		{
			try
			{
				string value = GetString(key);

				if (!string.IsNullOrEmpty(value))
					return Convert.ToInt32(value);
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

				if (!string.IsNullOrEmpty(value))
					return Convert.ToBoolean(value);
			}
			catch
			{
				// ignored
			}

			return def;
		}
	}
}