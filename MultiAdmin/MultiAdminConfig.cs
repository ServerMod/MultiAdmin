using System.IO;

namespace MultiAdmin
{
	public class MultiAdminConfig
	{
		/*
		 * For each config here, there is a static global value and an instance-based server value,
		 * the server config checks whether it's valid before attempting to return any value from it, otherwise
		 * it returns the static global value.
		 */

		#region Manual Start

		public const string ManualStartKey = "manual_start";
		public static bool GlobalManualStart => GlobalConfig.GetBool(ManualStartKey, false);

		public bool ManualStart => ServerConfigContains(ManualStartKey)
			? serverConfig.GetBool(ManualStartKey)
			: GlobalManualStart;

		#endregion

		#region Start Config On Full

		public const string StartConfigOnFullKey = "start_config_on_full";
		public static string GlobalStartConfigOnFull => GlobalConfig.GetString(StartConfigOnFullKey, "");

		public string StartConfigOnFull => ServerConfigContains(StartConfigOnFullKey)
			? serverConfig.GetString(StartConfigOnFullKey)
			: GlobalStartConfigOnFull;

		#endregion

		#region Shutdown When Empty For

		public const string ShutdownWhenEmptyForKey = "shutdown_when_empty_for";
		public static int GlobalShutdownWhenEmptyFor => GlobalConfig.GetInt(ShutdownWhenEmptyForKey, -1);

		public int ShutdownWhenEmptyFor => ServerConfigContains(ShutdownWhenEmptyForKey)
			? serverConfig.GetInt(ShutdownWhenEmptyForKey)
			: GlobalShutdownWhenEmptyFor;

		#endregion

		#region Restart Every Num Rounds

		public const string RestartEveryNumRoundsKey = "restart_every_num_rounds";
		public static int GlobalRestartEveryNumRounds => GlobalConfig.GetInt(RestartEveryNumRoundsKey, -1);

		public int RestartEveryNumRounds => ServerConfigContains(RestartEveryNumRoundsKey)
			? serverConfig.GetInt(RestartEveryNumRoundsKey)
			: GlobalRestartEveryNumRounds;

		#endregion

		#region Restart Low Memory

		public const string RestartLowMemoryKey = "restart_low_memory";
		public static float GlobalRestartLowMemory => GlobalConfig.GetFloat(RestartLowMemoryKey, 400);

		public float RestartLowMemory => ServerConfigContains(RestartLowMemoryKey)
			? serverConfig.GetFloat(RestartLowMemoryKey)
			: GlobalRestartLowMemory;

		#endregion

		#region Restart Low Memory Round End

		public const string RestartLowMemoryRoundEndKey = "restart_low_memory_roundend";
		public static float GlobalRestartLowMemoryRoundEnd => GlobalConfig.GetFloat(RestartLowMemoryRoundEndKey, 450);

		public float RestartLowMemoryRoundEnd =>
			ServerConfigContains(RestartLowMemoryRoundEndKey)
				? serverConfig.GetFloat(RestartLowMemoryRoundEndKey)
				: GlobalRestartLowMemoryRoundEnd;

		#endregion

		#region Max Memory

		public const string MaxMemoryKey = "max_memory";
		public static float GlobalMaxMemory => GlobalConfig.GetFloat(MaxMemoryKey, 2048);

		public float MaxMemory => ServerConfigContains(MaxMemoryKey)
			? serverConfig.GetFloat(MaxMemoryKey)
			: GlobalMaxMemory;

		#endregion

		#region No Log

		public const string NoLogKey = "multiadmin_nolog";
		public static bool GlobalNoLog => GlobalConfig.GetBool(NoLogKey, false);

		public bool NoLog => ServerConfigContains(NoLogKey)
			? serverConfig.GetBool(NoLogKey)
			: GlobalNoLog;

		#endregion

		#region Log Mod Actions To Own File

		public const string LogModActionsToOwnFileKey = "log_mod_actions_to_own_file";
		public static bool GlobalLogModActionsToOwnFile => GlobalConfig.GetBool(LogModActionsToOwnFileKey, false);

		public bool LogModActionsToOwnFile => ServerConfigContains(LogModActionsToOwnFileKey)
			? serverConfig.GetBool(LogModActionsToOwnFileKey)
			: GlobalLogModActionsToOwnFile;

		#endregion

		#region Max Players

		public const string MaxPlayersKey = "max_players";
		public static int GlobalMaxPlayers => GlobalConfig.GetInt(MaxPlayersKey, 20);

		public int MaxPlayers => ServerConfigContains(MaxPlayersKey)
			? serverConfig.GetInt(MaxPlayersKey)
			: GlobalMaxPlayers;

		#endregion

		#region Disable Config Validation

		public const string DisableConfigValidationKey = "disable_config_validation";
		public static bool GlobalDisableConfigValidation => GlobalConfig.GetBool(DisableConfigValidationKey, false);

		public bool DisableConfigValidation => ServerConfigContains(DisableConfigValidationKey)
			? serverConfig.GetBool(DisableConfigValidationKey)
			: GlobalDisableConfigValidation;

		#endregion

		#region Share Non Configs

		public const string ShareNonConfigsKey = "share_non_configs";
		public static bool GlobalShareNonConfigs => GlobalConfig.GetBool(ShareNonConfigsKey, true);

		public bool ShareNonConfigs => ServerConfigContains(ShareNonConfigsKey)
			? serverConfig.GetBool(ShareNonConfigsKey)
			: GlobalShareNonConfigs;

		#endregion

		#region Config Location

		public const string ConfigLocationKey = "config_location";
		public static string GlobalConfigLocation => GlobalConfig.GetString(ConfigLocationKey, "");

		public string ConfigLocation => ServerConfigContains(ConfigLocationKey)
			? serverConfig.GetString(ConfigLocationKey)
			: GlobalConfigLocation;

		#endregion

		#region Servers Folder

		public const string ServersFolderKey = "servers_folder";
		public static string GlobalServersFolder => GlobalConfig.GetString(ServersFolderKey, "servers");

		public string ServersFolder => ServerConfigContains(ServersFolderKey)
			? serverConfig.GetString(ServersFolderKey)
			: GlobalServersFolder;

		#endregion

		#region Random Input Colors

		public const string RandomInputColorsKey = "random_input_colors";
		public static bool GlobalRandomInputColors => GlobalConfig.GetBool(RandomInputColorsKey, false);

		public bool RandomInputColors => ServerConfigContains(RandomInputColorsKey)
			? serverConfig.GetBool(RandomInputColorsKey)
			: GlobalRandomInputColors;

		#endregion

		#region Port

		public const string PortKey = "port";
		public static int GlobalPort => GlobalConfig.GetInt(PortKey, 7777);

		public int Port => ServerConfigContains(PortKey)
			? serverConfig.GetInt(PortKey)
			: GlobalPort;

		#endregion

		#region Copy From Folder on Reload

		public const string CopyFromFolderOnReloadKey = "copy_from_folder_on_reload";
		public static string GlobalCopyFromFolderOnReload => GlobalConfig.GetString(CopyFromFolderOnReloadKey, "");

		public string CopyFromFolderOnReload => ServerConfigContains(CopyFromFolderOnReloadKey)
			? serverConfig.GetString(CopyFromFolderOnReloadKey)
			: GlobalCopyFromFolderOnReload;

		#endregion

		#region Files to Copy From Folder

		public const string FilesToCopyFromFolderKey = "files_to_copy_from_folder";
		public static string[] GlobalFilesToCopyFromFolder => GlobalConfig.GetStringList(FilesToCopyFromFolderKey, new string[0]);

		public string[] FilesToCopyFromFolder => ServerConfigContains(FilesToCopyFromFolderKey)
			? serverConfig.GetStringList(FilesToCopyFromFolderKey)
			: GlobalFilesToCopyFromFolder;

		#endregion

		#region Server Stop Timeout

		public const string ServerStopTimeoutKey = "server_stop_timeout";
		public static float GlobalServerStopTimeout => GlobalConfig.GetFloat(ServerStopTimeoutKey, 10);

		public float ServerStopTimeout => ServerConfigContains(ServerStopTimeoutKey)
			? serverConfig.GetFloat(ServerStopTimeoutKey)
			: GlobalServerStopTimeout;

		#endregion

		#region Server Restart Timeout

		public const string ServerRestartTimeoutKey = "server_restart_timeout";
		public static float GlobalServerRestartTimeout => GlobalConfig.GetFloat(ServerRestartTimeoutKey, 10);

		public float ServerRestartTimeout => ServerConfigContains(ServerRestartTimeoutKey)
			? serverConfig.GetFloat(ServerRestartTimeoutKey)
			: GlobalServerRestartTimeout;

		#endregion

		public const string ConfigFileName = "scp_multiadmin.cfg";

		public static readonly Config GlobalConfig = new Config(ConfigFileName);
		public readonly Config serverConfig;

		/// <summary>
		///     Creates a <see cref="MultiAdminConfig" /> object with a null <see cref="serverConfig" />. This object will always
		///     return the <see cref="GlobalConfig" />'s value.
		/// </summary>
		public MultiAdminConfig()
		{
			if (!File.Exists(GlobalConfig.ConfigPath))
			{
				File.Create(GlobalConfig.ConfigPath);
			}
		}

		public MultiAdminConfig(Config config) : this()
		{
			serverConfig = config;

			if (!File.Exists(serverConfig.ConfigPath))
			{
				File.Create(serverConfig.ConfigPath);
			}
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