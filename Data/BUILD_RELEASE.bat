@REM ================ BUILD ================

cd ..
cd Source
nuget restore
msbuild /nologo /t:Clean /p:Configuration=Release /verbosity:m
msbuild /nologo /t:Build /p:Configuration=Release /verbosity:m

@REM ================ CLEAN ================

cd ..
cd Bin
cd Release
cd Windows

del /s *.pdb
del /s *.vshost.exe
del /s *.manifest
del /s *.xml
if exist .notes del .notes
cd Plugins
del /s *.pdb
del /s *.vshost.exe
del /s *.manifest
del /s *.xml

@REM ================ PACKAGE ================

cd ..
cd ..
cd ..
cd ..

if exist AlephNote.zip del AlephNote.zip

cd Data

7za.exe a .\..\AlephNote.zip .\..\Bin\Release\Windows\*

@REM ================ FINISHED ================

echo "Finished successfully"
PAUSE

