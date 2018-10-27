# Looking for ServerMod?
ServerMod is on its own repo now: https://github.com/Grover-c13/Smod2/

# MultiAdmin
MultiAdmin is a replacement server tool for SCP: Secret Labratories, which was built to help enable servers to have multiple configurations per server instance.

The latest release can be found here: [Release link](https://github.com/Grover-c13/MultiAdmin/releases/latest)

## Discord
You can join our discord here: https://discord.gg/8nvmMTr


## Features
- Autoscale: Auto-starts a new server once this one becomes full. (Requires servermod to function fully)
- ChainStart: Automatically starts the next server after the first one is done loading.
- Config reload: Config reload will swap configs
- Exit command: Adds a graceful exit command.
- GitHub log submitted: Goes through the last log file and submits any stacktraces
- Help: Display a full list of multiadmin commands and in game commands.
- Stop Server once Inactive: Stops the server after a period inactivity.
- Restart On Low Memory: Restarts the server if the working memory becomes too low
- Restart On Low Memory at the end of the round: Restarts the server if the working memory becomes too low at the end of the round
- MutliAdminInfo: Prints the license/author information
- New: Adds a command to start a new server given a config folder.
- Restart Next Round: Restarts the server after the current round ends.
- Restart After X Rounds: Restarts the server after X num rounds completed.
- Stop Next Round: Stops the server after the current round ends.
- Titlebar: Updates the title bar with instance based information, such as session id and player count. (Requires servermod to function fully)

## Installation Instructions:
1. Place MultiAdmin.exe next to your LocalAdmin.exe
2. Create a new folder called servers within the same folder as LocalAdmin/MultiAdmin
3. For each instance with a unique config, create a new directory in the servers directory and place each config.txt in there, so for example for two unique configs:
* servers/FirstServer/config.txt
* servers/SecondServer/config.txt
4. If your config is not in its default location due to a different OS and such, make a new file next to MultiAdmin.exe called scp_multiadmin.cfg and place the follwing setting like so:
- cfg_loc=%appdata%\SCP Secret Laboratory\config.txt

## MultiAdmin Commands
This does not include ServerMod or ingame commands, for a full list type HELP in multiadmin which will produce all commands.

- CONFIG <reload>: Handles reloading the config
- EXIT: Exits the server
- GITHUBGEN [filelocation]: Generates a github .md file outlining all the features/commands
- HELP: Prints out available commands and their function.
- INFO: Prints license and author information.
- NEW <config_id>: Starts a new server with the given config id.
- RESTARTNEXTROUND: Restarts the server at the end of this round
- STOPNEXTROUND: Stops the server at the end of this round

## Config settings
- manual_start (wether of not to start the server automatically when launching multiadmin, default = False)
- start_config_on_full (start server with this config folder when the server becomes full) [requires servermod]
- multiadmin_print_speed (delay between outputs in the MultiAdmin console, default = 150)
- shutdown_once_empty_for (shutdown the server once a round hasnt started in x seconds, default = -1)
- restart_every_num_rounds (restart the server every x rounds, default = 15)
- restart_low_memory (restart if the games memory falls below this, default = 400)
- restart_low_memory_roundend (restart if the games memory falls below this at the end of this round, default = 400)
- max_memory (used to determine if the servers memory is low, default = 2048)
