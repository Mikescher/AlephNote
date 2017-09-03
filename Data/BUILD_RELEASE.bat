@REM ================ BUILD ================

cd ..
cd Source
nuget restore


echo "Please build RELEASE"
PAUSE
REM dotnet msbuild /t:Clean /p:Configuration=Release /verbosity:m
REM dotnet msbuild /t:Build /p:Configuration=Release /verbosity:m

@REM ================ CLEAN ================

cd ..
cd Bin
cd Release
cd net46

del /s *.pdb
del /s *.vshost.exe
del /s *.manifest
del /s *.xml
del /s *.config
if exist .notes rd /S /Q .notes
cd Plugins
del /s *.pdb
del /s *.vshost.exe
del /s *.manifest
del /s *.xml
cd ..

@REM ================ PACKAGE ================

cd ..
cd ..
cd ..

if exist AlephNote.zip del AlephNote.zip

cd Data

7za.exe a .\..\AlephNote.zip .\..\Bin\Release\net46\*

@REM ================ FINISHED ================

echo "Finished successfully"
PAUSE

