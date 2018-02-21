# MultiAdmin
MultiAdmin is a replacement server tool for SCP: Secret Labratories, which was built to help enable servers to have multiple configurations per server instance.

The latest release can be found here: [Release link](https://github.com/Grover-c13/MultiAdmin/releases/latest)

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
2. Create a new folder called servers within the same folder as LocalAdmin/MultiServer
3. For each instance with a unique config, create a new directory in the servers directory and place each config.txt in there, so for example for two unique configs:
* servers/FirstServer/config.txt
* servers/SecondServer/config.txt
4. If your config is not in its default location due to a different OS and such, make a new file next to MultiAdmin.exe called spc_multiadmin.cfg and place the follwing setting like so:
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
- manual_start (wether of not to start the server automatically when launching multiadmin, default = true)
- start_config_on_full (start server with config this config folder when the server becomes full) [requires servermod]
- shutdown_once_empty_for (shutdown the server once a round hasnt started in x seconds)
- restart_every_num_rounds (restart the server every x rounds)
- restart_low_memory (restart if the games memory falls below this, default = 400)
- restart_low_memory_roundend (restart if the games memory falls below this at the end of this round, default = 400)

# ServerMod
ServerMod is an additional tool i have developed to add more configuration settings, fix bugs, and attempt to make servers more stable where possible. You dont need multiadmin for this, but it is recommended!

The latest release can be found here: [Release link](https://github.com/Grover-c13/MultiAdmin/releases/latest)

## Features:
- Fixes as many null reference errors as i can find, making your log files smaller.
- Dynamic server names with player count etc.
- Increasing the player count.
- Disabling of SCPs
- Configuring max class healths.
- Anti-nuke spam.


## ServerMod Installation:
To install:
1. Navigate to your SCP Secret Lab folder.
2. Go into SCSL_Data/Managed/
3. Make a backup of Assembly-CSharp.dll
4. Replace Assembly-CSharp.dll with the one in the releases tab.

## Variables
Currently supported variables (place in your servers name):
- $player_count (current number of connected players) EG: "$player_count playing!"
- $port (the port of the current server) EG: "Welcome to SCPServer.com:$port"
- $ip (the ip of the server) EG: "Welcome to SCPServer.com [$ip:$port]"
- $full_player_count (will display player count as $player_count/$max_player_count or FULL if there are $max_player_count players) EG: "Server.com $full_player_count"
- $number (will display the number of the instance, assuming youre using default ports, this works by subtracting 7776 from the port (so $number will = 1 for the first server, #2 for the second)
- $lobby_id (debugging to print the lobby_id)
- $version (version of the game)
- $max_players (max amount of players in the config)
- $scp_alive - number of alive SCPS.
- $scp_start - number of SCPs at start of the round.
- $scp_counter - prints $scp_alive/$scp_start
- $scp_dead - number of dead scps.
- $scp_zombies - current number of zombies.
- $classd_escape - how many class ds have escaped.
- $classd_start - the amount of starting class ds.
- $classd_counter - $classd_escape/$classd_counter.
- $scientists_escape - The number of scientists to escape so far.
- $scientists_start - the amount of starting scientists
- $scientists_counter - $scientists_escape/$scientist_start.
- $scp_kills - number of people killed by scps.
- $warhead_detonated - prints ☢ WARHEAD DETONATED ☢ if its gone off.

Example:
![player count](https://user-images.githubusercontent.com/1520101/36029888-04689b5c-0de0-11e8-81cd-b1d458caf7e9.png)

## Config Additions
- max_players (default 20, max amount of players per server)
- no_scp079_first (default true, computer will never be the first scp in a game)
- nuke_disable_cooldown stop the nuke from being spammed, will stop the nuke arm switch from being disabled until this has elapsed. Default = 0
- SCP106_cleanup use this to stop ragdolls spawning in the pocket dimension [currently items still spawn]
- SCP049_HP use this to set the starting HP for the class. Default = 1200
- SCP049-2_HP use this to set the starting HP for the class. Default = 400
- SCP079_HP use this to set the starting HP for the class. Default = 100
- SCP106_HP use this to set the starting HP for the class. Default = 700
- SCP173_HP use this to set the starting HP for the class. Default = 2000
- SCP457_HP use this to set the starting HP for the class. Default = 700
- CLASSD_HP use this to set the starting HP for the class. Default = 100
- NTFSCIENTIST_HP use this to set the starting HP for the class. Default = 100
- SCIENTIST_HP use this to set the starting HP for the class. Default = 100
- CI_HP use this to set the starting HP for the class. Default = 120
- NTFL_HP use this to set the starting HP for the class. Default = 120
- NTFC_HP use this to set the starting HP for the class. Default = 150
- NTFG_HP use this to set the starting HP for the class. Default = 100
- SCP049_DISABLE disable this scp, default: no
- SCP079_DISABLE disable this scp, default: no
- SCP106_DISABLE disable this scp, default: no
- SCP173_DISABLE disable this scp, default: no
- SCP457_DISABLE disable this scp, default: no
##

Place any suggestions/problems in issues!

Thanks & Enjoy.



