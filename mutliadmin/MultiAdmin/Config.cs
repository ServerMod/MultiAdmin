using System;
using System.Collections.Generic;
using System.IO;


namespace MultiAdmin
{
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
            var multi_line_value = false;
            var current_key = "";
            var current_value = "";
            if (File.Exists(config_file))
            {
                var lines = File.ReadAllLines(config_file);
                foreach (String line in lines)
                {
                    if (line.Trim().Length == 0) continue;
                    if (line.StartsWith("/")) continue;
                    if (line.EndsWith(":")) continue;

                    if (multi_line_value)
                    {
                        current_value += line;
                        if (line.EndsWith(";"))
                        {
                            values.Add(current_key, current_value.Substring(0, current_value.Length - 1).Trim());
                            multi_line_value = false;
                        }
                    }
                    else
                    {
                        current_key = line.Substring(0, line.IndexOf("=")).ToLower().Trim();
                        current_value = line.Substring(line.IndexOf("=") + 1);
                        if (current_value.EndsWith(";"))
                        {
							String value = current_value.Substring(0, current_value.Length - 1).Trim();
							if (!values.ContainsKey(current_key))
							{
								values.Add(current_key, current_value.Substring(0, current_value.Length - 1).Trim());
							}
							else
							{
								Console.WriteLine("Found duplicate setting for " + current_key + " with value " + current_value + " using the existing setting. The game may not do it this way, please correct the issue");
							}
                            
                        }
                        else
                        {
                            multi_line_value = true;
                        }
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
