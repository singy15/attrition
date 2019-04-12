setlocal

set BASEPATH=%~dp0
ais title-all %* > %BASEPATH%tmp
type %BASEPATH%tmp | less -R -F -X

endlocal
exit /b

