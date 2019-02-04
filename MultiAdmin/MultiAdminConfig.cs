namespace MultiAdmin
{
	public class MultiAdminConfig
	{
		/*
		 * For each config here, there is a static global value and an instance-based server value,
		 * the server config checks whether it's valid before attempting to return any value from it, otherwise
		 * it returns the static global value.
		 */

		public const string ManualStartKey = "manual_start";
		public static bool GlobalManualStart => GlobalConfig.GetBool(ManualStartKey);

		public bool ManualStart => ServerConfigContains(ManualStartKey)
			? serverConfig.GetBool(ManualStartKey)
			: GlobalManualStart;

		public const string StartConfigOnFullKey = "start_config_on_full";
		public static string GlobalStartConfigOnFull => GlobalConfig.GetString(StartConfigOnFullKey);

		public string StartConfigOnFull => ServerConfigContains(StartConfigOnFullKey)
			? serverConfig.GetString(StartConfigOnFullKey)
			: GlobalStartConfigOnFull;

		public const string ShutdownWhenEmptyForKey = "shutdown_when_empty_for";
		public static int GlobalShutdownWhenEmptyFor => GlobalConfig.GetInt(ShutdownWhenEmptyForKey, -1);

		public int ShutdownWhenEmptyFor => ServerConfigContains(ShutdownWhenEmptyForKey)
			? serverConfig.GetInt(ShutdownWhenEmptyForKey)
			: GlobalShutdownWhenEmptyFor;

		public const string RestartEveryNumRoundsKey = "restart_every_num_rounds";
		public static int GlobalRestartEveryNumRounds => GlobalConfig.GetInt(RestartEveryNumRoundsKey, -1);

		public int RestartEveryNumRounds => ServerConfigContains(RestartEveryNumRoundsKey)
			? serverConfig.GetInt(RestartEveryNumRoundsKey)
			: GlobalRestartEveryNumRounds;

		public const string RestartLowMemoryKey = "restart_low_memory";
		public static int GlobalRestartLowMemory => GlobalConfig.GetInt(RestartLowMemoryKey, 400);

		public int RestartLowMemory => ServerConfigContains(RestartLowMemoryKey)
			? serverConfig.GetInt(RestartLowMemoryKey)
			: GlobalRestartLowMemory;

		public const string RestartLowMemoryRoundEndKey = "restart_low_memory_roundend";
		public static int GlobalRestartLowMemoryRoundEnd => GlobalConfig.GetInt(RestartLowMemoryRoundEndKey, 450);

		public int RestartLowMemoryRoundEnd =>
			ServerConfigContains(RestartLowMemoryRoundEndKey)
				? serverConfig.GetInt(RestartLowMemoryRoundEndKey)
				: GlobalRestartLowMemoryRoundEnd;

		public const string MaxMemoryKey = "max_memory";
		public static int GlobalMaxMemory => GlobalConfig.GetInt(MaxMemoryKey, 2048);

		public int MaxMemory => ServerConfigContains(MaxMemoryKey)
			? serverConfig.GetInt(MaxMemoryKey)
			: GlobalMaxMemory;

		public const string NoLogKey = "multiadmin_nolog";
		public static bool GlobalNoLog => GlobalConfig.GetBool(NoLogKey);

		public bool NoLog => ServerConfigContains(NoLogKey)
			? serverConfig.GetBool(NoLogKey)
			: GlobalNoLog;

		public const string LogModActionsToOwnFileKey = "log_mod_actions_to_own_file";
		public static bool GlobalLogModActionsToOwnFile => GlobalConfig.GetBool(LogModActionsToOwnFileKey);

		public bool LogModActionsToOwnFile => ServerConfigContains(LogModActionsToOwnFileKey)
			? serverConfig.GetBool(LogModActionsToOwnFileKey)
			: GlobalLogModActionsToOwnFile;

		public const string MaxPlayersKey = "max_players";
		public static int GlobalMaxPlayers => GlobalConfig.GetInt(MaxPlayersKey, 20);

		public int MaxPlayers => ServerConfigContains(MaxPlayersKey)
			? serverConfig.GetInt(MaxPlayersKey)
			: GlobalMaxPlayers;

		public const string DisableConfigValidationKey = "disable_config_validation";
		public static bool GlobalDisableConfigValidation => GlobalConfig.GetBool(DisableConfigValidationKey);

		public bool DisableConfigValidation => ServerConfigContains(DisableConfigValidationKey)
			? serverConfig.GetBool(DisableConfigValidationKey)
			: GlobalDisableConfigValidation;

		public const string ShareNonConfigsKey = "share_non_configs";
		public static bool GlobalShareNonConfigs => GlobalConfig.GetBool(ShareNonConfigsKey, true);

		public bool ShareNonConfigs => ServerConfigContains(ShareNonConfigsKey)
			? serverConfig.GetBool(ShareNonConfigsKey)
			: GlobalShareNonConfigs;

		public const string ConfigLocationKey = "config_location";
		public static string GlobalConfigLocation => GlobalConfig.GetString(ConfigLocationKey);

		public string ConfigLocation => ServerConfigContains(ConfigLocationKey)
			? serverConfig.GetString(ConfigLocationKey)
			: GlobalConfigLocation;

		public const string ServersFolderKey = "servers_folder";
		public static string GlobalServersFolder => GlobalConfig.GetString(ServersFolderKey, "servers");

		public string ServersFolder => ServerConfigContains(ServersFolderKey)
			? serverConfig.GetString(ServersFolderKey)
			: GlobalServersFolder;

		public const string ConfigFileName = "scp_multiadmin.cfg";

		public static readonly Config GlobalConfig = new Config(ConfigFileName);
		public readonly Config serverConfig;

		/// <summary>
		///     Creates a <see cref="MultiAdminConfig" /> object with a null <see cref="serverConfig" />. This object will always
		///     return the <see cref="GlobalConfig" />'s value.
		/// </summary>
		public MultiAdminConfig()
		{
		}

		public MultiAdminConfig(Config config)
		{
			serverConfig = config;
		}

		public MultiAdminConfig(string path) : this(new Config(path))
		{
		}

		public static void ReloadGlobalConfig()
		{
			GlobalConfig.ReadConfigFile();
		}

		public void ReloadConfig()
		{
			ReloadGlobalConfig();
			serverConfig?.ReadConfigFile();
		}

		public bool ServerConfigContains(string key)
		{
			return serverConfig != null && serverConfig.Contains(key);
		}
	}
}