@echo off
rmdir /s /q packages\linux-x64\httploadgen
mkdir packages\linux-x64\httploadgen
dotnet publish httploadgen --force --output packages\linux-x64\httploadgen -c Integration -r linux-x64 --no-self-contained
del packages\linux-x64\httploadgen\*.deps.json
