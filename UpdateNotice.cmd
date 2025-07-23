dotnet tool update TomsToolbox.LicenseGenerator --global
build-license -i "%~dp0src\JsonToResX.sln" -o "%~dp0Notice.txt"
