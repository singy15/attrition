setlocal

set BASEPATH=%~dp0
ais show %1 > %BASEPATH%tmp
type %BASEPATH%tmp

endlocal
exit /b

