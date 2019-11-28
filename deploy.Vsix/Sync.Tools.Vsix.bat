ROBOCOPY "%~dp0..\src\Data.DbInit" "%~dp0Data.DbInit" /MIR /XD .vs obj bin
ROBOCOPY "%~dp0..\src\Tools.Vsix.CodeAnalysis" "%~dp0Tools.Vsix.CodeAnalysis" /MIR /XD .vs obj bin
ROBOCOPY "%~dp0..\src\Tools.Vsix" "%~dp0Tools.Vsix" /MIR /XD .vs obj bin