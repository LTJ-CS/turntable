@echo off

set WORKSPACE=%~dp0
set LUBAN_DLL=%WORKSPACE%\luban\Tools\Luban\Luban.dll

echo.
echo 【生成客户端使用的数据表】开始
dotnet "%LUBAN_DLL%" ^
-t client ^
-d json ^
-c cs-simple-json ^
--conf "%WORKSPACE%\luban\luban.conf" ^
-x outputDataDir=..\screw-puzzle-client\client\assets\GameRes\Runtime\ConfigData ^
-x outputCodeDir=..\screw-puzzle-client\client\assets\GameScript\Runtime\ConfigScript
echo.
echo 【生成客户端使用的数据表】结束

pause