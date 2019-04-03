setlocal

ais new > tmp
set ID=%ERRORLEVEL%
vim --not-a-term -u .vimrc_utf8 tmp
if %ERRORLEVEL% == 0 (
    type tmp | ais ins %ID%
    ais show %ID% > tmp
    type tmp | less -R -F
)

endlocal
exit /b

