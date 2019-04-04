setlocal

ais ls-all %* > tmp
type tmp | less -R -F -X

endlocal
exit /b

