@ECHO OFF
@REM ================ BUILD ================

cd ..

del AlephNote.zip

cd Bin
cd Release
cd net47

rmdir de /S /Q
rmdir es /S /Q
rmdir fr /S /Q
rmdir hu /S /Q
rmdir it /S /Q
rmdir pt-BR /S /Q
rmdir ro /S /Q
rmdir ru /S /Q
rmdir sv /S /Q
rmdir zh-Hans /S /Q

del /q *.pdb
del /q *.vshost.exe
del /q *.manifest
del /q *.xml
del /q *.config
if exist .notes rd /S /Q .notes
cd Plugins
del /q *.pdb
del /q *.vshost.exe
del /q *.manifest
del /q *.xml
cd ..

@REM ================ PACKAGE ================

cd ..
cd ..
cd ..

if exist AlephNote.zip del AlephNote.zip

cd Data

7za.exe a .\..\AlephNote.zip .\..\Bin\Release\net47\*

@REM ================ FINISHED ================

echo "Finished successfully"
