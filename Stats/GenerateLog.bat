@echo off

echo Extracting log in "%~1"

cd "%~1"
rem svn cleanup
echo Updating branch...
svn update
echo Extracting log...
svn log --xml -v --non-interactive > svn.log

rem Pause to debug...
rem pause