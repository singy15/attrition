setlocal

set BASEPATH=%~dp0
ais get %1 > %BASEPATH%tmp
vim --not-a-term -u %~dp0.vimrc_utf8 %BASEPATH%tmp
if %ERRORLEVEL% == 0 (
    type %BASEPATH%tmp | ais set %1
    ais show %1 > %BASEPATH%tmp
    type %BASEPATH%tmp | cat
)

endlocal
exit /b

