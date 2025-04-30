#!/bin/bash

# 检查参数数量
if [ "$#" -ne 8 ]; then
    echo "请提供Unity项目路径、日志文件路径、目标平台、appId、appVersion、resVersion、运行环境、uid作为参数"
    exit 1
fi

# 获取参数
PROJECT_PATH="$1"
LOG_PATH="$2"
PLATFORM="$3"
APP_ID="$4"
APP_VERSION="$5"
RES_VERSION="$6"
ENV="$7"
MYUID="$8"

# 定义Unity可执行文件的路径 
UNITY_EXE="/Applications/Unity/Hub/Editor/2021.3.14f1c1/Unity.app/Contents/MacOS/Unity"

# 执行Unity命令
"$UNITY_EXE" -batchmode -projectPath "$PROJECT_PATH" -logFile "$LOG_PATH" -buildTarget WebGL -executeMethod Sdk.Editor.Scripts.Addressable.CustomBuildWindow.BuildMiniGame -platform "$PLATFORM" -appId "$APP_ID" -appVersion "$APP_VERSION" -resVersion "$RES_VERSION" -env "$ENV" -uid "$MYUID"

# 检查返回码
if [ $? -ne 0 ]; then
    echo "Unity项目打包失败!"
    exit 1
fi

echo "Unity项目打包成功!"
