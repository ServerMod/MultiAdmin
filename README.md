# Looking for ServerMod?
ServerMod is on its own repo now: https://github.com/ServerMod/Smod2/

# MultiAdmin
MultiAdmin is a replacement server tool for SCP: Secret Laboratory, which was built to help enable servers to have multiple configurations per server instance.

The latest release can be found here: [Release link](https://github.com/ServerMod/MultiAdmin/releases/latest)

Please check the [Installation Instructions](https://github.com/ServerMod/MultiAdmin#installation-instructions) for information about installing and running MultiAdmin.

## Discord
You can join our Discord server here: https://discord.gg/8nvmMTr

## Installation Instructions:
Make sure that you are running Mono 5.18.0 or higher, otherwise you might have issues. The latest Mono release can be found here: https://www.mono-project.com/download/stable.
### Running a Single Server with MultiAdmin
1. Place MultiAdmin.exe in your root server directory (next to LocalAdmin.exe)

### Running Multiple Servers with MultiAdmin
1. Place MultiAdmin.exe in your root server directory (next to LocalAdmin.exe)
2. Create a new directory defined by `servers_folder` (`servers` by default)
3. For each server you'd like, create a directory within the `servers_folder` directory
4. Optional: Create a file named `scp_multiadmin.cfg` within your server's folder for configuring MultiAdmin specifically for that server

## Features

- Config Generator: Generates a full default MultiAdmin config file
- Config Reload: Reloads the MultiAdmin configuration file
- Exit Command: Adds a graceful exit command
- File Copy Round Queue: Copies files from folders in a queue
- GitHub Generator: Generates a GitHub README file outlining all the features/commands
- Help: Display a full list of MultiAdmin commands and in game commands
- Restart On Low Memory: Restarts the server if the working memory becomes too low
- MultiAdminInfo: Prints MultiAdmin license and version information
- New Server: Adds a command to start a new server given a config folder and a config to start a new server when one is full [Config Requires Modding]
- Restart Command: Allows the game to be restarted without restarting MultiAdmin
- Restart After a Number of Rounds: Restarts the server after a number rounds completed [Requires Modding]
- TitleBar: Updates the title bar with instance based information

## MultiAdmin Commands
This does not include ingame commands, for a full list type `HELP` in MultiAdmin which will produce all commands.

- CONFIGGEN [FILE LOCATION]: Generates a full default MultiAdmin config file
- CONFIG <RELOAD>: Reloads the configuration file
- EXIT: Exits the server
- GITHUBGEN [FILE LOCATION]: Generates a GitHub README file outlining all the features/commands
- HELP: Prints out available commands and their function
- INFO: Prints MultiAdmin license and version information
- NEW <SERVER ID>: Starts a new server with the given Server ID
- RESTART: Restarts the game server (MultiAdmin will not restart, just the game)

## MultiAdmin Execution Arguments
The arguments available for running MultiAdmin with

- `--headless` or `-h`: Runs MultiAdmin in headless mode, this makes MultiAdmin not accept any input at all and only output to log files, not in console (Note: This argument is inherited by processes started by this MultiAdmin process)
- `--input-system <ConsoleInputSystem>` or `-is <ConsoleInputSystem>`: The [ConsoleInputSystem](#consoleinputsystem) to use for this MultiAdmin instance (Note: This is used over the config option `console_input_system` and is inherited by processes started by this MultiAdmin process)
- `--server-id <Server ID>` or `-id <Server ID>`: The Server ID to run this MultiAdmin instance with a config location (`--config` or `-c`) so that it reads the configs from the location, but stores the logs in the Server ID's folder
- `--config <Config Location>` or `-c <Config Location>`: The config location to use for this MultiAdmin instance (Note: This is used over the config option `config_location`)
- `--port <Server Port>` or `-p <Server Port>`: The port to use for this MultiAdmin instance (Note: This is used over the config option `port` and is inherited by processes started by this MultiAdmin process)

## Config Settings
All configuration settings go into a file named `scp_multiadmin.cfg` in the same directory as MultiAdmin.exe or in your server directory within the `servers_folder` value defined in the global configuration file
Any configuration files within the directory defined by `servers_folder` will have it's values used for that server over the global configuration file

Example config:
```yml
port: 7777
max_memory: 2048
config_location: "/home/container/Server Config"
```

Config Option | Value Type | Default Value | Description
--- | :---: | :---: | :------:
config_location | String | **Empty** | The default location for the game to use for storing configuration files (a directory)
appdata_location | String | **Empty** | The location for the game to use for AppData (a directory)
disable_config_validation | Boolean | False | Disable the config validator
share_non_configs | Boolean | True | Makes all files other than the config files store in AppData
multiadmin_log_location | String | logs | The folder that MultiAdmin will store logs in (a directory)
multiadmin_nolog | Boolean | False | Disable logging to file
multiadmin_debug_log | Boolean | True | Enables MultiAdmin debug logging, this logs to a separate file than any other logs
multiadmin_debug_log_blacklist | String List | HandleMessage, StringMatches, MessageListener | Which tags to block for MultiAdmin debug logging
multiadmin_debug_log_whitelist | String List | **Empty** | Which tags to log for MultiAdmin debug logging (Defaults to logging all if none are provided)
use_new_input_system | Boolean | True | **OBSOLETE: Use `console_input_system` instead, this config option may be removed in a future version of MultiAdmin.** Whether to use the new input system, if false, the original input system will be used
console_input_system | [ConsoleInputSystem](#consoleinputsystem) | New | Which console input system to use
hide_input | Boolean | False | Whether to hide console input, if true, typed input will not be printed
port | Unsigned Integer | 7777 | The port for the server to use
copy_from_folder_on_reload | String | **Empty** | The location of a folder to copy files from into the folder defined by `config_location` whenever the configuration file is reloaded
folder_copy_whitelist | String List | **Empty** | The list of file names to copy from the folder defined by `copy_from_folder_on_reload` (accepts `*` wildcards)
folder_copy_blacklist | String List | **Empty** | The list of file names to not copy from the folder defined by `copy_from_folder_on_reload` (accepts `*` wildcards)
folder_copy_round_queue | String List | **Empty** | The location of a folder to copy files from into the folder defined by `config_location` after each round, looping through the locations
folder_copy_round_queue_whitelist | String List | **Empty** | The list of file names to copy from the folders defined by `folder_copy_round_queue` (accepts `*` wildcards)
folder_copy_round_queue_blacklist | String List | **Empty** | The list of file names to not copy from the folders defined by `folder_copy_round_queue` (accepts `*` wildcards)
randomize_folder_copy_round_queue | Boolean | False | Whether to randomize the order of entries in `folder_copy_round_queue`
manual_start | Boolean | False | Whether or not to start the server automatically when launching MultiAdmin
max_memory | Decimal | 2048 | The amount of memory in megabytes for MultiAdmin to check against
restart_low_memory | Decimal | 400 | Restart if the game's remaining memory falls below this value in megabytes
restart_low_memory_ticks | Unsigned Integer | 10 | The number of ticks the memory can be over the limit before restarting
restart_low_memory_roundend | Decimal | 450 | Restart at the end of the round if the game's remaining memory falls below this value in megabytes
restart_low_memory_roundend_ticks | Unsigned Integer | 10 | The number of ticks the memory can be over the limit before restarting at the end of the round
random_input_colors | Boolean | False | Randomize the new input system's colors every time a message is input
restart_every_num_rounds | Integer | -1 | Restart the server every number of rounds
restart_every_num_rounds_counting | Boolean | False | Whether to print the count of rounds passed after each round if the server is set to restart after a number of rounds
safe_server_shutdown | Boolean | True | When MultiAdmin closes, if this is true, MultiAdmin will attempt to safely shutdown all servers
safe_shutdown_check_delay | Integer | 100 | The time in milliseconds between checking if a server is still running when safely shutting down
safe_shutdown_timeout | Integer | 10000 | The time in milliseconds before MultiAdmin gives up on safely shutting down a server
server_restart_timeout | Double | 10 | The time in seconds before MultiAdmin forces a server restart if it doesn't respond to the regular restart command
server_stop_timeout | Double | 10 | The time in seconds before MultiAdmin forces a server shutdown if it doesn't respond to the regular shutdown command
server_start_retry | Boolean | True | Whether to try to start the server again after crashing
server_start_retry_delay | Integer | 10000 | The time in milliseconds to wait before trying to start the server again after crashing
multiadmin_tick_delay | Integer | 1000 | The time in milliseconds between MultiAdmin ticks (any features that update over time)
servers_folder | String | servers | The location of the `servers` folder for MultiAdmin to load multiple server configurations from
set_title_bar | Boolean | True | Whether to set the console window's titlebar, if false, this feature won't be used
start_config_on_full | String | **Empty** | Start server with this config folder once the server becomes full [Requires Modding]

## ConsoleInputSystem
If you are running into issues with the `tmux send-keys` command, switch to the original input system.

String Value | Integer Value | Description
--- | :---: | :----:
Original | 0 | Represents the original input system. It may prevent MultiAdmin from closing and/or cause ghost game processes.
Old | 1 | Represents the old input system. This input system should operate similarly to the original input system but won't cause issues with MultiAdmin's functionality.
New | 2 | Represents the new input system. The main difference from the original input system is an improved display.
