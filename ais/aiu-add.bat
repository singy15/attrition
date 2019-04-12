setlocal

set BASEPATH=%~dp0
ais new %* > %BASEPATH%tmp
set ID=%ERRORLEVEL%
vim --not-a-term -u %BASEPATH%.vimrc_utf8 %BASEPATH%tmp
if %ERRORLEVEL% == 0 (
    type %BASEPATH%tmp | ais ins %ID%
    ais show %ID% > %BASEPATH%tmp
    type %BASEPATH%tmp | less -R -F -X
)

endlocal
exit /b

