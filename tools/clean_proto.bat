@echo off

:: change working directory to tools directory
cd %~dp0

:: delete generated files
del /S /Q %cd%\..\java\core\src\main\java\at\fh\hagenberg\aist\hlc\core\messages\*
del /S /Q %cd%\..\csharp\HeuristicLab.TruffleConnector\HeuristicLab.TruffleConnector\3.3\Messages\*

