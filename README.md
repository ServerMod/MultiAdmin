# MultiAdmin
SCP LocalAdmin modification to support multiple configurations per instance, based off LocalAdmin.exe and released under the same license as that (CC-BY-SA 4.0)

## Features:
- Auto start all instances when running
- provide config folder when using new.
- More detailed information printed to console + title to show which console is using which config
- Supports the config reload command (when the SCP server actually has that command working)
- Crash support.
- Command line parameter support (MultiAdmin.exe <config folder 1> <config folder 2>

## Caveats:
* The server works by hotswapping configs before certain events (new server, config reload etc), so if two servers crash at the same time they may have a conflict and load the wrong config. Should be very rare.
* new now takes a <conf> parameter, which refers to the folder where the config file is located.
* Probably stay away with spaces in server folder names.


## Instructions:
1. Place MultiServer.exe next to your LocalAdmin.exe
2. Create a new folder called servers
3. For each instance with a unique config, create a new directory in the servers directory and place each config.txt in there, so for example for two unique configs:
* servers/FirstServer/config.txt
* servers/SecondServer/config.txt

If you dont want a server to autolaunch place a blank file with no extension named "manual" in the server folder.

Place any suggestions/problems in issues!

Thanks & Enjoy.



