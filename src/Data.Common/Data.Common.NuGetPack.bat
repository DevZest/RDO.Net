@echo off
cd %~dp0

SETLOCAL
FOR %%c in (*.*proj) DO (call :set_proj_file %%c)
SET NUGET_VERSION=latest
SET CACHED_NUGET=%LocalAppData%\NuGet\nuget.%NUGET_VERSION%.exe
SET OUTPUT_DIR=.\bin\NuGet
SET NUGET_EXE=%OUTPUT_DIR%\nuget.exe

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/%NUGET_VERSION%/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST %NUGET_EXE% goto pack
IF NOT EXIST %OUTPUT_DIR% md %OUTPUT_DIR%
copy %CACHED_NUGET% %NUGET_EXE% > nul

:pack
echo Building NuGet package for %PROJ_FILE%...
%NUGET_EXE% pack %PROJ_FILE% -Build -Symbols -Properties Configuration=Release -OutputDirectory %OUTPUT_DIR%

PAUSE
goto :eof

:set_proj_file
IF NOT DEFINED PROJ_FILE goto set_proj_file_var
echo ERROR****: Multiple project files found for *.*proj.
PAUSE
exit 1
:set_proj_file_var
SET PROJ_FILE=%1
goto :eof

