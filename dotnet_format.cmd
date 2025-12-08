@echo off

set libraryPath=Libraries/quack.shadergraphplus

if not exist %libraryPath% (
	
	echo directory "Libraries/quack.shadergraphplus does not exist...
	
	echo.
	
	pause
	exit
)

pushd "%libraryPath%/Editor"

echo running dotnet format on code in directory "%cd%"

dotnet format

popd