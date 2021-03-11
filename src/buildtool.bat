@echo off

set BUILDTOOL_PATH=R136.BuildTool\bin\Debug\net5.0\R136.BuildTool.exe

pushd %~dp0 

if exist %BUILDTOOL_PATH% (
	%BUILDTOOL_PATH% %* .\conversions.json
) else (
	echo BuildTool not available at %BUILDTOOL_PATH%, skipping JSON conversion.
)

popd