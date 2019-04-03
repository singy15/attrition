setlocal

ais ls-all %* > tmp
type tmp | less -R -F

endlocal
exit /b

