@echo off

for /D %%i in ("%TEMP%\Stl_Fusion_Samples_Blazor_Server*") do (
  rmdir /S /Q "%%i"
)
