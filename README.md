# Deploynator

## Setup

- Install .net on the raspberry, this command worked wonders:
  - `wget -O - https://raw.githubusercontent.com/pjgpetecodes/dotnet5pi/master/install.sh | sudo bash`
- `git clone https://github.com/modmoto/Deploynator`
- `cd Deploynator`
- `bash ./InstallAsService.sh`

The Deploynator should be running on port 5000 now and will be accessible in the network on its ip adress. To get the Ip, run `ping raspberry.local`

To Rebuild:

`bash ./RebuildLatestVersion.sh`

## sounds
- https://ttsmp3.com/ the voice used is salli
- for the code to work, run `sudo apt install mpv -y` on the raspi first, or you will play the sounds in the browser
- install `sudo apt-get install mpg123` for the netcore audio to work
