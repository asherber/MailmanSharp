@echo off
@set msbuild_path="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin"
pushd ..
%msbuild_path%\msbuild /v:m /p:Configuration=Release
popd
echo.
nuget pack 