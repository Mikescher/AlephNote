REM ================ BUILD ================

cd ..
cd Source
nuget restore
msbuild /t:Build /nologo /t:Build /p:Configuration=Release /verbosity:m

REM ================ CLEAN ================

cd ..
cd Bin
cd Release

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

REM ================ PACKAGE ================

cd ..
cd ..
cd ..

if exist AlephNote.zip del AlephNote.zip

cd Data

7za.exe a .\..\AlephNote.zip .\..\Bin\Release\*

REM ================ FINISHED ================

echo "Finished successfully"
PAUSE

