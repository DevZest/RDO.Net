@echo off
setlocal
FOR %%a IN (.) DO SET currentfolder=%%~nxa
CALL %~dp0Sync.%currentfolder%.bat

SET PARAM_VERSION="-version %1"
if "%1"=="" (SET PARAM_VERSION=)
SET PARAM_ADDTIONAL_LABEL="-additionalLabel %2"
if "%2"=="" (SET PARAM_ADDTIONAL_LABEL=)
CALL %~dp0VerGen.%currentFolder%.bat
