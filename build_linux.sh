#!/bin/bash

# change working directory to here
cd "${0%/*}"

# remove bin
rm -rf ./csharp/HeuristicLab/bin

# build heuristic lab
(cd ./csharp/HeuristicLab/; ./prepareProjectsForMono.sh)
nuget restore "./csharp/HeuristicLab/HeuristicLab.ExtLibs.sln"
nuget restore "./csharp/HeuristicLab/HeuristicLab 3.3.sln"
msbuild "./csharp/HeuristicLab/HeuristicLab.ExtLibs.sln"
msbuild "./csharp/HeuristicLab/HeuristicLab 3.3.sln"

# build the truffle connector
nuget restore "csharp/HeuristicLab.TruffleConnector/HeuristicLab.TruffleConnector.sln"  
/bin/bash msbuild "csharp/HeuristicLab.TruffleConnector/HeuristicLab.TruffleConnector.sln"

