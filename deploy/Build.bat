@echo off
CALL sync_src.bat
CD sync_src

setlocal
SET PARAM_VERSION="-version %1"
if "%1"=="" (SET PARAM_VERSION=)
SET PARAM_ADDTIONAL_LABEL="-additionalLabel %2"
if "%2"=="" (SET PARAM_ADDTIONAL_LABEL=)
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "%~dp0build.ps1" %PARAM_VERSION% %PARAM_ADDTIONAL_LABEL%
