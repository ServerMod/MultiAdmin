using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MultiAdmin.Config;
using MultiAdmin.Config.ConfigHandler;
using MultiAdmin.Features.Attributes;

namespace MultiAdmin.Features
{
	[Feature]
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
			return "Generates a github .md file outlining all the features/commands";
		}

		public string GetUsage()
		{
			return "[FILE LOCATION]";
		}

		public void OnCall(string[] args)
		{
			if (!args.Any())
			{
				Server.Write("You must specify the location of the file.");
				return;
			}

			string dir = string.Join(" ", args);

			List<string> lines = new List<string> {"# MultiAdmin", string.Empty, "## Features"};

			foreach (Feature feature in Server.features)
			{
				if (feature.Equals(this)) continue;

				lines.Add($"- {feature.GetFeatureName()}: {feature.GetFeatureDescription()}");
			}

			lines.Add(string.Empty);
			lines.Add("## MultiAdmin Commands");
			lines.Add(string.Empty);
			foreach (ICommand comm in Server.commands.Values)
			{
				lines.Add($"- {(comm.GetCommand() + " " + comm.GetUsage()).Trim()}: {comm.GetCommandDescription()}");
			}

			lines.Add(string.Empty);
			lines.Add("## Config Settings");
			lines.Add(string.Empty);
			lines.Add($"Config Option{ColumnSeparator}Value Type{ColumnSeparator}Default Value{ColumnSeparator}Description");
			lines.Add($"---{ColumnSeparator}:---:{ColumnSeparator}:---:{ColumnSeparator}:------:");

			foreach (ConfigEntry configEntry in MultiAdminConfig.GlobalConfig.GetRegisteredConfigs())
			{
				StringBuilder stringBuilder = new StringBuilder($"{configEntry.Key ?? EmptyIndicator}{ColumnSeparator}");

				switch (configEntry)
				{
					case ConfigEntry<string> config:
					{
						stringBuilder.Append($"String{ColumnSeparator}{(string.IsNullOrEmpty(config.Default) ? EmptyIndicator : config.Default)}");
						break;
					}

					case ConfigEntry<string[]> config:
					{
						stringBuilder.Append($"String List{ColumnSeparator}{(!config.Default?.Any() ?? true ? EmptyIndicator : string.Join(", ", config.Default))}");
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

					default:
					{
						stringBuilder.Append($"{configEntry.ValueType?.Name ?? EmptyIndicator}{ColumnSeparator}{configEntry.ObjectDefault ?? EmptyIndicator}");
						break;
					}
				}

				stringBuilder.Append($"{ColumnSeparator}{configEntry.Description ?? EmptyIndicator}");

				lines.Add(stringBuilder.ToString());
			}

			File.WriteAllLines(dir, lines);
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
			return "NOT INCLUDED IN FILE";
		}

		public override string GetFeatureName()
		{
			return "GITHUB GEN";
		}

		public override void Init()
		{
		}
	}
}
