@echo off

ECHO Run docker container adventureworkslt...
docker run --name adventureworkslt --rm -p3306:3306 -d adventureworkslt

ECHO.
ECHO Waiting for server up and running...
:loop
timeout /t 1 >nul
docker inspect -f '{{.State.Health.Status}}' adventureworkslt | find "healthy" >nul
if errorlevel 1 goto :loop