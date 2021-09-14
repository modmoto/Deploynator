# Deploynator

## Setup

- Install .net on the raspberry, this command worked wonders:
  - `wget -O - https://raw.githubusercontent.com/pjgpetecodes/dotnet5pi/master/install.sh | sudo bash`
- `git clone https://github.com/modmoto/Deploynator`
- `cd Deploynator`
- `bash ./InstallAndRunAsService.sh`

The Deploynator should be running on port 80 now and will be accessible in the network on its ip adress. To get the Ip, run `ping raspberry.local`