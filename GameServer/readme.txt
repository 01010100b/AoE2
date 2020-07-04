This is a .NET Core 3.1 application, it requires the .Net Core 3.1 Runtime to be installed.
Change settings.json to point at your Aoe2 WK installation. Speed is the speedhack (10 = normal speed)
Run GameServer.exe
You will be requested for the password to upload games to the ladder. Leave blank to run in local mode.
It is suggested to first try out the game server in local mode to see if everything works, it will not upload the games at the end.

The game server will repeatedly do the following:
- get rankings and zips from the ladder
- start an AoE2 process and minimize the window
- run a game
- close the AoE2 process
- upload the game result

Since it is written in .Net Core it "should" also work in Linux, although dll injection might give issues.