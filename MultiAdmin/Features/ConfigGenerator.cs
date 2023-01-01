using System;
using System.Collections.Generic;
using System.IO;
using MultiAdmin.Config;
using MultiAdmin.Config.ConfigHandler;
using MultiAdmin.Utility;

namespace MultiAdmin.Features
{
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
			if (args.IsNullOrEmpty())
			{
				Server.Write("You must specify the location of the file.");
				return;
			}

			string path = args[0];
			try
			{
				FileAttributes fileAttributes = File.GetAttributes(path);

				if (fileAttributes.HasFlag(FileAttributes.Directory))
				{
					// Path provided is a directory, add a default file
					path = Path.Combine(path, MultiAdminConfig.ConfigFileName);
				}
			}
			catch (ArgumentException)
			{
				Server.Write("The path provided is empty, contains only white spaces, or contains invalid characters.");
				return;
			}
			catch (PathTooLongException)
			{
				Server.Write("The path provided is too long.");
				return;
			}
			catch (NotSupportedException)
			{
				Server.Write("The path provided is in an invalid format.");
				return;
			}
			catch (Exception)
			{
				// Ignore, any proper exceptions will be presented when the file is written
			}

			ConfigEntry[] registeredConfigs = MultiAdminConfig.GlobalConfig.GetRegisteredConfigs();

			List<string> lines = new(registeredConfigs.Length);
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
