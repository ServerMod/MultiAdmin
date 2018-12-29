# Looking for ServerMod?
ServerMod is on its own repo now: https://github.com/Grover-c13/Smod2/

# MultiAdmin
MultiAdmin is a replacement server tool for SCP: Secret Laboratory, which was built to help enable servers to have multiple configurations per server instance.

The latest release can be found here: [Release link](https://github.com/Grover-c13/MultiAdmin/releases/latest)

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
- manual_start (wether of not to start the server automatically when launching multiadmin, default = False)
- start_config_on_full (start server with this config folder when the server becomes full) [requires servermod]
- shutdown_when_empty_for (shutdown the server once a round hasnt started in a number of seconds, default = -1)
- restart_every_num_rounds (restart the server every number rounds, default = -1)
- restart_low_memory (restart if the games memory falls below this, default = 400)
- restart_low_memory_roundend (restart at the end of the round if the games memory falls below this, default = 450)
- max_memory (the amount of memory for MultiAdmin to check against, default = 2048)
- multiadmin_nolog (disable logging to file, default = false)
- log_mod_actions_to_own_file (logs admin messages to seperate file, default = false)

## Upcoming Features
- Support for running multiple server instances
- Support for running multiple server instances in one MultiAdmin instance