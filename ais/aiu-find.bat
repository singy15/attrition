setlocal

ais find %* > tmp
type tmp | less -R -F

endlocal
exit /b

