
**#Disaclaimer: This project is abandoned, explanation below, do not use**
**This project is abandoned because of unforseen issues causing the implementation of essential features* neigh impossible or very hard. C# and .NET might not be the right tools for this, for further details, see document "Known issues.txt"**
****
# MCServerControl
Host a private Minecraft-server on a local machine and let users interact with it through a web-interface.

## Purpose of this app:

This app was created for when you host a private Minecraft-server on your local network and machine,
you don't neccessarily want the Minecraft-server to be running 100% of the time.
So by configuring and hosting this web-app, you can allow friends (and other trusted users) to control when the host-machine 
should start the server, and to stop it.
If the server is heavily modded, this also has the benefit of letting users
remotely start the server and see when it is done loading (through their browser), and ready to join. Saving them from prematurely loading a
potentially resource-heavy game if they want to keep using their machine without interruptions, while waiting.

## Current features:
Let remote users start/stop the hosted Minecraft-server (with or without password-protection) from the browser.
See the server status (Offline/Loading/Online) in the browser.
Browse and download log-files.
If one is provided, let users download the Minecraft mods Twitch profile (listing all the mods/configs), which lets users import said profile in the twitch-app
to automatically download and setup a modded client compatible with the server.

OBS. This is tested using a vanilla Minecraft server, and may or may not work as intended when running a modded server.
Some modpacks provide their own "StartServer.bat/sh" script for starting the server, this script may also
help setup some required files or configs. So it is recommended to run their scripts to let it setup
and start the server at least once before using this app. (This app executes the .jar directly with the provided arguments
, and does not use any start-scripts that may be provided, this should work just fine as long as everything is already setup
beforehand)
