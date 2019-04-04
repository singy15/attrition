setlocal

ais get %1 > tmp
vim --not-a-term -u .vimrc_utf8 tmp
if %ERRORLEVEL% == 0 (
    type tmp | ais set %1
    ais show %1 > tmp
    type tmp | less -R -F -X
)

endlocal
exit /b

