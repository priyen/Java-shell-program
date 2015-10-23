@echo off

echo Generating stats for "%~2"

echo 1: Stats Path = "%~1"
echo 2: Checkout Path = "%~2"
echo 3: Log File = "%~3"

echo Generating using StatSVN 0.7.0...
java -jar .\statsvn-0.7.0\statsvn.jar -output-dir "%~1" -title "%~1" "%~3" "%~2"

echo.
echo Generating using SvnStat 1.0...
java -classpath .\SvnStat-1.0\SvnStat-all.jar de.agentlab.svnstat.SvnStat -jar .\SvnStat-1.0\SvnStat-all.jar -r "%~3" -d "%~1\more"

rem Pause to debug...
rem pause