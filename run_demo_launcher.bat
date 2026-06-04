@echo off
setlocal
cd /d "%~dp0"
if exist "RIHT_demo_launcher.exe" (
  start "" "%~dp0RIHT_demo_launcher.exe"
) else (
  powershell -ExecutionPolicy Bypass -File "%~dp0build_launcher.ps1"
  start "" "%~dp0RIHT_demo_launcher.exe"
)
