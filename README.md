# geosteering-game

Server/Client for November Geosteering Demo

## Server
Dotnet Core Kestrel Server

### Server Overview
1.	Cookie-based identification
2.	JSON-based get/post requests (send angle, and timestamp*, get points 4*80*100)
3.	Get again request
4.	Same data twice does not change anything
5.	AJAX request solves everything
6.	Logging all on backend 


## [Client](./client/gsgc/README.md)
Written in Clojurescript using the [re-frame framework](https://github.com/Day8/re-frame)
Dependencies:
* Leiningen

### Client Overview
1.	Implementation of the stratigraphy (SVG on backend…)
2.	Frequency plot

