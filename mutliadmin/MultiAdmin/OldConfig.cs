using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MutliAdmin;

namespace MultiAdmin
{
    public class OldConfig
    {
        public Dictionary<String, String> values;
        private String config_file;

        public OldConfig(String config_file)
        {
            this.config_file = config_file;
            Reload();
        }
        private readonly Regex rgx = new Regex("^[^;\\/:\\n\\r\\s=]+\\s*=[^;\\n\\r]+;", RegexOptions.Multiline | RegexOptions.Compiled);

		public string[] GetRaw()
		{
			if (File.Exists(config_file))
			{
				StreamReader streamReader = new StreamReader(config_file);
				List<string> content = new List<string>();

				string line;
				while ((line = streamReader.ReadLine()) != null)
				{
					content.Add(line);
				}

				streamReader.Close();

				return content.ToArray();
			}

			return new string[] { };
		}

		public void Reload()
        {
            values = new Dictionary<string, string>();

			if (File.Exists(config_file))
			{
				StreamReader streamReader = new StreamReader(config_file);
				string content = streamReader.ReadToEnd();
				streamReader.Close();

				MatchCollection matches = rgx.Matches(content);

				foreach (Match match in matches)
				{
					String[] parts = match.Value.Split(new char[] { '=' }, 2);

					String key = parts[0].Trim().ToLower();
					String value = parts[1].Trim();

					if (value.Length > 0)
					{
						value = value.Substring(0, value.Length - 1); // Removes ";" from the end

						if (!values.ContainsKey(key))
						{
							values.Add(key, value);
						}
						else
						{
							Program.Write("Duplicate value found in config file:" + key + " using the first");
						}
					}
					else
					{
						Program.Write("Error: Config value is missing!");
					}
				}
			}

		}


        public String GetValue(String key, String def="")
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
