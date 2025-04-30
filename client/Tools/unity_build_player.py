# -*- coding: utf-8 -*-

import subprocess
import sys

# python unity_build_player.py ../../client unity_build.log wx wx55b3d3e44b6d16ef 1.0 118 Beta
# python unity_build_player.py ../../client unity_build.log tt tt220154bcbd371b8707 1.0 118 Beta

# project_path = '/Users/raven/Desktop/workspace/screw-puzzle-client/client'
# log_path = '/Users/raven/Desktop/workspace/screw-puzzle-client/unity_build.log'

def build_unity_project(project_path, log_path, platform, appId, appVersion, resVersion, evn,uid):
    # 定义Unity可执行文件的路径
    unity_exe = '/Applications/Unity/Hub/Editor/2021.3.14f1c1/Unity.app/Contents/MacOS/Unity'

    # 定义要执行的Unity方法
    build_method = 'Sdk.Editor.Scripts.Addressable.CustomBuildWindow.BuildMiniGame'

    # 构建Unity命令
    unity_command = [
        unity_exe,
        '-batchmode',  # 批处理模式，无需图形界面
        '-projectPath', project_path,
        '-logFile', log_path,
        '-buildTarget', 'WebGL',
        '-executeMethod', build_method,
        '-platform', platform,
        '-appId', appId,
        '-appVersion', appVersion,
        '-resVersion', resVersion,
        '-env', evn,
        '-uid', uid,
    ]

    # 使用subprocess模块执行Unity命令
    try:
        # 执行命令并捕获输出和错误
        process = subprocess.Popen(unity_command, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        stdout, stderr = process.communicate()

        # 检查返回码以判断是否成功
        if process.returncode != 0:
            print("Unity项目打包失败!")
            print("错误码: %s" % process.returncode)
            print("输出信息: %s" % stdout)  # 直接输出字节串
            print("标准错误输出: %s" % stderr)  # 直接输出字节串
            sys.exit(1)  # 退出程序并返回非零值以表示失败
        else:
            print("Unity项目打包成功!")
            print(stdout)  # 可选：打印标准输出

    except Exception as e:
        print("执行过程中发生异常: %s" % str(e))
        sys.exit(1)

if __name__ == "__main__":
    print(sys.argv)
    if len(sys.argv) < 9:
        print("请提供Unity项目路径、日志文件路径、目标平台、版本号、资源号、运行环境、uid作为参数")
        sys.exit(1)

    project_path = sys.argv[1]
    log_path = sys.argv[2]
    platform = sys.argv[3]
    appId = sys.argv[4]
    appVersion = sys.argv[5]
    resVersion = sys.argv[6]
    env = sys.argv[7]
    uid = sys.argv[8]
    build_unity_project(project_path, log_path, platform, appId, appVersion, resVersion, env,uid)
