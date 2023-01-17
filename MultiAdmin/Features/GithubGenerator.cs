using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MultiAdmin.Config;
using MultiAdmin.Config.ConfigHandler;
using MultiAdmin.ServerIO;
using MultiAdmin.Utility;

namespace MultiAdmin.Features
{
	internal class GithubGenerator : Feature, ICommand
	{
		public const string EmptyIndicator = "**Empty**";
		public const string ColumnSeparator = " | ";

		public GithubGenerator(Server server) : base(server)
		{
		}

		public string GetCommand()
		{
			return "GITHUBGEN";
		}

		public string GetCommandDescription()
		{
			return "Generates a GitHub README file outlining all the features/commands";
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
					path = Path.Combine(path, "README.md");
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

			List<string> lines = new() { "# MultiAdmin", "", "## Features", "" };

			foreach (Feature feature in Server.features)
			{
				lines.Add($"- {feature.GetFeatureName()}: {feature.GetFeatureDescription()}");
			}

			lines.Add("");
			lines.Add("## MultiAdmin Commands");
			lines.Add("");
			foreach (ICommand comm in Server.commands.Values)
			{
				lines.Add($"- {(comm.GetCommand() + " " + comm.GetUsage()).Trim()}: {comm.GetCommandDescription()}");
			}

			lines.Add("");
			lines.Add("## Config Settings");
			lines.Add("");
			lines.Add(
				$"Config Option{ColumnSeparator}Value Type{ColumnSeparator}Default Value{ColumnSeparator}Description");
			lines.Add($"---{ColumnSeparator}:---:{ColumnSeparator}:---:{ColumnSeparator}:------:");

			foreach (ConfigEntry configEntry in MultiAdminConfig.GlobalConfig.GetRegisteredConfigs())
			{
				StringBuilder stringBuilder =
					new($"{configEntry.Key ?? EmptyIndicator}{ColumnSeparator}");

				switch (configEntry)
				{
					case ConfigEntry<string> config:
						{
							stringBuilder.Append(
								$"String{ColumnSeparator}{(string.IsNullOrEmpty(config.Default) ? EmptyIndicator : config.Default)}");
							break;
						}

					case ConfigEntry<string[]> config:
						{
							stringBuilder.Append(
								$"String List{ColumnSeparator}{(config.Default?.IsEmpty() ?? true ? EmptyIndicator : string.Join(", ", config.Default))}");
							break;
						}

					case ConfigEntry<int> config:
						{
							stringBuilder.Append($"Integer{ColumnSeparator}{config.Default}");
							break;
						}

					case ConfigEntry<uint> config:
						{
							stringBuilder.Append($"Unsigned Integer{ColumnSeparator}{config.Default}");
							break;
						}

					case ConfigEntry<float> config:
						{
							stringBuilder.Append($"Float{ColumnSeparator}{config.Default}");
							break;
						}

					case ConfigEntry<double> config:
						{
							stringBuilder.Append($"Double{ColumnSeparator}{config.Default}");
							break;
						}

					case ConfigEntry<decimal> config:
						{
							stringBuilder.Append($"Decimal{ColumnSeparator}{config.Default}");
							break;
						}

					case ConfigEntry<bool> config:
						{
							stringBuilder.Append($"Boolean{ColumnSeparator}{config.Default}");
							break;
						}

					case ConfigEntry<InputHandler.ConsoleInputSystem> config:
						{
							stringBuilder.Append($"[ConsoleInputSystem](#consoleinputsystem){ColumnSeparator}{config.Default}");
							break;
						}

					default:
						{
							stringBuilder.Append(
								$"{configEntry.ValueType?.Name ?? EmptyIndicator}{ColumnSeparator}{configEntry.ObjectDefault ?? EmptyIndicator}");
							break;
						}
				}

				stringBuilder.Append($"{ColumnSeparator}{configEntry.Description ?? EmptyIndicator}");

				lines.Add(stringBuilder.ToString());
			}

			File.WriteAllLines(path, lines);
			Server.Write($"GitHub README written to \"{path}\"");
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
			return "Generates a GitHub README file outlining all the features/commands";
		}

		public override string GetFeatureName()
		{
			return "GitHub Generator";
		}

		public override void Init()
		{
		}
	}
}
