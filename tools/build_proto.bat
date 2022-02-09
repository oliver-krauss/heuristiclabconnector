@echo off
setlocal enabledelayedexpansion

:: change working directory to tools directory
cd %~dp0

:: determine absolute path to proto files
pushd %cd%\..\proto
set protopath=%cd%
popd

:: get all files in proto directory
set protofiles=
for /F "tokens=*" %%F in ('dir /B /S %protopath%\*.proto') do (
    set "protofiles=!protofiles! %%F"
)

:: remove existing generated files
call clean_proto.bat

:: compile proto files with protoc
%cd%\protoc\bin\protoc ^
-I=%protopath% ^
-I=protoc\include ^
--csharp_out=%cd%\..\csharp\HeuristicLab.TruffleConnector\HeuristicLab.TruffleConnector\3.3 ^
--csharp_opt=file_extension=.g.cs,base_namespace=HeuristicLab.TruffleConnector ^
--java_out=%cd%\..\java\core\src\main\java ^
!protofiles!
