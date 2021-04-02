@echo off

set BUILDTOOL_PUBLISH_PATH=R136.BuildTool\bin\Debug\net5.0\publish\R136.BuildTool.exe
set BUILDTOOL_BUILD_PATH=R136.BuildTool\bin\Debug\net5.0\R136.BuildTool.exe

setlocal enableextensions

pushd %~dp0 

set BUILDTOOL_PATH=

if exist %BUILDTOOL_PUBLISH_PATH% (
	echo BuildTool found at publish path
	set BUILDTOOL_PATH=%BUILDTOOL_PUBLISH_PATH%
) else (
	if exist %BUILDTOOL_BUILD_PATH% (
		echo BuildTool found at build path
		set BUILDTOOL_PATH=%BUILDTOOL_BUILD_PATH%
	)
)

if defined BUILDTOOL_PATH (
	%BUILDTOOL_PATH% %* .\conversions.json
) else (
	echo BuildTool not available at %BUILDTOOL_PUBLISH_PATH% or %BUILDTOOL_BUILD_PATH%, skipping JSON conversion.
)

popd