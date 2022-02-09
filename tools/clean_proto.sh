#!/bin/bash

# change working directory to tools directory
cd "${0%/*}"

# delete directories for generated files
rm -rf $(pwd)/../csharp/HeuristicLab.TruffleConnector/HeuristicLab.TruffleConnector/3.3/Messages/*
rm -rf $(pwd)/../java/core/src/main/java/at/fh/hagenberg/aist/hlc/core/messages/*
