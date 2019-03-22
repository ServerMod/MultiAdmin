using System;
using System.IO;
using MultiAdmin.ConsoleTools;

namespace MultiAdmin.Config
{
	public class MultiAdminConfig
	{
		#region Config Keys and Values

		public const string ConfigLocationKey = "config_location";
		public string ConfigLocation { get; private set; }

		public const string DisableConfigValidationKey = "disable_config_validation";
		public bool DisableConfigValidation { get; private set; }

		public const string ShareNonConfigsKey = "share_non_configs";
		public bool ShareNonConfigs { get; private set; }

		public const string NoLogKey = "multiadmin_nolog";
		public bool NoLog { get; private set; }

		public const string DebugLogKey = "multiadmin_debug_log";
		public bool DebugLog { get; private set; }

		public const string DebugLogBlacklistKey = "multiadmin_debug_log_blacklist";
		public string[] DebugLogBlacklist { get; private set; }

		public const string DebugLogWhitelistKey = "multiadmin_debug_log_whitelist";
		public string[] DebugLogWhitelist { get; private set; }

		public const string PortKey = "port";
		public uint Port { get; private set; }

		public const string CopyFromFolderOnReloadKey = "copy_from_folder_on_reload";
		public string CopyFromFolderOnReload { get; private set; }

		public const string FilesToCopyFromFolderKey = "files_to_copy_from_folder";
		public string[] FilesToCopyFromFolder { get; private set; }

		public const string FolderCopyRoundQueueKey = "folder_copy_round_queue";
		public string[] FolderCopyRoundQueue { get; private set; }

		public const string RandomizeFolderCopyRoundQueueKey = "randomize_folder_copy_round_queue";
		public bool RandomizeFolderCopyRoundQueue { get; private set; }

		public const string LogModActionsToOwnFileKey = "log_mod_actions_to_own_file";
		public bool LogModActionsToOwnFile { get; private set; }

		public const string ManualStartKey = "manual_start";
		public bool ManualStart { get; private set; }

		public const string MaxMemoryKey = "max_memory";
		public float MaxMemory { get; private set; }

		public const string RestartLowMemoryKey = "restart_low_memory";
		public float RestartLowMemory { get; private set; }

		public const string RestartLowMemoryRoundEndKey = "restart_low_memory_roundend";
		public float RestartLowMemoryRoundEnd { get; private set; }

		public const string MaxPlayersKey = "max_players";
		public int MaxPlayers { get; private set; }

		public const string RandomInputColorsKey = "random_input_colors";
		public bool RandomInputColors { get; private set; }

		public const string RestartEveryNumRoundsKey = "restart_every_num_rounds";
		public int RestartEveryNumRounds { get; private set; }

		public const string ServerRestartTimeoutKey = "server_restart_timeout";
		public float ServerRestartTimeout { get; private set; }

		public const string ServerStopTimeoutKey = "server_stop_timeout";
		public float ServerStopTimeout { get; private set; }

		public const string ServersFolderKey = "servers_folder";
		public string ServersFolder { get; private set; }

		public const string ShutdownWhenEmptyForKey = "shutdown_when_empty_for";
		public int ShutdownWhenEmptyFor { get; private set; }

		public const string StartConfigOnFullKey = "start_config_on_full";
		public string StartConfigOnFull { get; private set; }

		#endregion

		public const string ConfigFileName = "scp_multiadmin.cfg";
		public static readonly string ConfigFilePath = Utils.GetFullPathSafe(ConfigFileName);

		public static readonly MultiAdminConfig GlobalConfig = new MultiAdminConfig(ConfigFilePath, null);

		public MultiAdminConfig ParentConfig { get; }
		public Config Config { get; }

		public MultiAdminConfig(Config config, MultiAdminConfig parentConfig)
		{
			Config = config;
			ParentConfig = parentConfig;

			if (!File.Exists(Config?.ConfigPath))
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
						new ColoredMessage($"Error while creating config (Path = {Config?.ConfigPath ?? "Null"}):", ConsoleColor.Red),
						new ColoredMessage(e.ToString(), ConsoleColor.Red)
					}.WriteLines();
				}
			}

			ReloadConfig();
		}

		public MultiAdminConfig(Config config) : this(config, GlobalConfig)
		{
		}

		public MultiAdminConfig(string path, MultiAdminConfig parentConfig) : this(new Config(path), parentConfig)
		{
		}

		public MultiAdminConfig(string path) : this(path, GlobalConfig)
		{
		}

		public void ReloadConfig()
		{
			ParentConfig?.ReloadConfig();
			Config?.ReadConfigFile();

			ConfigLocation = ShouldGetFromConfig(ConfigLocationKey) ? Config.GetString(ConfigLocationKey, "") : ParentConfig.ConfigLocation;
			DisableConfigValidation = ShouldGetFromConfig(DisableConfigValidationKey) ? Config.GetBool(DisableConfigValidationKey, false) : ParentConfig.DisableConfigValidation;
			ShareNonConfigs = ShouldGetFromConfig(ShareNonConfigsKey) ? Config.GetBool(ShareNonConfigsKey, true) : ParentConfig.ShareNonConfigs;
			NoLog = ShouldGetFromConfig(NoLogKey) ? Config.GetBool(NoLogKey, false) : ParentConfig.NoLog;
			DebugLog = ShouldGetFromConfig(DebugLogKey) ? Config.GetBool(DebugLogKey, false) : ParentConfig.DebugLog;
			DebugLogBlacklist = ShouldGetFromConfig(DebugLogBlacklistKey) ? Config.GetStringList(DebugLogBlacklistKey, new string[] {"ProcessFile"}) : ParentConfig.DebugLogBlacklist;
			DebugLogWhitelist = ShouldGetFromConfig(DebugLogWhitelistKey) ? Config.GetStringList(DebugLogWhitelistKey, new string[0]) : ParentConfig.DebugLogWhitelist;
			Port = ShouldGetFromConfig(PortKey) ? Config.GetUInt(PortKey, 7777) : ParentConfig.Port;
			CopyFromFolderOnReload = ShouldGetFromConfig(CopyFromFolderOnReloadKey) ? Config.GetString(CopyFromFolderOnReloadKey, "") : ParentConfig.CopyFromFolderOnReload;
			FilesToCopyFromFolder = ShouldGetFromConfig(FilesToCopyFromFolderKey) ? Config.GetStringList(FilesToCopyFromFolderKey, new string[0]) : ParentConfig.FilesToCopyFromFolder;
			FolderCopyRoundQueue = ShouldGetFromConfig(FolderCopyRoundQueueKey) ? Config.GetStringList(FolderCopyRoundQueueKey, new string[0]) : ParentConfig.FolderCopyRoundQueue;
			RandomizeFolderCopyRoundQueue = ShouldGetFromConfig(RandomizeFolderCopyRoundQueueKey) ? Config.GetBool(RandomizeFolderCopyRoundQueueKey, false) : ParentConfig.RandomizeFolderCopyRoundQueue;
			LogModActionsToOwnFile = ShouldGetFromConfig(LogModActionsToOwnFileKey) ? Config.GetBool(LogModActionsToOwnFileKey, false) : ParentConfig.LogModActionsToOwnFile;
			ManualStart = ShouldGetFromConfig(ManualStartKey) ? Config.GetBool(ManualStartKey, false) : ParentConfig.ManualStart;
			MaxMemory = ShouldGetFromConfig(MaxMemoryKey) ? Config.GetFloat(MaxMemoryKey, 2048) : ParentConfig.MaxMemory;
			RestartLowMemory = ShouldGetFromConfig(RestartLowMemoryKey) ? Config.GetFloat(RestartLowMemoryKey, 400) : ParentConfig.RestartLowMemory;
			RestartLowMemoryRoundEnd = ShouldGetFromConfig(RestartLowMemoryRoundEndKey) ? Config.GetFloat(RestartLowMemoryRoundEndKey, 450) : ParentConfig.RestartLowMemoryRoundEnd;
			MaxPlayers = ShouldGetFromConfig(MaxPlayersKey) ? Config.GetInt(MaxPlayersKey, 20) : ParentConfig.MaxPlayers;
			RandomInputColors = ShouldGetFromConfig(RandomInputColorsKey) ? Config.GetBool(RandomInputColorsKey, false) : ParentConfig.RandomInputColors;
			RestartEveryNumRounds = ShouldGetFromConfig(RestartEveryNumRoundsKey) ? Config.GetInt(RestartEveryNumRoundsKey, -1) : ParentConfig.RestartEveryNumRounds;
			ServerRestartTimeout = ShouldGetFromConfig(ServerRestartTimeoutKey) ? Config.GetFloat(ServerRestartTimeoutKey, 10) : ParentConfig.ServerRestartTimeout;
			ServerStopTimeout = ShouldGetFromConfig(ServerStopTimeoutKey) ? Config.GetFloat(ServerStopTimeoutKey, 10) : ParentConfig.ServerStopTimeout;
			ServersFolder = ShouldGetFromConfig(ServersFolderKey) ? Config.GetString(ServersFolderKey, "servers") : ParentConfig.ServersFolder;
			ShutdownWhenEmptyFor = ShouldGetFromConfig(ShutdownWhenEmptyForKey) ? Config.GetInt(ShutdownWhenEmptyForKey, -1) : ParentConfig.ShutdownWhenEmptyFor;
			StartConfigOnFull = ShouldGetFromConfig(StartConfigOnFullKey) ? Config.GetString(StartConfigOnFullKey, "") : ParentConfig.StartConfigOnFull;
		}

		private bool ShouldGetFromConfig(string key)
		{
			return ParentConfig == null || ConfigContains(key);
		}

		public bool ConfigContains(string key)
		{
			return Config != null && Config.Contains(key);
		}

		public bool ConfigOrGlobalConfigContains(string key)
		{
			return ConfigContains(key) || GlobalConfig.ConfigContains(key);
		}
	}
}
