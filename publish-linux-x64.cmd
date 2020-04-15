@echo off
rmdir /s /q packages\linux-x64\httploadgen
mkdir packages\linux-x64\httploadgen
dotnet publish httploadgen --output packages\linux-x64\httploadgen -c Integration -r linux-x64 --self-contained false
del packages\linux-x64\httploadgen\*.deps.json
