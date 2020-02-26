# MCServerControl
Host a private MInecraft-server on a local machine and let users interact with it through a web-interface.

##Purpose of this app:

This app was created for when you host a private Minecraft-server on your local network and machine,
you don't neccessarily want the Minecraft-server to be running 100% of the time.
So by configuring and hosting this web-app, you can allow friends (and other trusted users) to control when the host-machine 
should start the server, and to stop it.
If the server is heavily modded, this also has the benefit of letting users
remotely start the server and see when it is done loading (through their browser), and ready to join. Saving them from prematurely loading a
potentially resource-heavy game if they want to keep using their machine without interruptions, while waiting.

###Current features:
Let remote users start/stop the hosted Minecraft-server (with or without password-protection) from the browser.
See the server status (Offline/Loading/Online) in the browser.
Browse and download log-files.
If one is provided, let users download the Minecraft mods Twitch profile (listing all the mods/configs), which lets users import said profile in the twitch-app
to automatically download and setup a modded client compatible with the server.
