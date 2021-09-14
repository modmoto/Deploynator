#!/bin/bash
git pull
sudo mkdir /srv/Deploynator
sudo chown pi /srv/Deploynator
dotnet publish -c Release -o /srv/Deploynator
chmod +x /srv/Deploynator/Deploynator
sudo systemctl stop Deploynator
sudo cp Deploynator.service /etc/systemd/system/Deploynator.service
sudo systemctl daemon-reload
sudo systemctl start Deploynator