setlocal

ais ls %* > tmp
type tmp | less -R -F -X

endlocal
exit /b

