@echo off

REM To remove the confusion with setup.exe, between 2.0.4 and 2.10, the name of the MSI was changed from Setup.MSI to VRDB.MSI
REM This name change causes it to not be able to automatically uninstall 2.0.4 (or earlier) when 2.1.0 (or later) is installed.
REM To accomodate this, this batch file is invoked silently from upgrade.bat to handle the swapping out the database,
REM handlig the uninstall and install, and then swapping the database back in.

REM Get the install paths
REM reg query HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\Folders /f "VRDB"


SET tasklist=%WINDIR%\System32\tasklist.exe
SET taskkill=%WINDIR%\System32\taskkill.exe

GOTO MAIN

-------------------------------------------------------
:STOPPROC
ECHO Stopping %procName%
    SET wasStopped=0
    SET procFound=0
    SET notFound_result=ERROR:
    SET procName=%1
    FOR /f "usebackq" %%A in (`%taskkill% /IM %procName% /F`) do (
      IF NOT %%A==%notFound_result% (SET procFound=1)
    )
    if %procFound%==0 (
      ECHO The process was not running.
      GOTO :EOF
    )
    SET wasStopped=1
    SET ignore_result=INFO:
:CHECKDEAD
    "%WINDIR%\system32\timeout.exe" 3 /NOBREAK
    FOR /f "usebackq" %%A in (`%tasklist% /nh /fi "imagename eq %procName%"`) do (
      IF not %%A==%ignore_result% (GOTO :CHECKDEAD)
    )
    GOTO :EOF
-------------------------------------------------------

:MAIN 

REM These are killed in reverse order -- not sure why
CALL :STOPPROC sqlservr.exe
CALL :STOPPROC vrdb.exe

:SWAP
REM Move the DB files out of the way
ECHO Moving database files out of the way...
MOVE "%APPDATA%\Advanced Applications\VRDB\VRDB*.*" %TEMP% > nul

REM Uninstall the old VRDB
ECHO Removing the old version...
msiexec /q /x {5A3EF7DD-DB41-41C1-A5B0-C667C5CF2F2F}

REM Install the new VRDB
ECHO Installing the new version...
msiexec /i VRDB.msi

REM Put the DB files back
ECHO Restoring the database files...
MOVE /Y %TEMP%\VRDB*.* "%APPDATA%\Advanced Applications\VRDB" > nul
