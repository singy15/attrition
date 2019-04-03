setlocal

ais ls %* > tmp
type tmp | less -R -F

endlocal
exit /b

