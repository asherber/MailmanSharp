@echo off
@set msbuild_path="C:\Program Files (x86)\MSBuild\12.0\bin"
pushd ..
%msbuild_path%\msbuild /v:m /p:Configuration=Release
popd
echo.
nuget pack 