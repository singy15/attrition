setlocal

ais show %1 > tmp
type tmp

endlocal
exit /b

