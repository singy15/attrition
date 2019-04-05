setlocal

ais title-all %* > tmp
type tmp | less -R -F -X

endlocal
exit /b

