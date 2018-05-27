using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class YamlConfig
{
	public string[] RawData;

	public YamlConfig()
	{
	}

	public YamlConfig(string path)
	{
		if (File.Exists(path))
		{
			LoadConfigFile(path);
		}
		else
		{
			RawData = new string[] { };
		}
	}

	public void LoadConfigFile(string path)
	{
		RawData = FileManager.ReadAllLines(path);
	}

	public string GetString(string key, string def = "")
	{
		foreach (var line in RawData)
		{
			if (line.StartsWith(key + ": ")) return line.Substring(key.Length + 2);
		}

		return def;
	}

	public int GetInt(string key, int def = 0)
	{
		foreach (var line in RawData)
		{
			if (!line.StartsWith(key + ": ")) continue;
			try
			{
				return Convert.ToInt32(line.Substring(key.Length + 2));
			}
			catch
			{
				return 0;
			}
		}

		return def;
	}

	public float GetFloat(string key, float def = 0)
	{
		var ky = GetString(key);
		if (ky == "") return def;
		ky = ky.Replace(',', '.');
		float result;
		return float.TryParse(ky, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result) ? result : def;
	}

	public bool GetBool(string key, bool def = false)
	{
		foreach (var line in RawData)
		{
			if (!line.StartsWith(key + ": ")) continue;
			return line.Substring(key.Length + 2) == "true";
		}

		return def;
	}

	public List<string> GetStringList(string key)
	{
		var read = false;
		var list = new List<string>();
		foreach (var line in RawData)
		{
			if (line.StartsWith(key + ":"))
			{
				read = true;
				continue;
			}
			if (!read) continue;
			if (line.StartsWith(" - ")) list.Add(line.Substring(3));
			else if (!line.StartsWith("#")) break;
		}
		return list;
	}

	public List<int> GetIntList(string key)
	{
		var list = GetStringList(key);
		return list.Select(x => Convert.ToInt32(x)).ToList();
	}

	public Dictionary<string, string> GetStringDictionary(string key)
	{
		var list = GetStringList(key);
		var dict = new Dictionary<string, string>();
		foreach (var item in list)
		{
			var i = item.IndexOf(": ", StringComparison.Ordinal);
			dict.Add(item.Substring(0, i), item.Substring(i + 2));
		}

		return dict;
	}

	public static string[] ParseCommaSeparatedString(string data)
	{
		if (!data.StartsWith("[") || !data.EndsWith("]")) return null;
		data = data.Substring(1, data.Length - 2);
		return data.Split(new string[] { ", " }, StringSplitOptions.None);
	}
}
