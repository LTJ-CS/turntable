#!/bin/bash

WORKSPACE=$(cd "$(dirname $0)";pwd -P)
LUBAN_DLL=$WORKSPACE/luban/Tools/Luban/Luban.dll


function genData() {
echo "\n【生成客户端使用的数据表】开始"
dotnet $LUBAN_DLL \
        -t client \
        -d json \
        -c cs-simple-json \
        --conf $WORKSPACE/luban/luban.conf \
        -x outputDataDir=../turntable/client/assets/GameRes/Runtime/ConfigData \
        -x outputCodeDir=../turntable/client/assets/GameScript/Runtime/ConfigScript
echo "\n【生成客户端使用的数据表】结束"

if [ -d "../screw-puzzle-server-orleans" ]; then
        echo "\n【生成服务器使用的数据表】开始"
        absolute_path=$(readlink -f ../screw-puzzle-server-orleans/Grains/DataTables/Tables)
        dotnet $LUBAN_DLL \
                -t server \
                -d json \
                -c cs-dotnet-json \
                --conf $WORKSPACE/luban/luban.conf \
                -x outputDataDir=../screw-puzzle-server-orleans/Grains/DataTables/Tables \
                -x outputCodeDir=../screw-puzzle-server-orleans/Grains/DataTables/Gen
        echo "\n【生成服务器使用的数据表】结束"

        echo "\n【生成golang服务器使用的数据表】开始"
        absolute_path=$(readlink -f ../screw-puzzle-server/cfg/tables)
        dotnet $LUBAN_DLL \
                -t golang \
                -d json \
                -c go-json \
                --conf $WORKSPACE/luban/luban.conf \
                -x outputDataDir=../screw-puzzle-server/cfg/tables \
                -x outputCodeDir=../screw-puzzle-server/cfg/gen \
                -x lubanGoModule=../screw-puzzle-server/
        echo "\n【生成golang服务器使用的数据表】结束"
fi
}

# 导出数据表
genData