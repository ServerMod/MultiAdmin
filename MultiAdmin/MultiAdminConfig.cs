namespace MultiAdmin
{
	public class MultiAdminConfig
	{
		public const string ManualStartKey = "manual_start";
		public static bool GlobalManualStart => multiAdminConfig.GetBool(ManualStartKey);
		public bool ManualStart => serverConfig != null && (serverConfig.Contains(ManualStartKey)
			                           ? serverConfig.GetBool(ManualStartKey)
			                           : GlobalManualStart);

		public const string StartConfigOnFullKey = "start_config_on_full";
		public static string GlobalStartConfigOnFull => multiAdminConfig.GetString(StartConfigOnFullKey);
		public string StartConfigOnFull => serverConfig != null && serverConfig.Contains(StartConfigOnFullKey)
			? serverConfig.GetString(StartConfigOnFullKey)
			: GlobalStartConfigOnFull;

		public const string ShutdownWhenEmptyForKey = "shutdown_when_empty_for";
		public static int GlobalShutdownWhenEmptyFor => multiAdminConfig.GetInt(ShutdownWhenEmptyForKey, -1);
		public int ShutdownWhenEmptyFor => serverConfig != null && serverConfig.Contains(ShutdownWhenEmptyForKey)
			? serverConfig.GetInt(ShutdownWhenEmptyForKey)
			: GlobalShutdownWhenEmptyFor;

		public const string RestartEveryNumRoundsKey = "restart_every_num_rounds";
		public static int GlobalRestartEveryNumRounds => multiAdminConfig.GetInt(RestartEveryNumRoundsKey, -1);
		public int RestartEveryNumRounds => serverConfig != null && serverConfig.Contains(RestartEveryNumRoundsKey)
			? serverConfig.GetInt(RestartEveryNumRoundsKey)
			: GlobalRestartEveryNumRounds;

		public const string RestartLowMemoryKey = "restart_low_memory";
		public static int GlobalRestartLowMemory => multiAdminConfig.GetInt(RestartLowMemoryKey, 400);
		public int RestartLowMemory => serverConfig != null && serverConfig.Contains(RestartLowMemoryKey)
			? serverConfig.GetInt(RestartLowMemoryKey)
			: GlobalRestartLowMemory;

		public const string RestartLowMemoryRoundEndKey = "restart_low_memory_roundend";
		public static int GlobalRestartLowMemoryRoundEnd => multiAdminConfig.GetInt(RestartLowMemoryRoundEndKey, 450);
		public int RestartLowMemoryRoundEnd =>
			serverConfig != null && serverConfig.Contains(RestartLowMemoryRoundEndKey)
				? serverConfig.GetInt(RestartLowMemoryRoundEndKey)
				: GlobalRestartLowMemoryRoundEnd;

		public const string MaxMemoryKey = "max_memory";
		public static int GlobalMaxMemory => multiAdminConfig.GetInt(MaxMemoryKey, 2048);
		public int MaxMemory => serverConfig != null && serverConfig.Contains(MaxMemoryKey)
			? serverConfig.GetInt(MaxMemoryKey)
			: GlobalMaxMemory;

		public const string NoLogKey = "multiadmin_nolog";
		public static bool GlobalNoLog => multiAdminConfig.GetBool(NoLogKey);
		public bool NoLog => serverConfig != null && serverConfig.Contains(NoLogKey)
			? serverConfig.GetBool(NoLogKey)
			: GlobalNoLog;

		public const string LogModActionsToOwnFileKey = "log_mod_actions_to_own_file";
		public static bool GlobalLogModActionsToOwnFile => multiAdminConfig.GetBool(LogModActionsToOwnFileKey);
		public bool LogModActionsToOwnFile => serverConfig != null && serverConfig.Contains(LogModActionsToOwnFileKey)
			? serverConfig.GetBool(LogModActionsToOwnFileKey)
			: GlobalLogModActionsToOwnFile;

		public const string MaxPlayersKey = "max_players";
		public static int GlobalMaxPlayers => multiAdminConfig.GetInt(MaxPlayersKey, 20);
		public int MaxPlayers => serverConfig != null && serverConfig.Contains(MaxPlayersKey)
			? serverConfig.GetInt(MaxPlayersKey)
			: GlobalMaxPlayers;

		public const string DisableConfigValidationKey = "disable_config_validation";
		public static bool GlobalDisableConfigValidation => multiAdminConfig.GetBool(DisableConfigValidationKey);
		public bool DisableConfigValidation => serverConfig != null && serverConfig.Contains(DisableConfigValidationKey)
			? serverConfig.GetBool(DisableConfigValidationKey)
			: GlobalDisableConfigValidation;

		public const string ShareNonConfigsKey = "share_non_configs";
		public static bool GlobalShareNonConfigs => multiAdminConfig.GetBool(ShareNonConfigsKey);
		public bool ShareNonConfigs => serverConfig != null && serverConfig.Contains(ShareNonConfigsKey)
			? serverConfig.GetBool(ShareNonConfigsKey)
			: GlobalShareNonConfigs;

		public const string ConfigLocationKey = "config_location";
		public static string GlobalConfigLocation => multiAdminConfig.GetString(ConfigLocationKey);
		public string ConfigLocation => serverConfig != null && serverConfig.Contains(ConfigLocationKey)
			? serverConfig.GetString(ConfigLocationKey)
			: GlobalConfigLocation;

		public const string ServersFolderKey = "servers_folder";
		public static string GlobalServersFolder => multiAdminConfig.GetString(ServersFolderKey);
		public string ServersFolder => serverConfig != null && serverConfig.Contains(ServersFolderKey)
			? serverConfig.GetString(ServersFolderKey)
			: GlobalServersFolder;

		public static Config multiAdminConfig = new Config("scp_multiadmin.cfg");
		public Config serverConfig;

		public MultiAdminConfig(string path)
		{
			serverConfig = new Config(path);
		}
	}
}