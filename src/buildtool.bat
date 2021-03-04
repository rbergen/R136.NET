@echo off
pushd %~dp0 
R136.BuildTool\bin\Debug\net5.0\R136.BuildTool.exe %* .\conversions.json
popd