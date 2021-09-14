#!/bin/bash
sudo mkdir /srv/Deploynator
sudo systemctl stop Deploynator
sudo chown pi /srv/Deploynator
dotnet publish -c Release -o /srv/Deploynator
chmod +x /srv/Deploynator/Deploynator
sudo cp Deploynator.service /etc/systemd/system/Deploynator.service
sudo systemctl daemon-reload
sudo systemctl start Deploynator
sudo systemctl enable Deploynator