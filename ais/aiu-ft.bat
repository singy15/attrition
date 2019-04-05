setlocal

ais find-title %* > tmp
type tmp | less -R -F -X

endlocal
exit /b

