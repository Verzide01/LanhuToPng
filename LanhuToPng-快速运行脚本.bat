@echo off
chcp 65001 >nul

REM ========================================
REM  LanhuToPng - 蓝湖原型转PNG/PDF工具
REM ========================================

REM 检查是否已编译
if exist "src\bin\Release\net10.0-windows\JsonToPngConverter.exe" (
    REM 已编译，直接运行
    start "" "src\bin\Release\net10.0-windows\JsonToPngConverter.exe"
    exit /b 0
)

REM 未编译，先编译
echo ========================================
echo   首次运行，正在编译程序...
echo ========================================
echo.

cd src

echo [1/2] 还原依赖包...
"C:\Program Files\dotnet\dotnet.exe" restore --force --nologo
if errorlevel 1 (
    echo.
    echo ✗ 还原失败，请检查网络连接
    cd ..
    pause
    exit /b 1
)

echo [2/2] 编译程序...
"C:\Program Files\dotnet\dotnet.exe" build -c Release --nologo
if errorlevel 1 (
    echo.
    echo ✗ 编译失败
    cd ..
    pause
    exit /b 1
)

cd ..

if exist "src\bin\Release\net10.0-windows\JsonToPngConverter.exe" (
    echo.
    echo ========================================
    echo   ✓ 编译完成！启动程序...
    echo ========================================
    echo.
    timeout /t 2 >nul
    start "" "src\bin\Release\net10.0-windows\JsonToPngConverter.exe"
) else (
    echo.
    echo ✗ 编译失败，未找到可执行文件
    pause
    exit /b 1
)
