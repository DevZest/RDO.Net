@echo off
setlocal
FOR %%a IN (.) DO SET currentfolder=%%~nxa
CALL "%~dp0Sync.%currentfolder%.bat"

@echo Deleting all BIN and OBJ folders...
for /d /r "%~dp0" %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"

SET PARAM_VERSION="-version %1"
if "%1"=="" (SET PARAM_VERSION=)
SET PARAM_ADDTIONAL_LABEL="-additionalLabel %2"
if "%2"=="" (SET PARAM_ADDTIONAL_LABEL=)
CALL "%~dp0VerGen.%currentFolder%.bat"

for /f "usebackq tokens=*" %%i in (`vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
  set msBuildExe="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"
)

echo.
echo msBuildExe=%msBuildExe%
echo.

call nuget restore "%~dp0Deploy.%currentFolder%.sln"
call %msBuildExe% "%~dp0Deploy.%currentFolder%.sln" /t:build /p:Configuration=Release /verbosity:m
@IF %ERRORLEVEL% NEQ 0 PAUSE
