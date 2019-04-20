# Looking for ServerMod?
ServerMod is on its own repo now: https://github.com/Grover-c13/Smod2/

# MultiAdmin
MultiAdmin is a replacement server tool for SCP: Secret Laboratory, which was built to help enable servers to have multiple configurations per server instance.

The latest release can be found here: [Release link](https://github.com/Grover-c13/MultiAdmin/releases/latest)

A quick note about running MultiAdmin, if you're on a Mono version lower than Mono 5.18.0, you might have issues
If you do have an outdated Mono version, you can download the latest from here: https://www.mono-project.com/download/stable

## Discord
You can join our discord here: https://discord.gg/8nvmMTr

## Installation Instructions:
### Running a Single Server with MultiAdmin
1. Place MultiAdmin.exe in your root server directory (next to LocalAdmin.exe)

### Running Multiple Servers with MultiAdmin
1. Place MultiAdmin.exe in your root server directory (next to LocalAdmin.exe)
2. Create a new directory defined by `servers_folder` (`servers` by default)
3. For each server you'd like, create a directory within the `servers_folder` directory
4. Optional: Create a file named `scp_multiadmin.cfg` within your server's folder for configuring MultiAdmin specifically for that server

## Features
- Autoscale: Auto-starts a new server once this one becomes full (Requires ServerMod to function fully)
- Config Reload: Reloads the MultiAdmin configuration file
- Exit Command: Adds a graceful exit command
- Help: Display a full list of MultiAdmin commands and in game commands
- Stop Server When Inactive: Stops the server after a period inactivity
- Restart On Low Memory: Restarts the server if the working memory becomes too low
- Restart On Low Memory at Round End: Restarts the server if the working memory becomes too low at the end of the round
- ModLog: Logs admin messages to separate file, or prints them
- MultiAdminInfo: Prints MultiAdmin license information
- New: Adds a command to start a new server given a config folder
- Restart Command: Allows the game to be restarted without restarting MultiAdmin
- Restart Next Round: Restarts the server after the current round ends
- Restart After a Number of Rounds: Restarts the server after a number rounds completed
- Stop Next Round: Stops the server after the current round ends
- TitleBar: Updates the title bar with instance based information, such as session id and player count (Requires ServerMod to function fully)

## MultiAdmin Commands
This does not include ServerMod or ingame commands, for a full list type `HELP` in multiadmin which will produce all commands.

- CONFIG <RELOAD>: Reloads the configuration file
- EXIT: Exits the server
- GITHUBGEN [FILE LOCATION]: Generates a github .md file outlining all the features/commands
- HELP: Prints out available commands and their function
- INFO: Prints MultiAdmin license information
- NEW <SERVER ID>: Starts a new server with the given Server ID
- RESTART: Restarts the game server (MultiAdmin will not restart, just the game)
- RESTARTNEXTROUND: Restarts the server at the end of this round
- STOPNEXTROUND: Stops the server at the end of this round

## MultiAdmin Execution Arguments
The arguments available for running MultiAdmin with

- `--headless` or `-h`: Runs MultiAdmin in headless mode, this makes MultiAdmin not accept any input at all and only output to log files, not in console (Note: This argument is inherited by processes started by this MultiAdmin process)
- `--server-id <Server ID>` or `-id <Server ID>`: The Server ID to run this MultiAdmin instance with a config location (`--config` or `-c`) so that it reads the configs from the location, but stores the logs in the Server ID's folder
- `--config <Config Location>` or `-c <Config Location>`: The config location to use for this MultiAdmin instance (Note: This is used over the config option `config_location`)
- `--port <Server Port>` or `-p <Server Port>`: The port to use for this MultiAdmin instance (Note: This is used over the config option `config_location` and is inherited by processes started by this MultiAdmin process)

## Config Settings
All configuration settings go into a file named `scp_multiadmin.cfg` in the same directory as MultiAdmin.exe or in your server directory within the `servers_folder` value defined in the global configuration file
Any configuration files within the directory defined by `servers_folder` will have it's values used for that server over the global configuration file

Config Option | Value Type | Default Value | Description
--- | :---: | :---: | :------:
config_location | String | **Empty** | The default location for the game to use for storing configuration files (a directory)
disable_config_validation | Boolean | False | Disable the config validator
share_non_configs | Boolean | True | Makes all files other than the config files store in AppData
multiadmin_nolog | Boolean | False | Disable logging to file
multiadmin_debug_log | Boolean | True | Enables MultiAdmin debug logging, this logs to a separate file than any other logs
multiadmin_debug_log_blacklist | String List | ProcessFile | Which tags to block for MultiAdmin debug logging
multiadmin_debug_log_whitelist | String List | **Empty** | Which tags to log for MultiAdmin debug logging (Defaults to logging all if none are provided)
use_new_input_system | Boolean | True | Whether to use the new input system, if false, the original input system will be used
port | Unsigned Integer | 7777 | The port for the server to use (Preparing for next game release, currently does nothing)
copy_from_folder_on_reload | String | **Empty** | The location of a folder to copy files from into the folder defined by `config_location` whenever the configuration file is reloaded
folder_copy_whitelist | String List | **Empty** | The list of file names to copy from the folder defined by `copy_from_folder_on_reload` (accepts `*` wildcards)
folder_copy_blacklist | String List | **Empty** | The list of file names to not copy from the folder defined by `copy_from_folder_on_reload` (accepts `*` wildcards)
folder_copy_round_queue | String List | **Empty** | The location of a folder to copy files from into the folder defined by `config_location` after each round, looping through the locations
folder_copy_round_queue_whitelist | String List | **Empty** | The list of file names to copy from the folders defined by `folder_copy_round_queue` (accepts `*` wildcards)
folder_copy_round_queue_blacklist | String List | **Empty** | The list of file names to not copy from the folders defined by `folder_copy_round_queue` (accepts `*` wildcards)
randomize_folder_copy_round_queue | Boolean | False | Whether to randomize the order of entries in `folder_copy_round_queue`
log_mod_actions_to_own_file | Boolean | False | Logs admin messages to seperate file
manual_start | Boolean | False | Whether or not to start the server automatically when launching MultiAdmin
max_memory | Float | 2048.0 | The amount of memory in megabytes for MultiAdmin to check against
restart_low_memory | Float | 400.0 | Restart if the games memory falls below this value in megabytes
restart_low_memory_roundend | Float | 450.0 | Restart at the end of the round if the game's memory falls below this value in megabytes
max_players | Integer | 20 | The number of players to display as the maximum for the server (within MultiAdmin, not in-game)
restart_every_num_rounds | Integer | -1 | Restart the server every number rounds
server_restart_timeout | Float | 10.0 | The time in seconds before MultiAdmin forces a server restart if it doesn't respond to the regular restart command
server_stop_timeout | Float | 10.0 | The time in seconds before MultiAdmin forces a server shutdown if it doesn't respond to the regular shutdown command
servers_folder | String | servers | The location of the "servers" folder for MultiAdmin to load multiple server configurations from
shutdown_when_empty_for | Seconds | -1 | Shutdown the server once a round hasn't started in a number of seconds
start_config_on_full | String | **Empty** | Start server with this config folder once the server becomes full [Requires ServerMod]

## Upcoming Features
- Support for running multiple server instances in one MultiAdmin instance