@echo off

for /D %%i in ("%TEMP%\Samples_Blazor_Server*") do (
  rmdir /S /Q "%%i"
)
