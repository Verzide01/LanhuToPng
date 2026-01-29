@echo off
chcp 65001 >nul
echo.
echo ========================================
echo    打包分发版本
echo ========================================
echo.

REM 检查是否需要先发布
if not exist "src\bin\Publish\JsonToPngConverter.exe" (
    echo 正在发布独立版本...
    echo.
    
    cd src
    
    echo [1/2] 还原依赖包...
    "C:\Program Files\dotnet\dotnet.exe" restore --force --nologo >nul 2>&1
    if errorlevel 1 (
        echo ✗ 还原失败
        cd ..
        pause
        exit /b 1
    )
    
    echo [2/2] 发布独立版本...
    "C:\Program Files\dotnet\dotnet.exe" publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o bin\Publish --nologo >nul 2>&1
    if errorlevel 1 (
        echo ✗ 发布失败
        cd ..
        pause
        exit /b 1
    )
    
    cd ..
    echo ✓ 发布完成
    echo.
)

REM 检查 Publish 文件夹是否存在
if not exist "src\bin\Publish\JsonToPngConverter.exe" (
    echo 错误: 未找到发布的 exe 文件
    echo.
    pause
    exit /b 1
)

REM 检查 override.zip 是否存在
if not exist "override.zip" (
    echo 错误: 未找到 override.zip 文件
    echo 该文件是程序运行的必需文件
    echo.
    pause
    exit /b 1
)

REM 创建分发目录
set "DIST_DIR=LanhuToPng_分发版"
if exist "%DIST_DIR%" rmdir /s /q "%DIST_DIR%"
mkdir "%DIST_DIR%"

echo [1/3] 复制可执行文件...
copy "src\bin\Publish\JsonToPngConverter.exe" "%DIST_DIR%\" >nul
echo OK

echo [2/3] 复制资源文件...
copy "override.zip" "%DIST_DIR%\" >nul
echo OK

echo [3/3] 创建使用说明...
echo LanhuToPng - 蓝湖原型转PNG/PDF工具 > "%DIST_DIR%\使用说明.txt"
echo. >> "%DIST_DIR%\使用说明.txt"
echo 使用方法： >> "%DIST_DIR%\使用说明.txt"
echo 1. 双击 JsonToPngConverter.exe 启动程序 >> "%DIST_DIR%\使用说明.txt"
echo 2. 选择 JSON 文件和输出目录 >> "%DIST_DIR%\使用说明.txt"
echo 3. 点击一键完成按钮 >> "%DIST_DIR%\使用说明.txt"
echo. >> "%DIST_DIR%\使用说明.txt"
echo 功能说明： >> "%DIST_DIR%\使用说明.txt"
echo - 下载 HTML >> "%DIST_DIR%\使用说明.txt"
echo - 转换 PNG >> "%DIST_DIR%\使用说明.txt"
echo - 转换 PDF >> "%DIST_DIR%\使用说明.txt"
echo - 一键完成所有步骤 >> "%DIST_DIR%\使用说明.txt"
echo. >> "%DIST_DIR%\使用说明.txt"
echo 注意事项： >> "%DIST_DIR%\使用说明.txt"
echo - 首次运行会自动下载 Chromium 浏览器 >> "%DIST_DIR%\使用说明.txt"
echo - override.zip 文件必须和 exe 在同一目录 >> "%DIST_DIR%\使用说明.txt"
echo - 需要网络连接才能下载蓝湖原型 >> "%DIST_DIR%\使用说明.txt"
echo. >> "%DIST_DIR%\使用说明.txt"
echo 系统要求： >> "%DIST_DIR%\使用说明.txt"
echo - Windows 10/11 64位 >> "%DIST_DIR%\使用说明.txt"
echo - 无需安装 .NET 运行时 >> "%DIST_DIR%\使用说明.txt"
echo. >> "%DIST_DIR%\使用说明.txt"
echo 输出文件： >> "%DIST_DIR%\使用说明.txt"
echo - html/ - 下载的 HTML 文件 >> "%DIST_DIR%\使用说明.txt"
echo - png/ - 转换的 PNG 截图 >> "%DIST_DIR%\使用说明.txt"
echo - 原型截图.pdf - 合并的 PDF 文档 >> "%DIST_DIR%\使用说明.txt"
echo OK

echo.
echo ========================================
echo    打包完成！
echo ========================================
echo.
echo 分发包位置: %DIST_DIR%\
echo.
echo 包含文件:
echo   - JsonToPngConverter.exe
echo   - override.zip
echo   - 使用说明.txt
echo.
echo 可以将整个文件夹压缩成 zip 分发
echo.
pause
