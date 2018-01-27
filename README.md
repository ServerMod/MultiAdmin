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

## Caveats:
* The server works by hotswapping configs before certain events (new server, config reload etc), so if two servers crash at the same time they may have a conflict and load the wrong config. Should be very rare.
* new now takes a <conf> parameter, which refers to the folder where the config file is located.
* Probably stay away with spaces in server folder names.


## Instructions (MutliAdmin):
1. Place MultiServer.exe next to your LocalAdmin.exe
2. Create a new folder called servers within the same folder as LocalAdmin/MultiServer
3. For each instance with a unique config, create a new directory in the servers directory and place each config.txt in there, so for example for two unique configs:
* servers/FirstServer/config.txt
* servers/SecondServer/config.txt

If you dont want a server to autolaunch place a blank file with no extension named "manual" in the server folder.

## Single Server
If you only want a single server and just want the other features, have a single folder under servers.

# ServerMod
## ServerMod Installation:
Additional to MultiAdmin, i have modified the ServerManager class and recompiled the DLL to allow variables in the server name. It also has an uncofirmed fix for the Server Ghosting issue on crash.

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
- $full_player_count (will display player count as $player_count/20 or FULL if there are 20 players) EG: "Server.com $full_player_count"


This DLL supports the version of SCP released on the 27th of January 2017 and for Windows only.

##

Place any suggestions/problems in issues!

Thanks & Enjoy.



