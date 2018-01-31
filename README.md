# MultiAdmin
SCP LocalAdmin modification to support multiple configurations per instance, based off LocalAdmin.exe and released under the same license as that (CC-BY-SA 4.0)

## Features:
- Auto start all instances when running
- provide config folder when using new.
- Individual Log files per instance (located under servers/<config_id>/logs/[date]_output_log.txt)
- A crash reason log (located under servers/<config_id>/crash_reason.txt)
- More detailed information printed to console + title to show which console is using which config
- Supports the config reload command (when the SCP server actually has that command working)
- Crash support.
- Command line parameter support (MultiAdmin.exe <config folder 1> <config folder 2>
- RESTARTNEXTROUND command - restart the server after this round is complete.
- SHUTDOWNNEXTROUND command - shutdown the server after next round
- Autoscale more servers once one becomes full/

## Caveats:
* The server works by hotswapping configs before certain events (new server, config reload etc), so if two servers crash at the same time they may have a conflict and load the wrong config. Should be very rare.
* new now takes a <conf> parameter, which refers to the folder where the config file is located.
* Probably stay away with spaces in server folder names.


## Instructions (MutliAdmin):
1. Place MultiAdmin.exe next to your LocalAdmin.exe
2. Create a new folder called servers within the same folder as LocalAdmin/MultiServer
3. For each instance with a unique config, create a new directory in the servers directory and place each config.txt in there, so for example for two unique configs:
* servers/FirstServer/config.txt
* servers/SecondServer/config.txt
4. If your config is not in its default location due to a different OS and such, make a new file next to MultiAdmin.exe called spc_multiadmin.cfg and place the follwing setting like so:
- cfg_loc=%appdata%\SCP Secret Laboratory\config.txt

If you dont want a server to autolaunch place a blank file with no extension named "manual" in the server folder.

## Single Server
If you only want a single server and just want the other features, have a single folder under servers.

# ServerMod
## ServerMod Installation:
Additional to MultiAdmin, i have modified the ServerManager class and recompiled the DLL to allow variables in the server name. It also containts a fix for the server ghosting issue, no more duplicate servers showing after crash!

Example:
![player count](https://i.imgur.com/pJgS2WJ.png)

The name of the server will update everytime someone leaves/joins!

[Release link](https://github.com/Grover-c13/MultiAdmin/releases/tag/ServerMod0.1)

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

This DLL supports the version of SCP released on the 27th of January 2017 and tested on Windows. Unsure about linux at this point.

## Config Additions
### mutliadmin
- manual_start (wether of not to start the server automatically when launching multiadmin, default = true)
- start_config_on_full (start server with config this config folder when the server becomes full) [requires servermod]
- shutdown_once_empty_for (shutdown the server once a round hasnt started in x seconds)
- restart_every_num_rounds (restart the server every x rounds)
### servermod
- max_players (default 20, max amount of players per server)
- no_scp079_first (default true, computer will never be the first scp in a game)
- SCP049_HP use this to set the starting HP for the class. Default = 1200
- SCP049-2_HP use this to set the starting HP for the class. Default = 400
- SCP079_HP use this to set the starting HP for the class. Default = 100
- SCP106_HP use this to set the starting HP for the class. Default = 700
- SCP457_HP use this to set the starting HP for the class. Default = 700
- SCP173_HP use this to set the starting HP for the class. Default = 2000
##

Place any suggestions/problems in issues!

Thanks & Enjoy.



