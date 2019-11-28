@echo off
setlocal
SET project=Tools.Vsix
FOR %%a IN (.) DO SET solutionname=%%~nxa
CALL "%~dp0Sync.%project%.bat"

@echo Deleting all BIN and OBJ folders...
for /d /r "%~dp0" %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"

SET PARAM_VERSION="-version %1"
SET PARAM_PROJECT="-project %project%"
if "%1"=="" (SET PARAM_VERSION=)
CALL "%~dp0VerGen.bat"

for /f "usebackq tokens=*" %%i in (`vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
  set msBuildPath="%InstallDir%\MSBuild\15.0\Bin\
  set msBuildExe="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"
) else (
  set msBuildPath="%InstallDir%\MSBuild\Current\Bin\
  set msBuildExe="%InstallDir%\MSBuild\Current\Bin\MSBuild.exe"
)

echo.
echo msBuildPath=%msBuildPath%
echo msBuildExe=%msBuildExe%
echo.

call nuget restore "%~dp0%solutionname%.sln" -MSBuildPath %msBuildPath%
call %msBuildExe% "%~dp0%solutionname%.sln" /t:build /p:Configuration=Release /verbosity:m
@IF %ERRORLEVEL% NEQ 0 PAUSE
