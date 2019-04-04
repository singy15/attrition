setlocal

ais find %* > tmp
type tmp | less -R -F -X

endlocal
exit /b

