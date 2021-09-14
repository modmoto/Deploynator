#!/bin/bash
git pull
sudo systemctl stop Deploynator
dotnet publish -c Release -r linux-arm  --self-contained=true -p:PublishSingleFile=true -p:GenerateRuntimeConfigurationFiles=true -o /srv/Deploynator
sudo systemctl start Deploynator