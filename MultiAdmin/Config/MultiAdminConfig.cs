using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiAdmin.Config.ConfigHandler;
using MultiAdmin.ConsoleTools;
using MultiAdmin.ServerIO;
using MultiAdmin.Utility;

namespace MultiAdmin.Config
{
	public class MultiAdminConfig : InheritableConfigRegister
	{
		#region Config Keys and Values

		public ConfigEntry<string> ConfigLocation { get; } =
			new ConfigEntry<string>("config_location", "", false,
				"Config Location", "The default location for the game to use for storing configuration files (a directory)");

		public ConfigEntry<string> AppDataLocation { get; } =
			new ConfigEntry<string>("appdata_location", "",
				"AppData Location", "The location for the game to use for AppData (a directory)");

		public ConfigEntry<bool> DisableConfigValidation { get; } =
			new ConfigEntry<bool>("disable_config_validation", false,
				"Disable Config Validation", "Disable the config validator");

		public ConfigEntry<bool> ShareNonConfigs { get; } =
			new ConfigEntry<bool>("share_non_configs", true,
				"Share Non-Configs", "Makes all files other than the config files store in AppData");

		public ConfigEntry<string> LogLocation { get; } =
			new ConfigEntry<string>("multiadmin_log_location", "logs",
				"MultiAdmin Log Location", "The folder that MultiAdmin will store logs in (a directory)");

		public ConfigEntry<bool> NoLog { get; } =
			new ConfigEntry<bool>("multiadmin_nolog", false,
				"MultiAdmin No-Logging", "Disable logging to file");

		public ConfigEntry<bool> DebugLog { get; } =
			new ConfigEntry<bool>("multiadmin_debug_log", true,
				"MultiAdmin Debug Logging", "Enables MultiAdmin debug logging, this logs to a separate file than any other logs");

		public ConfigEntry<string[]> DebugLogBlacklist { get; } =
			new ConfigEntry<string[]>("multiadmin_debug_log_blacklist", new string[] { nameof(OutputHandler.HandleMessage), nameof(Utils.StringMatches), nameof(ServerSocket.MessageListener) },
				"MultiAdmin Debug Logging Blacklist", "Which tags to block for MultiAdmin debug logging");

		public ConfigEntry<string[]> DebugLogWhitelist { get; } =
			new ConfigEntry<string[]>("multiadmin_debug_log_whitelist", Array.Empty<string>(),
				"MultiAdmin Debug Logging Whitelist", "Which tags to log for MultiAdmin debug logging (Defaults to logging all if none are provided)");

		public ConfigEntry<bool> UseNewInputSystem { get; } =
			new ConfigEntry<bool>("use_new_input_system", true,
				"Use New Input System", "**OBSOLETE: Use `console_input_system` instead, this config option may be removed in a future version of MultiAdmin.** Whether to use the new input system, if false, the original input system will be used");

		public ConfigEntry<InputHandler.ConsoleInputSystem> ConsoleInputSystem { get; } =
			new ConfigEntry<InputHandler.ConsoleInputSystem>("console_input_system", InputHandler.ConsoleInputSystem.New,
				"Console Input System", "Which console input system to use");

		public ConfigEntry<bool> HideInput { get; } =
			new ConfigEntry<bool>("hide_input", false,
				"Hide Console Input", "Whether to hide console input, if true, typed input will not be printed");

		public ConfigEntry<uint> Port { get; } =
			new ConfigEntry<uint>("port", 7777,
				"Game Port", "The port for the server to use");

		public ConfigEntry<string> CopyFromFolderOnReload { get; } =
			new ConfigEntry<string>("copy_from_folder_on_reload", "",
				"Copy from Folder on Reload", "The location of a folder to copy files from into the folder defined by `config_location` whenever the configuration file is reloaded");

		public ConfigEntry<string[]> FolderCopyWhitelist { get; } =
			new ConfigEntry<string[]>("folder_copy_whitelist", Array.Empty<string>(),
				"Folder Copy Whitelist", "The list of file names to copy from the folder defined by `copy_from_folder_on_reload` (accepts `*` wildcards)");

		public ConfigEntry<string[]> FolderCopyBlacklist { get; } =
			new ConfigEntry<string[]>("folder_copy_blacklist", Array.Empty<string>(),
				"Folder Copy Blacklist", "The list of file names to not copy from the folder defined by `copy_from_folder_on_reload` (accepts `*` wildcards)");

		public ConfigEntry<string[]> FolderCopyRoundQueue { get; } =
			new ConfigEntry<string[]>("folder_copy_round_queue", Array.Empty<string>(),
				"Folder Copy Round Queue", "The location of a folder to copy files from into the folder defined by `config_location` after each round, looping through the locations");

		public ConfigEntry<string[]> FolderCopyRoundQueueWhitelist { get; } =
			new ConfigEntry<string[]>("folder_copy_round_queue_whitelist", Array.Empty<string>(),
				"Folder Copy Round Queue Whitelist", "The list of file names to copy from the folders defined by `folder_copy_round_queue` (accepts `*` wildcards)");

		public ConfigEntry<string[]> FolderCopyRoundQueueBlacklist { get; } =
			new ConfigEntry<string[]>("folder_copy_round_queue_blacklist", Array.Empty<string>(),
				"Folder Copy Round Queue Blacklist", "The list of file names to not copy from the folders defined by `folder_copy_round_queue` (accepts `*` wildcards)");

		public ConfigEntry<bool> RandomizeFolderCopyRoundQueue { get; } =
			new ConfigEntry<bool>("randomize_folder_copy_round_queue", false,
				"Randomize Folder Copy Round Queue", "Whether to randomize the order of entries in `folder_copy_round_queue`");

		public ConfigEntry<bool> ManualStart { get; } =
			new ConfigEntry<bool>("manual_start", false,
				"Manual Start", "Whether or not to start the server automatically when launching MultiAdmin");

		public ConfigEntry<decimal> MaxMemory { get; } =
			new ConfigEntry<decimal>("max_memory", 2048,
				"Max Memory", "The amount of memory in megabytes for MultiAdmin to check against");

		public ConfigEntry<decimal> RestartLowMemory { get; } =
			new ConfigEntry<decimal>("restart_low_memory", 400,
				"Restart Low Memory", "Restart if the game's remaining memory falls below this value in megabytes");

		public ConfigEntry<uint> RestartLowMemoryTicks { get; } =
			new ConfigEntry<uint>("restart_low_memory_ticks", 10,
				"Restart Low Memory Ticks", "The number of ticks the memory can be over the limit before restarting");

		public ConfigEntry<decimal> RestartLowMemoryRoundEnd { get; } =
			new ConfigEntry<decimal>("restart_low_memory_roundend", 450,
				"Restart Low Memory Round-End", "Restart at the end of the round if the game's remaining memory falls below this value in megabytes");

		public ConfigEntry<uint> RestartLowMemoryRoundEndTicks { get; } =
			new ConfigEntry<uint>("restart_low_memory_roundend_ticks", 10,
				"Restart Low Memory Round-End Ticks", "The number of ticks the memory can be over the limit before restarting at the end of the round");

		public ConfigEntry<bool> RandomInputColors { get; } =
			new ConfigEntry<bool>("random_input_colors", false,
				"Random Input Colors", "Randomize the new input system's colors every time a message is input");

		public ConfigEntry<int> RestartEveryNumRounds { get; } =
			new ConfigEntry<int>("restart_every_num_rounds", -1,
				"Restart Every Number of Rounds", "Restart the server every number of rounds");

		public ConfigEntry<bool> RestartEveryNumRoundsCounting { get; } =
			new ConfigEntry<bool>("restart_every_num_rounds_counting", false,
				"Restart Every Number of Rounds Counting", "Whether to print the count of rounds passed after each round if the server is set to restart after a number of rounds");

		public ConfigEntry<bool> SafeServerShutdown { get; } =
			new ConfigEntry<bool>("safe_server_shutdown", true,
				"Safe Server Shutdown", "When MultiAdmin closes, if this is true, MultiAdmin will attempt to safely shutdown all servers");

		public ConfigEntry<int> SafeShutdownCheckDelay { get; } =
			new ConfigEntry<int>("safe_shutdown_check_delay", 100,
				"Safe Shutdown Check Delay", "The time in milliseconds between checking if a server is still running when safely shutting down");

		public ConfigEntry<int> SafeShutdownTimeout { get; } =
			new ConfigEntry<int>("safe_shutdown_timeout", 10000,
				"Safe Shutdown Timeout", "The time in milliseconds before MultiAdmin gives up on safely shutting down a server");

		public ConfigEntry<double> ServerRestartTimeout { get; } =
			new ConfigEntry<double>("server_restart_timeout", 10,
				"Server Restart Timeout", "The time in seconds before MultiAdmin forces a server restart if it doesn't respond to the regular restart command");

		public ConfigEntry<double> ServerStopTimeout { get; } =
			new ConfigEntry<double>("server_stop_timeout", 10,
				"Server Stop Timeout", "The time in seconds before MultiAdmin forces a server shutdown if it doesn't respond to the regular shutdown command");

		public ConfigEntry<bool> ServerStartRetry { get; } =
			new ConfigEntry<bool>("server_start_retry", true,
				"Server Start Retry", "Whether to try to start the server again after crashing");

		public ConfigEntry<int> ServerStartRetryDelay { get; } =
			new ConfigEntry<int>("server_start_retry_delay", 10000,
				"Server Start Retry Delay", "The time in milliseconds to wait before trying to start the server again after crashing");

		public ConfigEntry<int> MultiAdminTickDelay { get; } =
			new ConfigEntry<int>("multiadmin_tick_delay", 1000,
				"MultiAdmin Tick Delay", "The time in milliseconds between MultiAdmin ticks (any features that update over time)");

		public ConfigEntry<string> ServersFolder { get; } =
			new ConfigEntry<string>("servers_folder", "servers",
				"Servers Folder", "The location of the `servers` folder for MultiAdmin to load multiple server configurations from");

		public ConfigEntry<bool> SetTitleBar { get; } =
			new ConfigEntry<bool>("set_title_bar", true,
				"Set Title Bar", "Whether to set the console window's titlebar, if false, this feature won't be used");

		public ConfigEntry<string> StartConfigOnFull { get; } =
			new ConfigEntry<string>("start_config_on_full", "",
				"Start Config on Full", "Start server with this config folder once the server becomes full [Requires Modding]");

		#endregion

		public InputHandler.ConsoleInputSystem ActualConsoleInputSystem
		{
			get
			{
				// If defined through execution arguments, use that as an override
				var programInputSystem = Program.ConsoleInputSystem;
				if (programInputSystem != null)
					return (InputHandler.ConsoleInputSystem)programInputSystem;

				if (UseNewInputSystem.Value)
				{
					switch (ConsoleInputSystem.Value)
					{
						case InputHandler.ConsoleInputSystem.New:
							return HideInput.Value ? InputHandler.ConsoleInputSystem.Old : InputHandler.ConsoleInputSystem.New;

						case InputHandler.ConsoleInputSystem.Old:
							return InputHandler.ConsoleInputSystem.Old;
					}
				}

				return InputHandler.ConsoleInputSystem.Original;
			}
		}

		public const string ConfigFileName = "scp_multiadmin.cfg";
		public static readonly string GlobalConfigFilePath = Utils.GetFullPathSafe(ConfigFileName) ?? throw new FileNotFoundException($"Config file \"{nameof(GlobalConfigFilePath)}\" was not set", ConfigFileName);

		public static readonly MultiAdminConfig GlobalConfig = new(GlobalConfigFilePath, null);

		public MultiAdminConfig? ParentConfig
		{
			get => ParentConfigRegister as MultiAdminConfig;
			protected set => ParentConfigRegister = value;
		}

		public Config? Config { get; }

		public MultiAdminConfig(Config? config, MultiAdminConfig? parentConfig, bool createConfig = true)
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
					new ColoredMessage[]
					{
						new ColoredMessage($"Error while creating config (Path = {Config?.ConfigPath ?? "Null"}):",
							ConsoleColor.Red),
						new ColoredMessage(e.ToString(), ConsoleColor.Red)
					}.WriteLines();
				}
			}

			#region MultiAdmin Config Register

			RegisterConfig(ConfigLocation);
			RegisterConfig(AppDataLocation);
			RegisterConfig(DisableConfigValidation);
			RegisterConfig(ShareNonConfigs);
			RegisterConfig(LogLocation);
			RegisterConfig(NoLog);
			RegisterConfig(DebugLog);
			RegisterConfig(DebugLogBlacklist);
			RegisterConfig(DebugLogWhitelist);
			RegisterConfig(UseNewInputSystem);
			RegisterConfig(ConsoleInputSystem);
			RegisterConfig(HideInput);
			RegisterConfig(Port);
			RegisterConfig(CopyFromFolderOnReload);
			RegisterConfig(FolderCopyWhitelist);
			RegisterConfig(FolderCopyBlacklist);
			RegisterConfig(FolderCopyRoundQueue);
			RegisterConfig(FolderCopyRoundQueueWhitelist);
			RegisterConfig(FolderCopyRoundQueueBlacklist);
			RegisterConfig(RandomizeFolderCopyRoundQueue);
			RegisterConfig(ManualStart);
			RegisterConfig(MaxMemory);
			RegisterConfig(RestartLowMemory);
			RegisterConfig(RestartLowMemoryTicks);
			RegisterConfig(RestartLowMemoryRoundEnd);
			RegisterConfig(RestartLowMemoryRoundEndTicks);
			RegisterConfig(RandomInputColors);
			RegisterConfig(RestartEveryNumRounds);
			RegisterConfig(RestartEveryNumRoundsCounting);
			RegisterConfig(SafeServerShutdown);
			RegisterConfig(SafeShutdownCheckDelay);
			RegisterConfig(SafeShutdownTimeout);
			RegisterConfig(ServerRestartTimeout);
			RegisterConfig(ServerStopTimeout);
			RegisterConfig(ServerStartRetry);
			RegisterConfig(ServerStartRetryDelay);
			RegisterConfig(MultiAdminTickDelay);
			RegisterConfig(ServersFolder);
			RegisterConfig(SetTitleBar);
			RegisterConfig(StartConfigOnFull);

			#endregion

			ReloadConfig();
		}

		public MultiAdminConfig(Config? config, bool createConfig = true) : this(config, GlobalConfig, createConfig)
		{
		}

		public MultiAdminConfig(string? path, MultiAdminConfig? parentConfig, bool createConfig = true) : this(
			path != null ? new Config(path) : null, parentConfig, createConfig)
		{
		}

		public MultiAdminConfig(string? path, bool createConfig = true) : this(path, GlobalConfig, createConfig)
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
						config.Value = Config.GetString(config.Key, config.Default) ?? config.Default;
						break;
					}

				case ConfigEntry<string[]> config:
					{
						config.Value = Config.GetStringArray(config.Key, config.Default) ?? config.Default;
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

				case ConfigEntry<double> config:
					{
						config.Value = Config.GetDouble(config.Key, config.Default);
						break;
					}

				case ConfigEntry<decimal> config:
					{
						config.Value = Config.GetDecimal(config.Key, config.Default);
						break;
					}

				case ConfigEntry<bool> config:
					{
						config.Value = Config.GetBool(config.Key, config.Default);
						break;
					}

				case ConfigEntry<InputHandler.ConsoleInputSystem> config:
					{
						config.Value = Config.GetConsoleInputSystem(config.Key, config.Default);
						break;
					}

				default:
					{
						throw new ArgumentException(
							$"Config type unsupported (Config: Key = \"{configEntry.Key ?? "Null"}\" Type = \"{configEntry.ValueType.FullName ?? "Null"}\" Name = \"{configEntry.Name ?? "Null"}\" Description = \"{configEntry.Description ?? "Null"}\").",
							nameof(configEntry));
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
			List<MultiAdminConfig> configHierarchy = new();

			foreach (ConfigRegister configRegister in GetConfigRegisterHierarchy(highestToLowest))
			{
				if (configRegister is MultiAdminConfig config)
					configHierarchy.Add(config);
			}

			return configHierarchy.ToArray();
		}

		public bool ConfigHierarchyContainsPath(string? path)
		{
			string? fullPath = Utils.GetFullPathSafe(path) ?? path;

			return !string.IsNullOrEmpty(fullPath) &&
				   GetConfigHierarchy().Any(config => config.Config?.ConfigPath == path);
		}
	}
}
