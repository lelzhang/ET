  
@echo off 
title Robot
cd ../Bin
@echo on
dotnet Robot.App.dll --AppType=Robot --Process=2 --Console=1

pause