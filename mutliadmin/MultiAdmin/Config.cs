using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace MultiAdmin
{
	public class OldConfigException : Exception
	{
		public OldConfigException(string message) : base(message)
		{
		}
	}


	public class Config
	{
		public Dictionary<String, String> values;
		private String config_file;

		public Config(String config_file)
		{
			this.config_file = config_file;
			Reload();
		}

		public void Reload()
		{
			values = new Dictionary<string, string>();

			if (File.Exists(config_file))
			{
				StreamReader streamReader = new StreamReader(config_file);

				var yaml = new YamlStream();
				yaml.Load(streamReader);
				streamReader.Close();

				if (yaml.Documents.Count == 0)
				{
					throw new OldConfigException("Could not load YAML config, have you updated your configs to be YAML?");
				}

				var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
				// TODO: We dont need to put it in a dict, MA should just read from the YAML tree directly. This is just a quick fix
				foreach (var entry in mapping.Children)
				{
					Console.WriteLine(entry);
					values.Add((String) entry.Key, (String) entry.Value);
				}
			}
		}


		public String GetValue(String key, String def = "")
		{
			String val = null;
			if (!values.TryGetValue(key.ToLower(), out val))
			{
				val = def;
			}

			return val;
		}

		public int GetIntValue(String key, int def)
		{
			int result;
			bool successful = int.TryParse(GetValue(key, def.ToString()), out result);

			if (successful)
			{
				return result;
			}
			else
			{
				Console.WriteLine("WARNING: failed to parse integer value for config setting:" + key + " using default value.");
				return def;
			}
		}

		public Boolean GetBoolean(String key, bool def)
		{
			String configValue = GetValue(key, def.ToString());

			// Why did I make it so you can use these words? Because I can.
			string[] trueWords = new string[]
			{
			"true",
			"t",
			"y",
			"yes",
			"sure",
			"yeah",
			"yea",
			"affirmative",
			"aye",
			"1"
			};

			string[] falseWords = new string[]
			{
			"false",
			"f",
			"n",
			"no",
			"nope",
			"nah",
			"negative",
			"nay",
			"0"
			};

			foreach (string word in trueWords)
			{
				if (configValue.Equals(word.ToLower()))
					return true;
			}

			foreach (string word in falseWords)
			{
				if (configValue.Equals(word.ToLower()))
					return false;
			}

			return def;
		}
	}
}
