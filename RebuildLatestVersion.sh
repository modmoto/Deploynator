#!/bin/bash
git pull
sudo systemctl stop Deploynator
dotnet publish -c Release -r linux-arm -o /srv/Deploynator
sudo systemctl start Deploynator