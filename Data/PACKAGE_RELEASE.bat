@ECHO OFF
@REM ================ BUILD ================

cd ..

if exist AlephNote.zip del AlephNote.zip

cd Bin

if exist Package rd /S /Q Package

xcopy Release\ Package\ /s /e /y

cd Package

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
rmdir cs-CZ /S /Q
rmdir ja-JP /S /Q

rename AlephNote.App.exe AlephNote.exe

del /q *.pdb
del /q *.vshost.exe
del /q *.manifest
del /q *.xml
del /q *.deps.json
del /q *.config
if exist .notes rd /S /Q .notes

cd Plugins
del /q *.pdb
del /q *.vshost.exe
del /q *.manifest
del /q *.xml
del /q *.deps.json
cd ..

@REM ================ PACKAGE ================

cd ..
cd ..

if exist AlephNote.zip del AlephNote.zip

cd Data

7za.exe a .\..\AlephNote.zip .\..\Bin\Package\*

@REM ================ FINISHED ================

echo "Finished successfully"
