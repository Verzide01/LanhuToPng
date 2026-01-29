@echo off
chcp 65001 >nul
echo.
echo ========================================
echo    重新编译 LanhuToPng
echo ========================================
echo.

cd src

echo [1/3] 清理旧文件...
if exist bin\Release rmdir /s /q bin\Release
if exist obj rmdir /s /q obj
echo OK
echo.

echo [2/3] 还原依赖包...
"C:\Program Files\dotnet\dotnet.exe" restore --force --nologo
if errorlevel 1 (
    echo FAILED
    cd ..
    pause
    exit /b 1
)
echo OK
echo.

echo [3/3] 编译程序...
"C:\Program Files\dotnet\dotnet.exe" build -c Release --nologo
if errorlevel 1 (
    echo FAILED
    cd ..
    pause
    exit /b 1
)
echo OK
echo.

cd ..

echo ========================================
echo    编译完成！
echo ========================================
echo.
echo 可执行文件: src\bin\Release\net10.0-windows\JsonToPngConverter.exe
echo.
echo 现在可以：
echo   1. 双击 LanhuToPng.bat 启动程序
echo   2. 或直接运行 src\bin\Release\net10.0-windows\JsonToPngConverter.exe
echo.
pause
