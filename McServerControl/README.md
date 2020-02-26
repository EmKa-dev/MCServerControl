## Instructions:

publish using the dotnet CLI (or other tools), see link further down.
Navigate to the published folder.
Place your server files inside the folder "MinecraftServerFiles"
(OBS. It is the users responsibilty to make sure all the files are present to make the server function. If you downloaded a server .jar-file, make sure to run it
once to generate the eula.txt, open it to accept the eula (change it to "true"), and then run the .jar again to generate the rest of the files and folders).
Rename the server executable to "server.jar".

## Configure Config.json:

Title: Set the title on the web-interface.

Password: Set the password required for Starting/stopping the server via the web-interface. Currently, cannot be empty.

JavaArguments: The arguments used to start the java executable, these are the same arguments that you would put in a "Start.bat/sh"-file.
Example: "-Xms256M -Xmx1G -jar server.jar" ("-jar server.jar" is fine if you don't need to set extra parameters")
(OBS. don't prefix the arguments with "java", for example don't write "java -jar server-jar")

IncludeTwitchMCProfile: If you made a mod-profile using the Twitch-app, this profile-file (.zip) is placed in /Content/TwitchProfile. 
Then the exact name of that file is typed into this field (including extension). Example: "IncludeTwitchMCProfile": "MyModPack.zip".
Otherwise, if a mod-profile is not included, "false" or "".


Set up and host the web-app using a web-server (no support is given for this, see link below for guidance).
Make sure your web-server is reachable from the internet. Likely this requires port-forward/trigger to be enabled in your router (this may also be required for
the actual minecraft server).

The app is ready to be hosted using a IIS web-server out of the box
(It needs to be published first, for information on building/publishing and hosting, see: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/?view=aspnetcore-3.1).
Some additional configuration may be needed for other web-servers, see the above link.
No support is given for setting up hosting. (The most common problem with hosting with IIS (and other) is permissions,
so make sure IIS has sufficient permissions to the serverfiles and folders!)
