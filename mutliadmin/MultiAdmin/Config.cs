﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                            values.Add(current_key, current_value.Substring(0, current_value.Length - 1).Trim());
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
            String result = GetValue(key, def.ToString());
            if (result.ToLower().Equals("yes") || result.ToLower().Equals("y") || result.ToLower().Equals("t") || result.ToLower().Equals("true") || result.ToLower().Equals("1"))
            {
                return true;
            }

            if (result.ToLower().Equals("n") || result.ToLower().Equals("no") || result.ToLower().Equals("f") || result.ToLower().Equals("false") || result.ToLower().Equals("f"))
            {
                return true;
            }

            Console.WriteLine("WARNING: config setting " + key + " is suppose to be a boolean value, but it is set to" + result + " usign default value.");
            return def;
        }
    }
}