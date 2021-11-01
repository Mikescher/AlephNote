@ECHO OFF
@REM ================ BUILD ================

echo "[pwd] %cd%"

echo.

echo "cd ~dp0"
cd "%~dp0"
echo "[pwd] %cd%"
echo.

echo "cd .."
cd ..
echo "[pwd] %cd%"
echo.

echo "del AlephNote.zip"
if exist AlephNote.zip del AlephNote.zip
echo.

echo "cd Bin"
cd Bin
echo "[pwd] %cd%"
echo.

echo "rd Package"
if exist Package echo "(exists)"
if exist Package rd /S /Q Package
echo.

echo "xcopy Release Package"
xcopy /s /i Release Package
echo.

echo "cd Package"
cd Package
echo "[pwd] %cd%"
echo.

echo "rmdir [language dirs]"
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
echo.

echo "rename AlephNote.exe"
rename AlephNote.App.exe AlephNote.exe
echo.

echo "del symbols"
del /q *.pdb
del /q *.vshost.exe
del /q *.manifest
del /q *.xml
del /q *.deps.json
del /q *.config
echo.

echo "rd .notes"
if exist .notes rd /S /Q .notes
echo.

echo "cd Plugins"
cd Plugins
echo "[pwd] %cd%"
echo.

echo "del symbols"
del /q *.pdb
del /q *.vshost.exe
del /q *.manifest
del /q *.xml
del /q *.deps.json
echo.

echo "cd .."
cd ..
echo "[pwd] %cd%"
echo.

echo "copy AlephNote.exe.config"
copy ..\..\Data\AlephNote.exe.config .
echo.

@REM ================ PACKAGE ================

echo "cd .."
cd ..
echo "[pwd] %cd%"
echo.

echo "cd .."
cd ..
echo "[pwd] %cd%"
echo.


echo "del AlephNote.zip"
if exist AlephNote.zip del AlephNote.zip
echo.

echo "cd Data"
cd Data
echo "[cd Data ->] %cd%"
echo.

echo "7za AlephNote.zip"
7za.exe a .\..\AlephNote.zip .\..\Bin\Package\*
echo.

echo "cd .."
cd ..
echo "[pwd] %cd%"
echo.

@REM ================ FINISHED ================

echo "Finished successfully"
