@echo off

setlocal enabledelayedexpansion

set ARGS=
set POS=cmd
for %%a in (%*) do (
  if "!pos!"=="cmd" (
    set COMMAND=%%~a
    set POS=arg
  ) else if "!pos!"=="arg" (
    set ARGS=!ARGS! %%a
  )
)

call aiu-%COMMAND%%ARGS%

endlocal

