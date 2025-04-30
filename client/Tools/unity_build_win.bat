@echo off
setlocal

:: 检查参数数量
if "%~8"=="" (
    echo 请提供Unity项目路径、日志文件路径、目标平台、appId、appVersion、resVersion、运行环境、uid作为参数
    exit /b 1
)

:: 获取参数
set PROJECT_PATH=%1
set LOG_PATH=%2
set PLATFORM=%3
set APP_ID=%4
set APP_VERSION=%5
set RES_VERSION=%6
set ENV=%7
set MYUID=%8

:: 定义Unity可执行文件的路径
set UNITY_EXE="C:\Program Files\Unity\Hub\Editor\2021.3.14f1c1\Editor\Unity.exe"

:: 执行Unity命令
%UNITY_EXE% -batchmode -projectPath %PROJECT_PATH% -logFile %LOG_PATH% -buildTarget WebGL -executeMethod Sdk.Editor.Scripts.Addressable.CustomBuildWindow.BuildMiniGame -platform %PLATFORM% -appId %APP_ID% -appVersion %APP_VERSION% -resVersion %RES_VERSION% -env %ENV% -uid %MYUID%

:: 检查返回码
if %ERRORLEVEL% neq 0 (
    echo Unity项目打包失败! 错误码: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo Unity项目打包成功!
exit /b 0
