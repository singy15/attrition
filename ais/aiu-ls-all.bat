setlocal

set BASEPATH=%~dp0
ais ls-all %* > %BASEPATH%tmp
type %BASEPATH%tmp | less -R -F -X

endlocal
exit /b

