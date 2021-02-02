@ECHO OFF
@REM ================ BUILD ================

echo "[cd . ->] %cd%"

cd "%~dp0"
echo "[cd ~dp0 ->] %cd%"

cd ..
echo "[cd .. ->] %cd%"

if exist AlephNote.zip del AlephNote.zip

echo.

cd Bin
echo "[cd Bin ->] %cd%"

if exist Package rd /S /Q Package

xcopy Release\ Package\ /s /e /y

cd Package
echo "[cd Package ->] %cd%"

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
echo "[cd Plugins ->] %cd%"

del /q *.pdb
del /q *.vshost.exe
del /q *.manifest
del /q *.xml
del /q *.deps.json
cd ..

@REM ================ PACKAGE ================

cd ..
echo "[cd .. ->] %cd%"
cd ..
echo "[cd .. ->] %cd%"

if exist AlephNote.zip del AlephNote.zip

cd Data
echo "[cd Data ->] %cd%"

7za.exe a .\..\AlephNote.zip .\..\Bin\Package\*

cd ..
echo "[cd .. ->] %cd%"

@REM ================ FINISHED ================

echo "Finished successfully"
