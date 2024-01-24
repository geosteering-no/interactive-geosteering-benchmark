# Interactive Sequential-decision Geosteering Benchmark
Also known as **NORCE Geosteering Game 2019** or **Project Geobanana**

## Contents
 - Server,
 - Client, and
 - Ensemble-Based Decision Support System
for **Interactive Sequential-decision Geosteering Benchmark Platform**

### Server
1. Dotnet Kestrel Server
2.	Cookie-based identification
3.	JSON-based get/post requests (including send angle and timestamp, get points 4*80*100)
4.	Get-again request
5.	The same data twice does not change anything
6.	Logging all user actions on the backend 

### Client
1. Written in javascript using the [p5](https://p5js.org/) client-side library
2. Visualization of uncertainty
3. Visualization of the tool's depth of detection
4. Ability to evaluate the objective function within the predicted uncertainty

### Ensemble-Based Decision Support System
1. Custom multi-layer toy geomodel and a simplified electromagnetic sensing tool 
2. Ensemble Kalman Filter implementation (in C#) to reduce uncertainty
3. Novel automated decision-support-system (DSS) bot with discrete dynamic programming for global and robust optimization under uncertainty
The default DSS is integrated into the backend, but third-party bots are supported via REST API.

## Instalation

### Requirements
The current solution and project files are configure for **.NET Core Runtime 8.0**

#### Publishing server
This may require building in release first?
1. Navigate to folder which contains GameServer.csproj
2. Run the command `dotnet publish -c Release -r <target-os> --self-contained true`
   * For 64 bit windows 10: \<target-os> = win10-x64
   * Run command:  `dotnet publish -c Release -r win10-x64 --self-contained true`
3. The self-contained folder should now be located at GameServer/bin/Release/netcoreapp3.0/\<target-os>/

4. 
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
url = {https://www.sciencedirect.com/science/article/pii/S2590197421000203},
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
url = {https://www.sciencedirect.com/science/article/pii/S0920410519308022},
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

