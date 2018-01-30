using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAdmin
{
    class ConfigParser
    {
        public Dictionary<String, String> values;
        public ConfigParser(String config_file)
        {
            values = new Dictionary<string, string>();
            var multi_line_value = false;
            var current_key = "";
            var current_value = "";
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
                    current_value = line.Substring(line.IndexOf("=")+1);
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


        public String GetValue(String key, String def="")
        {
            String val = null;
            if (!values.TryGetValue(key.ToLower(), out val))
            {
                val = def;
            }

            return val;
        }
    }
}
