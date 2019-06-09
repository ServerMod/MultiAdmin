using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MultiAdmin.Config.ConfigHandler;
using MultiAdmin.ConsoleTools;

namespace MultiAdmin.Config
{
	public class MultiAdminConfig : InheritableConfigRegister
	{
		#region Config Keys and Values

		public ConfigEntry<string> ConfigLocation { get; } =
			new ConfigEntry<string>("config_location", "", false,
				"Config Location", "The default location for the game to use for storing configuration files (a directory)");

		public ConfigEntry<bool> DisableConfigValidation { get; } =
			new ConfigEntry<bool>("disable_config_validation", false,
				"Disable Config Validation", "Disable the config validator");

		public ConfigEntry<bool> ShareNonConfigs { get; } =
			new ConfigEntry<bool>("share_non_configs", true,
				"Share Non-Configs", "Makes all files other than the config files store in AppData");

		public ConfigEntry<bool> NoLog { get; } =
			new ConfigEntry<bool>("multiadmin_nolog", false,
				"MultiAdmin No-Logging", "Disable logging to file");

		public ConfigEntry<bool> DebugLog { get; } =
			new ConfigEntry<bool>("multiadmin_debug_log", true,
				"MultiAdmin Debug Logging", "Enables MultiAdmin debug logging, this logs to a separate file than any other logs");

		public ConfigEntry<string[]> DebugLogBlacklist { get; } =
			new ConfigEntry<string[]>("multiadmin_debug_log_blacklist", new string[] {"ProcessFile"},
				"MultiAdmin Debug Logging Blacklist", "Which tags to block for MultiAdmin debug logging");

		public ConfigEntry<string[]> DebugLogWhitelist { get; } =
			new ConfigEntry<string[]>("multiadmin_debug_log_whitelist", new string[0],
				"MultiAdmin Debug Logging Whitelist", "Which tags to log for MultiAdmin debug logging (Defaults to logging all if none are provided)");

		public ConfigEntry<bool> UseNewInputSystem { get; } =
			new ConfigEntry<bool>("use_new_input_system", true,
				"Use New Input System", "Whether to use the new input system, if false, the original input system will be used");

		public ConfigEntry<uint> Port { get; } =
			new ConfigEntry<uint>("port", 7777,
				"Game Port", "The port for the server to use");

		public ConfigEntry<string> CopyFromFolderOnReload { get; } =
			new ConfigEntry<string>("copy_from_folder_on_reload", "",
				"Copy from Folder on Reload", "The location of a folder to copy files from into the folder defined by \"config_location\" whenever the configuration file is reloaded");

		public ConfigEntry<string[]> FolderCopyWhitelist { get; } =
			new ConfigEntry<string[]>("folder_copy_whitelist", new string[0],
				"Folder Copy Whitelist", "The list of file names to copy from the folder defined by \"copy_from_folder_on_reload\" (accepts \"*\" wildcards)");

		public ConfigEntry<string[]> FolderCopyBlacklist { get; } =
			new ConfigEntry<string[]>("folder_copy_blacklist", new string[0],
				"Folder Copy Blacklist", "The list of file names to not copy from the folder defined by \"copy_from_folder_on_reload\" (accepts \"*\" wildcards)");

		public ConfigEntry<string[]> FolderCopyRoundQueue { get; } =
			new ConfigEntry<string[]>("folder_copy_round_queue", new string[0],
				"Folder Copy Round Queue", "The location of a folder to copy files from into the folder defined by \"config_location\" after each round, looping through the locations");

		public ConfigEntry<string[]> FolderCopyRoundQueueWhitelist { get; } =
			new ConfigEntry<string[]>("folder_copy_round_queue_whitelist", new string[0],
				"Folder Copy Round Queue Whitelist", "The list of file names to copy from the folders defined by \"folder_copy_round_queue\" (accepts \"*\" wildcards)");

		public ConfigEntry<string[]> FolderCopyRoundQueueBlacklist { get; } =
			new ConfigEntry<string[]>("folder_copy_round_queue_blacklist", new string[0],
				"Folder Copy Round Queue Blacklist", "The list of file names to not copy from the folders defined by \"folder_copy_round_queue\" (accepts \"*\" wildcards)");

		public ConfigEntry<bool> RandomizeFolderCopyRoundQueue { get; } =
			new ConfigEntry<bool>("randomize_folder_copy_round_queue", false,
				"Randomize Folder Copy Round Queue", "Whether to randomize the order of entries in \"folder_copy_round_queue\"");

		public ConfigEntry<bool> LogModActionsToOwnFile { get; } =
			new ConfigEntry<bool>("log_mod_actions_to_own_file", false,
				"Log Mod Actions to Own File", "Logs admin messages to separate file");

		public ConfigEntry<bool> ManualStart { get; } =
			new ConfigEntry<bool>("manual_start", false,
				"Manual Start", "Whether or not to start the server automatically when launching MultiAdmin");

		public ConfigEntry<float> MaxMemory { get; } =
			new ConfigEntry<float>("max_memory", 2048,
				"Max Memory", "The amount of memory in megabytes for MultiAdmin to check against");

		public ConfigEntry<float> RestartLowMemory { get; } =
			new ConfigEntry<float>("restart_low_memory", 400,
				"Restart Low Memory", "Restart if the game's remaining memory falls below this value in megabytes");

		public ConfigEntry<float> RestartLowMemoryRoundEnd { get; } =
			new ConfigEntry<float>("restart_low_memory_roundend", 450,
				"Restart Low Memory Round-End", "Restart at the end of the round if the game's remaining memory falls below this value in megabytes");

		public ConfigEntry<int> MaxPlayers { get; } =
			new ConfigEntry<int>("max_players", 20,
				"Max Players", "The number of players to display as the maximum for the server (within MultiAdmin, not in-game)");

		public ConfigEntry<bool> RandomInputColors { get; } =
			new ConfigEntry<bool>("random_input_colors", false,
				"Random Input Colors", "Randomize the new input system's colors every time a message is input");

		public ConfigEntry<int> RestartEveryNumRounds { get; } =
			new ConfigEntry<int>("restart_every_num_rounds", -1,
				"Restart Every Number of Rounds", "Restart the server every number of rounds");

		public ConfigEntry<bool> SafeServerShutdown { get; } =
			new ConfigEntry<bool>("safe_server_shutdown", true,
				"Safe Server Shutdown", "When MultiAdmin closes, if this is true, MultiAdmin will attempt to safely shutdown all the servers");

		public ConfigEntry<float> ServerRestartTimeout { get; } =
			new ConfigEntry<float>("server_restart_timeout", 10,
				"Server Restart Timeout", "The time in seconds before MultiAdmin forces a server restart if it doesn't respond to the regular restart command");

		public ConfigEntry<float> ServerStopTimeout { get; } =
			new ConfigEntry<float>("server_stop_timeout", 10,
				"Server Stop Timeout", "The time in seconds before MultiAdmin forces a server shutdown if it doesn't respond to the regular shutdown command");

		public ConfigEntry<string> ServersFolder { get; } =
			new ConfigEntry<string>("servers_folder", "servers",
				"Servers Folder", "The location of the \"servers\" folder for MultiAdmin to load multiple server configurations from");

		public ConfigEntry<bool> SetTitleBar { get; } =
			new ConfigEntry<bool>("set_title_bar", true,
				"Set Title Bar", "Whether to set the console window's titlebar, if false, this feature won't be used");

		public ConfigEntry<int> ShutdownWhenEmptyFor { get; } =
			new ConfigEntry<int>("shutdown_when_empty_for", -1,
				"Shutdown When Empty For", "Shutdown the server once a round hasn't started in a number of seconds");

		public ConfigEntry<string> StartConfigOnFull { get; } =
			new ConfigEntry<string>("start_config_on_full", "",
				"Start Config on Full", "Start server with this config folder once the server becomes full [Requires ServerMod]");

		#endregion

		public const string ConfigFileName = "scp_multiadmin.cfg";
		public static readonly string GlobalConfigFilePath = Utils.GetFullPathSafe(ConfigFileName);

		public static readonly MultiAdminConfig GlobalConfig = new MultiAdminConfig(GlobalConfigFilePath, null);

		public MultiAdminConfig ParentConfig
		{
			get => ParentConfigRegister as MultiAdminConfig;
			protected set => ParentConfigRegister = value;
		}

		public Config Config { get; }

		public MultiAdminConfig(Config config, MultiAdminConfig parentConfig, bool createConfig = true)
		{
			Config = config;
			ParentConfig = parentConfig;

			if (createConfig && !File.Exists(Config?.ConfigPath))
			{
				try
				{
					if (Config?.ConfigPath != null)
						File.Create(Config.ConfigPath).Close();
				}
				catch (Exception e)
				{
					new ColoredMessage[] {new ColoredMessage($"Error while creating config (Path = {Config?.ConfigPath ?? "Null"}):", ConsoleColor.Red), new ColoredMessage(e.ToString(), ConsoleColor.Red)}.WriteLines();
				}
			}

			#region MultiAdmin Config Register

			foreach (PropertyInfo property in GetType().GetProperties())
			{
				if (property.GetValue(this) is ConfigEntry entry)
				{
					RegisterConfig(entry);
				}
			}

			#endregion

			ReloadConfig();
		}

		public MultiAdminConfig(Config config, bool createConfig = true) : this(config, GlobalConfig, createConfig)
		{
		}

		public MultiAdminConfig(string path, MultiAdminConfig parentConfig, bool createConfig = true) : this(new Config(path), parentConfig, createConfig)
		{
		}

		public MultiAdminConfig(string path, bool createConfig = true) : this(path, GlobalConfig, createConfig)
		{
		}

		#region Config Registration

		public override void UpdateConfigValueInheritable(ConfigEntry configEntry)
		{
			if (configEntry == null)
				throw new NullReferenceException("Config type unsupported (Config: Null).");

			if (Config == null)
			{
				configEntry.ObjectValue = configEntry.ObjectDefault;
				return;
			}

			switch (configEntry)
			{
				case ConfigEntry<string> config:
				{
					config.Value = Config.GetString(config.Key, config.Default);
					break;
				}

				case ConfigEntry<string[]> config:
				{
					config.Value = Config.GetStringArray(config.Key, config.Default);
					break;
				}

				case ConfigEntry<int> config:
				{
					config.Value = Config.GetInt(config.Key, config.Default);
					break;
				}

				case ConfigEntry<uint> config:
				{
					config.Value = Config.GetUInt(config.Key, config.Default);
					break;
				}

				case ConfigEntry<float> config:
				{
					config.Value = Config.GetFloat(config.Key, config.Default);
					break;
				}

				case ConfigEntry<bool> config:
				{
					config.Value = Config.GetBool(config.Key, config.Default);
					break;
				}

				default:
				{
					throw new ArgumentException($"Config type unsupported (Config: Key = \"{configEntry.Key ?? "Null"}\" Type = \"{configEntry.ValueType.FullName ?? "Null"}\" Name = \"{configEntry.Name ?? "Null"}\" Description = \"{configEntry.Description ?? "Null"}\").", nameof(configEntry));
				}
			}
		}

		public override bool ShouldInheritConfigEntry(ConfigEntry configEntry)
		{
			return !ConfigContains(configEntry.Key);
		}

		#endregion

		public void ReloadConfig()
		{
			ParentConfig?.ReloadConfig();
			Config?.ReadConfigFile();

			UpdateRegisteredConfigValues();
		}

		public bool ConfigContains(string key)
		{
			return Config != null && Config.Contains(key);
		}

		public bool ConfigOrGlobalConfigContains(string key)
		{
			return ConfigContains(key) || GlobalConfig.ConfigContains(key);
		}

		public MultiAdminConfig[] GetConfigHierarchy(bool highestToLowest = true)
		{
			List<MultiAdminConfig> configHierarchy = new List<MultiAdminConfig>();

			foreach (InheritableConfigRegister configRegister in GetConfigRegisterHierarchy(highestToLowest))
			{
				if (configRegister is MultiAdminConfig config)
					configHierarchy.Add(config);
			}

			return configHierarchy.ToArray();
		}

		public bool ConfigHierarchyContainsPath(string path)
		{
			string fullPath = Utils.GetFullPathSafe(path);

			return !string.IsNullOrEmpty(fullPath) && GetConfigHierarchy().Any(config => config.Config?.ConfigPath == path);
		}
	}
}
