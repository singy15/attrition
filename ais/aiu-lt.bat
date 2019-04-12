setlocal

set BASEPATH=%~dp0
ais title %* > %BASEPATH%tmp
type %BASEPATH%tmp | less -R -F -X

endlocal
exit /b

