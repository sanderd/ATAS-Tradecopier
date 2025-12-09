@echo off
echo ========================================
echo  ATAS Trade Copy System - Local Build
echo ========================================
echo.

REM Check if PowerShell execution policy allows scripts
powershell -Command "Get-ExecutionPolicy" | findstr /i "restricted" >nul
if %errorlevel% == 0 (
    echo WARNING: PowerShell execution policy is Restricted.
    echo You may need to run: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
    echo.
    pause
)

REM Run the PowerShell script with parameters
if "%1"=="" (
    powershell -ExecutionPolicy Bypass -File "publish-local.ps1"
) else (
    powershell -ExecutionPolicy Bypass -File "publish-local.ps1" -Tag "%1"
)

pause