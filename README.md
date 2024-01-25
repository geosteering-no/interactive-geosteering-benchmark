# Interactive Sequential-decision Geosteering Benchmark
Also known as **NORCE Geosteering Game 2019** or **Project Geobanana**

## Contents
 1. **Server**
 2. **Client**
 3. **Ensemble-Based Decision Support System**

which comprise the Interactive Sequential-decision Geosteering Benchmark Platform.

### 1. Server
The server project is located in the folder [/server/GameServer2](/server/GameServer2).

Server description and features:
1. Dotnet Kestrel Server
2.	Cookie-based identification
3.	JSON-based get/post requests (including send angle and timestamp, get points 4*80*100)
4.	Get-again request
5.	The same data twice does not change anything
6.	Logging all user actions on the backend

The folder [/server/ServerObjectives2](/server/ServerObjectives2) contains the implementation of objective functions used for the benchmark; see [Alyaev et al. (2021)](https://doi.org/10.1016/j.acags.2021.100072) cited below.

### 2. Client
The client web files are served by the Server and are located in the folder [/server/GameServer2/wwwroot](/server/GameServer2/wwwroot).

Client description and features:
1. Written in javascript using the [p5.js](https://p5js.org/) client-side library
2. Visualization of uncertainty
3. Visualization of the tool's depth of detection
4. Ability to evaluate the objective function within the predicted uncertainty

### 3. Ensemble-Based Decision Support System
Ensemble-Based Decision Support System components (and corresponding project folders):
1. Custom multi-layer toy geomodel and a simplified electromagnetic sensing tool ([/server/ResistivitySimulator](/server/ResistivitySimulator))
2. Ensemble Kalman Filter implementation (in C#) to reduce uncertainty ([/server/EnKFLib2](/server/EnKFLib2))
3. Novel automated decision-support-system (DSS) bot with discrete dynamic programming for global and robust optimization under uncertainty ([/server/TrajectoryOptimization](/server/TrajectoryOptimization))

The default DSS is integrated into the backend, but third-party bots are supported via REST API.

Other directories contain utility libraries. 

## Installation and execution

### Requirements
The current solution and project files are configured for **.NET 8.0**.

**.NET 8.0** is available for many platforms and can be installed from its official site following the instructions: 

https://dotnet.microsoft.com/en-us/download/dotnet/8.0

### Building (Publishing) Server

Build and publish the Server project [server/GameServer2/GameServer2.csproj](server/GameServer2/GameServer2.csproj) by running:
```
dotnet publish server/GameServer2/GameServer2.csproj -c Release --self-contained true
```

The self-contained folder should now be located at **server/GameServer2/bin/Release/net8.0/\<Target-OS>/publish/**,
where Target-OS is the identifier of your operating system.

### Running server

To start the server, run the "published" executable in **publish** directory.

Here are some examples depending on your OS:

#### Mac-OS (arm-64)
Go to directory:
```
cd server/GameServer2/bin/Release/net8.0/osx-arm64/publish
```
Run:
```
./GameServer2
```

#### Windows

#### Linux

### Debugging and developing

Use a debugger tool in [Microsoft Visual Studio](https://visualstudio.microsoft.com/) (Windows only) or [Microsoft VS Code](https://code.visualstudio.com/) for debugging and developing.

In **Visual Studio** open `server/GameServer.sln`

In **VS Code** open the project root directory.

## Citing and details: 

The repository contains the results of two software projects:
1. Web-based Interactive Sequential-decision Geosteering Platform and Benchmark
2. Back-end system that represents and reduces uncertainties using Ensemble Kalman Filter paired with fully automated Decision Support System

Below are details of two papers describing these components:

### 1. Web-based Interactive Sequential-decision Geosteering Platform and Benchmark
### Cite as:

Sergey Alyaev, Sofija Ivanova, Andrew Holsaeter, Reidar Brumer Bratvold, Morten Bendiksen,
**An interactive sequential-decision benchmark from geosteering**,
*Applied Computing and Geosciences, Volume 12,*
2021,
100072,
ISSN 2590-1974,
https://doi.org/10.1016/j.acags.2021.100072

#### Bibtex
```
@article{alyaev2021interactive,
title = {An interactive sequential-decision benchmark from geosteering},
journal = {Applied Computing and Geosciences},
volume = {12},
pages = {100072},
year = {2021},
issn = {2590-1974},
doi = {https://doi.org/10.1016/j.acags.2021.100072},
author = {Sergey Alyaev and Sofija Ivanova and Andrew Holsaeter and Reidar Brumer Bratvold and Morten Bendiksen},
keywords = {Interactive benchmark, Sequential geosteering decisions, Uncertainty quantification, Expert decisions, Experimental study, Decision support system}
}
```

### 2. Decision Support System for Multi-target Geosteering
### Cite as:
Sergey Alyaev, Erich Suter, Reider Brumer Bratvold, Aojie Hong, Xiaodong Luo, Kristian Fossum,
**A decision support system for multi-target geosteering**,
*Journal of Petroleum Science and Engineering, Volume 183,*
2019,
106381,
ISSN 0920-4105,
https://doi.org/10.1016/j.petrol.2019.106381

#### Bibtex
```
@article{ALYAEV2019106381,
title = {A decision support system for multi-target geosteering},
journal = {Journal of Petroleum Science and Engineering},
volume = {183},
pages = {106381},
year = {2019},
issn = {0920-4105},
doi = {https://doi.org/10.1016/j.petrol.2019.106381},
author = {Sergey Alyaev and Erich Suter and Reider Brumer Bratvold and Aojie Hong and Xiaodong Luo and Kristian Fossum},
keywords = {Geosteering, Sequential decision, Dynamic programming, Statistical inversion, Well placement decision, Multi-objective optimization},
}
```

## Contributors

**Sergey Alyaev** (aka [alin256](https://github.com/alin256): lead developer and principal investigator),
**Andrew Holsaeter** (front-end and GUI refinement),
**Morten Bendiksen** (server and client-server interaction development),
**Sofija Ivanova** (user-experience design), and
**Erich Suter** (decision-support-system contributions).
### Special thanks
For useful advice to NORCE colleagues: **Kristian Fossum**, **Robert Ewald**, and **Rolf Johan Lorentzen**.

### Acknowledgements 
The development of this software
was supported by the research project <b>Geosteering for IOR</b> 
(NFR-Petromaks2 project no. 268122) hosted by NORCE and funded by the Research Council of Norway, Aker BP, Equinor, Vår Energi, and Baker Hughes Norge.

The open-source release of the code is supported by the research project <b>DISTINGUISH</b>
(NFR-Petromaks2 project no. 344236) hosted by NORCE and funded by the Research Council of Norway, Aker BP, and Equinor.

For details visit the project website **[geosteering.no](https://geosteering.no/)**.

