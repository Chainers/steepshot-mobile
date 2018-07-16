cd ..\Sources\
@echo off

del *.user /S /Q 
rmdir /S /Q Steepshot\packages

FOR /R %%f IN (.) DO (
		if "%%~nf"=="obj" (
		rmdir /S /Q "%%f"
	)
	if "%%~nf"=="bin"	(	
		rmdir /S /Q "%%f"
	)
	if "%%~nf"=="Bin"	(	
		rmdir /S /Q "%%f"
	)
	if EXIST "ReSharper.*" (	
		rmdir /S /Q "_ReSharper.*"
	)
)