@echo off
ROBOCOPY "%~dp0..\src\Data.DbInit" "%~dp0DevZest.Data.DbInit" /MIR /XD .vs obj bin
