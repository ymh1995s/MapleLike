XCOPY /Y ServerContents\Room\Data\Maps.Json "ServerContents\bin\Debug\net8.0\"
XCOPY /Y ServerContents\Room\Data\Monsters.Json "ServerContents\bin\Debug\net8.0\"
IF ERRORLEVEL 1 PAUSE
