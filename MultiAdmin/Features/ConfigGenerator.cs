using System.Collections.Generic;
using System.IO;
using System.Text;
using MultiAdmin.Config;
using MultiAdmin.Config.ConfigHandler;
using MultiAdmin.Features.Attributes;
using MultiAdmin.Utility;

namespace MultiAdmin.Features
{
	[Feature]
	internal class ConfigGenerator : Feature, ICommand
	{

		public ConfigGenerator(Server server) : base(server)
		{
		}

		public string GetCommand()
		{
			return "CONFIGGEN";
		}

		public string GetCommandDescription()
		{
			return "Generates a full default MultiAdmin config file";
		}

		public string GetUsage()
		{
			return "[FILE LOCATION]";
		}

		public void OnCall(string[] args)
		{
			if (args.IsEmpty())
			{
				Server.Write("You must specify the location of the file.");
				return;
			}

			string path = Utils.GetFullPathSafe(string.Join(" ", args));

			ConfigEntry[] registeredConfigs = MultiAdminConfig.GlobalConfig.GetRegisteredConfigs();

			List<string> lines = new List<string>(registeredConfigs.Length);
			foreach (ConfigEntry configEntry in registeredConfigs)
			{
				switch (configEntry)
				{
					case ConfigEntry<string[]> config:
					{
						lines.Add($"{config.Key}: {(config.Default == null ? "" : string.Join(", ", config.Default))}");
						break;
					}

					default:
					{
						lines.Add($"{configEntry.Key}: {configEntry.ObjectDefault ?? ""}");
						break;
					}
				}
			}

			File.WriteAllLines(path, lines);
			Server.Write($"Default config written to \"{path}\"");
		}

		public bool PassToGame()
		{
			return false;
		}

		public override void OnConfigReload()
		{
		}

		public override string GetFeatureDescription()
		{
			return "Generates a full default MultiAdmin config file";
		}

		public override string GetFeatureName()
		{
			return "Config Generator";
		}

		public override void Init()
		{
		}
	}
}
