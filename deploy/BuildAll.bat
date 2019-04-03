@echo off

call "%~dp0Sync.bat"

cd "%~dp0DevZest.Data"
call ..\Build.bat %1 %2

cd "%~dp0DevZest.Data.SqlServer"
call ..\Build.bat %1 %2

cd "%~dp0DevZest.Data.WPF"
call ..\BuildWpf.bat %1 %2

cd "%~dp0DevZest.Data.MySql"
call ..\Build.bat %1 %2

cd "%~dp0DevZest.Data.AspNetCore"
call ..\Build.bat %1 %2
