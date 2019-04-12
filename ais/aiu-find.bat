setlocal

set BASEPATH=%~dp0
ais find %* > %BASEPATH%tmp
type %BASEPATH%tmp | less -R -F -X

endlocal
exit /b

