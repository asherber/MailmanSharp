@setlocal
@set msbuild_path="C:\Program Files (x86)\MSBuild\12.0\bin\"
@set path=%msbuild_path%;%path%

msbuild /p:Configuration=Release
nuget pack -Prop Configuration=Release