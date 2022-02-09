#!/bin/bash

# change working directory to tools directory
cd "${0%/*}"

# remove existing generated files
sh $(pwd)/clean_proto.sh

# make sure the folders exist
mkdir -p ../java/core/src/main/java
mkdir -p ../csharp/HeuristicLab.TruffleConnector/HeuristicLab.TruffleConnector/3.3/Messages/

#generate protos
$(pwd)/protoc/bin/protoc \
-I=$(pwd)/../proto \
-I=protoc/include \
--csharp_out=$(pwd)/../csharp/HeuristicLab.TruffleConnector/HeuristicLab.TruffleConnector/3.3 \
--csharp_opt=file_extension=.g.cs,base_namespace=HeuristicLab.TruffleConnector \
--java_out=$(pwd)/../java/core/src/main/java \
$(pwd)/../proto/*.proto
