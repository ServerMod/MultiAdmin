# Looking for ServerMod?
ServerMod is on its own repo now: https://github.com/Grover-c13/Smod2/

# MultiAdmin
MultiAdmin is a replacement server tool for SCP: Secret Laboratory, which was built to help enable servers to have multiple configurations per server instance.

The latest release can be found here: [Release link](https://github.com/Dankrushen/MultiAdmin/releases/latest)

## Discord
You can join our discord here: https://discord.gg/8nvmMTr

## Installation Instructions:
1. Place MultiAdmin.exe in your root server directory (next to LocalAdmin.exe)

## Features
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
- TitleBar: Updates the title bar with instance based information, such as session id and player count. (Requires ServerMod to function fully)

## MultiAdmin Commands
This does not include ServerMod or ingame commands, for a full list type `HELP` in multiadmin which will produce all commands.

- CONFIG <RELOAD>: Reloads the configuration file
- EXIT: Exits the server
- GITHUBGEN [FILE LOCATION]: Generates a github .md file outlining all the features/commands
- HELP: Prints out available commands and their function
- INFO: Prints MultiAdmin license information
- NEW <CONFIG ID>: Starts a new server with the given config id
- RESTART: Restarts the game server (MultiAdmin will not restart, just the game)
- RESTARTNEXTROUND: Restarts the server at the end of this round
- STOPNEXTROUND: Stops the server at the end of this round

## Config settings
All configuration settings go into a file named `scp_multiadmin.cfg` (you'll have to make this file) in the same directory as MultiAdmin.exe

Config Option | Value Type | Default Value | Description
--- | :---: | :---: | :------:
manual_start | Boolean | False | Whether or not to start the server automatically when launching MultiAdmin
start_config_on_full | String | **Empty** | Start server with this config folder once the server becomes full [Requires ServerMod]
shutdown_when_empty_for | Seconds | -1 | Shutdown the server once a round hasn't started in a number of seconds
restart_every_num_rounds | Integer | -1 | Restart the server every number rounds
restart_low_memory | Integer | 400 | Restart if the games memory falls below this value in megabytes
restart_low_memory_roundend | Integer | 450 | Restart at the end of the round if the games memory falls below this value in megabytes
max_memory | Integer | 2048 | The amount of memory in megabytes for MultiAdmin to check against
multiadmin_nolog | Boolean | False | Disable logging to file
log_mod_actions_to_own_file | Boolean | False | Logs admin messages to seperate file
disable_config_validation | Boolean | False | Disable the config validator
share_non_configs | Boolean | False | Make all servers share an AppData directory rather than being organized by port
config_location | String | **Empty** | The default location for the game to use for storing configuration files

## Upcoming Features
- Support for running multiple server instances
- Support for running multiple server instances in one MultiAdmin instance
- Printing speed configuration option