# geosteering-game

Server/Client for November Geosteering Demo

## Server
Dotnet Core 3.0 Kestrel Server

### Requires:
* Asp.NetCore 3.0
  * Which requires Visual Studio 2019 or MSBuild 16(??)

### Server Overview
1.	Cookie-based identification
2.	JSON-based get/post requests (send angle, and timestamp*, get points 4*80*100)
3.	Get again request
4.	Same data twice does not change anything
5.	AJAX request solves everything
6.	Logging all on backend 

### Publishing server
This may require building in release first?
1. Navigate to folder which contains GameServer.csproj
2. Run the command `dotnet publish -c Release -r <target-os> --self-contained true`
   * For 64 bit windows 10: \<target-os> = win10-x64
3. The self-contained folder should now be located at GameServer/bin/Release/netcoreapp3.0/\<target-os>/

## Client
Written in javascript using the [p5](https://p5js.org/) client-side library



