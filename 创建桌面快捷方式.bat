@echo off
chcp 65001 >nul
echo.
echo ========================================
echo    创建桌面快捷方式
echo ========================================
echo.

REM 检查程序是否已编译
if not exist "src\bin\Release\net10.0-windows\JsonToPngConverter.exe" (
    echo 程序尚未编译，正在编译...
    echo.
    
    cd src
    
    echo [1/2] 还原依赖包...
    "C:\Program Files\dotnet\dotnet.exe" restore --force --nologo
    if errorlevel 1 (
        echo ✗ 还原失败
        cd ..
        pause
        exit /b 1
    )
    
    echo [2/2] 编译程序...
    "C:\Program Files\dotnet\dotnet.exe" build -c Release --nologo
    if errorlevel 1 (
        echo ✗ 编译失败
        cd ..
        pause
        exit /b 1
    )
    
    cd ..
    
    if not exist "src\bin\Release\net10.0-windows\JsonToPngConverter.exe" (
        echo.
        echo ✗ 编译失败，无法创建快捷方式
        pause
        exit /b 1
    )
)

REM 获取当前目录
set "CURRENT_DIR=%~dp0"
set "EXE_PATH=%CURRENT_DIR%src\bin\Release\net10.0-windows\JsonToPngConverter.exe"
set "DESKTOP=%USERPROFILE%\Desktop"
set "SHORTCUT=%DESKTOP%\LanhuToPng.lnk"

REM 使用 PowerShell 创建快捷方式
powershell -Command "$WS = New-Object -ComObject WScript.Shell; $SC = $WS.CreateShortcut('%SHORTCUT%'); $SC.TargetPath = '%EXE_PATH%'; $SC.WorkingDirectory = '%CURRENT_DIR%'; $SC.Description = '蓝湖原型转PNG工具'; $SC.Save()"

if exist "%SHORTCUT%" (
    echo.
    echo ✓ 快捷方式已创建到桌面
    echo.
    echo 现在可以直接双击桌面的 LanhuToPng 快捷方式启动程序了！
) else (
    echo.
    echo ✗ 创建快捷方式失败
)

echo.
pause
