  
@echo off 
title StartServer
cd ../Bin
@echo on
dotnet Server.App.dll --Process=1 --Console=1

pause