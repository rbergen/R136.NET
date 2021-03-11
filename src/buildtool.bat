@echo off

pushd %~dp0 

if exist R136.BuildTool\bin\Debug\net5.0\R136.BuildTool.exe (
	R136.BuildTool\bin\Debug\net5.0\R136.BuildTool.exe %* .\conversions.json
) else (
	echo "BuildTool not available at R136.BuildTool\bin\Debug\net5.0\R136.BuildTool.exe, skipping JSON conversion."
)

popd