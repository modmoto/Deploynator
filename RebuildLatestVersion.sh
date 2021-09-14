#!/bin/bash
git pull
sudo systemctl stop Deploynator
dotnet publish -c Release -o /srv/Deploynator
sudo systemctl start Deploynator