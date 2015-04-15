@ECHO OFF
SET csc="%windir%\Microsoft.NET\Framework\v3.5\csc.exe"
IF NOT EXIST %csc% SET csc=SET csc="%windir%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
%csc% /target:exe hotlinesav.cs

%~n0.exe %*
goto :EOF
